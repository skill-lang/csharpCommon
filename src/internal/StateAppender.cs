using System;
using System.Collections.Generic;
using System.Threading;

using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Implementation of append operation.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class StateAppender : SerializationFunctions
        {

            public StateAppender(SkillState state, FileOutputStream @out) : base(state)
            {

                // save the index of the first new pool
                int newPoolIndex;
                {
                    int i = 0;
                    foreach (AbstractStoragePool t in state.types)
                    {
                        if (t.Blocks.Count == 0)
                        {
                            break;
                        }
                        i++;
                    }
                    newPoolIndex = i;
                }

                // ensure fast size() operations
                StoragePool<SkillObject, SkillObject>.fix(state.types);

                // make lbpsi map, update data map to contain dynamic instances and
                // create serialization skill IDs for
                // serialization
                // index → bpsi
                int[] lbpoMap = new int[state.types.Count];
                Dictionary<AbstractFieldDeclaration, Chunk> chunkMap = new Dictionary<AbstractFieldDeclaration, Chunk>();
                Semaphore barrier = new Semaphore(0, Int32.MaxValue);
                {
                    int bases = 0;
                    foreach (AbstractStoragePool p in state.types)
                    {
                        if (p is IBasePool)
                        {
                            bases++;
                            ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) =>
                            {
                                ((IBasePool)p).prepareAppend(lbpoMap, chunkMap);
                                barrier.Release();
                            }));
                        }
                    }
                    for (int i = 0; i < bases; i++)
                    {
                        barrier.WaitOne();
                    }
                }

                // locate relevant pools
                List<AbstractStoragePool> rPools = new List<AbstractStoragePool>(state.types.Count);
                foreach (AbstractStoragePool p in state.types)
                {
                    // new index?
                    if (p.TypeID - 32 >= newPoolIndex)
                    {
                        rPools.Add(p);
                    }
                    // new instance or field?
                    else if (p.size() > 0)
                    {
                        bool exists = false;
                        foreach (AbstractFieldDeclaration f in p.dataFields)
                        {
                            if (chunkMap.ContainsKey(f))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (exists)
                        {
                            rPools.Add(p);
                        }
                    }
                }

                /// <summary>
                /// **************** PHASE 3: WRITE * ****************
                /// </summary>

                // write string block
                state.strings.prepareAndAppend(@out, this);

                // calculate offsets for relevant fields
                int fieldCount = 0;
                {
                    foreach (AbstractFieldDeclaration f in chunkMap.Keys)
                    {
                        fieldCount++;
                        ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) =>
                        {
                            f.offset = 0;
                            Chunk c = f.lastChunk();
                            if (c is SimpleChunk)
                            {
                                int i = (int)((SimpleChunk)c).bpo;
                                f.osc(i, i + (int)c.count);
                            }
                            else
                            {
                                f.obc((BulkChunk)c);
                            }
                            barrier.Release();
                        }));
                    }
                    for (int i = 0; i < fieldCount; i++)
                    {
                        barrier.WaitOne();
                    }
                }

                // write count of the type block
                @out.v64(rPools.Count);

                // write headers
                List<List<AbstractFieldDeclaration>> fieldQueue = new List<List<AbstractFieldDeclaration>>(fieldCount);
                foreach (AbstractStoragePool p in rPools)
                {
                    // generic append
                    bool newPool = p.TypeID - 32 >= newPoolIndex;
                    List<AbstractFieldDeclaration> fields = new List<AbstractFieldDeclaration>(p.dataFields.Count);
                    foreach (AbstractFieldDeclaration f in p.dataFields)
                    {
                        if (chunkMap.ContainsKey(f))
                        {
                            fields.Add(f);
                        }
                    }

                    if (newPool || (0 != fields.Count && p.size() > 0))
                    {
                        @out.v64(stringIDs[p.Name]);
                        long count = p.lastBlock().count;
                        @out.v64(count);

                        if (newPool)
                        {
                            restrictions(p, @out);
                            if (null == p.superName())
                            {
                                @out.i8((sbyte)0);
                            }
                            else
                            {
                                @out.v64(p.superPool.TypeID - 31);
                                if (0 != count)
                                {
                                    // we used absolute BPOs to ease overall
                                    // implementation
                                    @out.v64(lbpoMap[p.TypeID - 32] - lbpoMap[((AbstractStoragePool)p.basePool).TypeID - 32]);
                                }

                            }
                        }
                        else if (null != p.superName() && 0 != count)
                        {
                            @out.v64(lbpoMap[p.TypeID - 32] - lbpoMap[((AbstractStoragePool)p.basePool).TypeID - 32]);
                        }

                        if (newPool && 0 == count)
                        {
                            @out.i8(0);
                        }
                        else
                        {
                            @out.v64(fields.Count);
                            fieldQueue.Add(fields);
                        }
                    }
                }

                // write fields
                List<Task> data = new List<Task>(fieldCount);
                long offset = 0L;
                foreach (List<AbstractFieldDeclaration> fields in fieldQueue)
                {
                    foreach (AbstractFieldDeclaration f in fields)
                    {
                        @out.v64(f.index);

                        if (1 == f.dataChunks.Count)
                        {
                            @out.v64(stringIDs[f.Name]);
                            writeType((FieldType)f.Type, @out);
                            restrictions(f, @out);
                        }

                        // put end offset and enqueue data
                        long end = offset + f.offset;
                        @out.v64(end);
                        data.Add(new Task(f, offset, end));
                        offset = end;
                    }
                }

                writeFieldData(state, @out, data, (int)offset, barrier);
            }
        }
    }
}