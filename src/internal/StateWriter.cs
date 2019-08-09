using System;
using System.Threading;
using System.Collections.Generic;

using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        public sealed class StateWriter : SerializationFunctions
        {
            internal sealed class OT
            {
                readonly AbstractFieldDeclaration f;
                readonly Semaphore barrier;

                internal OT(AbstractFieldDeclaration f, Semaphore barrier)
                {
                    this.f = f;
                    this.barrier = barrier;
                }

                public void run()
                {
                    try
                    {
                        f.offset = 0;
                        SimpleChunk c = (SimpleChunk)f.lastChunk();
                        int i = (int)c.bpo;
                        f.osc(i, i + (int)c.count);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                        Console.Error.WriteLine("Offset calculation failed, resulting file will be corrupted.");
                        f.offset = long.MinValue;
                    }
                    finally
                    {
                        barrier.Release();
                    }
                }

            }

            public StateWriter(SkillState state, FileOutputStream @out) : base(state)
            {

                // ensure fast size() operations
                StoragePool<SkillObject, SkillObject>.fix(state.types);

                // make lbpo map, update data map to contain dynamic instances and
                // create skill IDs for serialization
                // index → bpo
                // @note pools.par would not be possible if it were an actual map:)
                int[] lbpoMap = new int[state.types.Count];
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
                                ((IBasePool)p).compress(lbpoMap);
                                barrier.Release();
                            }));
                        }
                    }
                    for (int i = 0; i < bases; i++)
                    {
                        barrier.WaitOne();
                    }
                }

                /// <summary>
                /// **************** PHASE 3: WRITE * ****************
                /// </summary>
                // write string block
                state.strings.prepareAndWrite(@out, this);

                // write count of the type block
                @out.v64(state.types.Count);

                // calculate offsets
                int fieldCount = 0;
                {
                    foreach (AbstractStoragePool p in state.types)
                    {
                        foreach (AbstractFieldDeclaration f in p.dataFields)
                        {
                            fieldCount++;
                            ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) =>
                            {
                                new OT(f, barrier).run();
                            }));
                        }
                    }
                    for (int i = 0; i < fieldCount; i++)
                    {
                        barrier.WaitOne();
                    }
                }

                // write types
                List<AbstractFieldDeclaration> fieldQueue = new List<AbstractFieldDeclaration>(fieldCount);
                foreach (AbstractStoragePool p in state.types)
                {
                    @out.v64(stringIDs[p.Name]);
                    long LCount = p.lastBlock().count;
                    @out.v64(LCount);
                    restrictions(p, @out);
                    if (null == p.superPool)
                    {
                        @out.i8((sbyte)0);
                    }
                    else
                    {
                        @out.v64(p.superPool.TypeID - 31);
                        if (0L != LCount)
                        {
                            @out.v64(lbpoMap[p.TypeID - 32]);
                        }
                    }

                    @out.v64(p.dataFields.Count);
                    fieldQueue.AddRange(p.dataFields);
                }

                // write fields
                List<Task> data = new List<Task>(fieldCount);
                long offset = 0L;
                foreach (AbstractFieldDeclaration f in fieldQueue)
                {
                    if (f.offset < 0)
                    {
                        throw new api.SkillException("aborting write because offset calculation failed");
                    }

                    // write info
                    @out.v64(f.index);
                    @out.v64(stringIDs[f.Name]);
                    writeType((FieldType)f.Type, @out);
                    restrictions(f, @out);
                    long end = offset + f.offset;
                    @out.v64(end);

                    // update last chunk and prepare write
                    Chunk c = f.lastChunk();
                    c.begin = offset;
                    c.end = end;
                    data.Add(new Task(f, offset, end));
                    offset = end;
                }

                writeFieldData(state, @out, data, (int)offset, barrier);
            }
        }
    }
}