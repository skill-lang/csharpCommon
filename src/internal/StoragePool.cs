using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.api;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Top level implementation of all storage pools.
        /// 
        /// @author Simon Glaub, Timm Felden 
        /// @note Storage pools must be created in type order!
        /// @note We do not guarantee functional correctness if instances from multiple
        ///       skill files are mixed. Such usage will likely break at least one of the
        ///       files. 
        /// </summary>
        /// <param> T: static type of instances </param>
        /// <param> B: base type of this hierarchy </param>

        public class StoragePool<T, B> : AbstractStoragePool, Access<T>, ReferenceType where T : B where B : SkillObject
        {

            public override StaticFieldIterator fields()
            {
                return new StaticFieldIterator(this);
            }

            public override FieldIterator allFields()
            {
                return new FieldIterator(this);
            }

            /// <summary>
            /// internal use only!
            /// </summary>
            public override List<Block> Blocks
            {
                get
                {
                    return blocks;
                }
            }

            /// <summary>
            /// internal use only!
            /// </summary>
            public override Block lastBlock()
            {
                return blocks[blocks.Count - 1];
            }

            /// <summary>
            /// retrieve a new object
            /// </summary>
            /// <param name="index"> in [0;#newObjectsSize()[ </param>
            /// <returns> the new object at the given position </returns>
            public override object newObject(int index)
            {
                return newObjects[index];
            }

            /// <summary>
            /// Ensures that at least capacity many new objects can be stored in this
            /// pool without moving references.
            /// </summary>
            public override void hintNewObjectsSize(int capacity)
            {
                newObjects.Capacity = capacity;
            }

            internal DynamicNewInstancesIterator<T, B> newDynamicInstances()
            {
                return new DynamicNewInstancesIterator<T, B>(this);
            }

            internal override int newDynamicInstancesSize()
            {
                int rval = 0;
                TypeHierarchyIterator<T, B> ts = new TypeHierarchyIterator<T, B>(this);
                while (ts.hasNext())
                {
                    rval += ts.next().newObjects.Count;
                }

                return rval;
            }

            /// <summary>
            /// the number of instances of exactly this type, excluding sub-types
            /// </summary>
            /// <returns> size excluding subtypes </returns>
            public override int staticSize()
            {
                return staticDataInstances + newObjects.Count;
            }

            /// <summary>
            ///*
            /// @note cast required to work around type system
            /// </summary>
            public StaticDataIterator<T> staticInstances()
            {
                return new StaticDataIterator<T>(this);
            }

            /// <summary>
            /// !!internal use only!!
            /// </summary>
            public override bool Fixed
            {
                get
                {
                    return @fixed;
                }
            }

            public override string Name
            {
                get
                {
                    return name;
                }
            }

            public override string superName()
            {
                if (null != superPool)
                {
                    return superPool.name;
                }
                return null;
            }

            /// <summary>
            /// @note the unchecked cast is required, because we can not supply this as
            ///       an argument in a super constructor, thus the base pool can not be
            ///       an argument to the constructor. The cast will never fail anyway.
            /// </summary>
            protected StoragePool(int poolIndex, string name, AbstractStoragePool superPool, string[] knownFields, IAutoField[] autoFields) : base(poolIndex, name, superPool, knownFields, autoFields)
            {
            }

            /// <returns> the instance matching argument skill id </returns>
            public override object getByID(int ID)
            {
                int index = ID - 1;
                if (index < 0 | data.Length <= index)
                {
                    return null;
                }
                return (T)data[index];
            }

            public override object readSingleField(InStream @in)
            {
                int index = @in.v32() - 1;
                if (index < 0 | data.Length <= index)
                {
                    return null;
                }
                return (T)data[index];
            }

            public override long calculateOffset(ICollection xs)
            {
                // shortcut small compressed types
                if (data.Length < 128)
                {
                    return xs.Count;
                }

                long result = 0L;
                foreach (T x in xs)
                {
                    result += null == x ? 1 : V64.singleV64Offset(x.skillID);
                }
                return result;
            }

            public override long singleOffset(object x)
            {
                if (null == x)
                {
                    return 1L;
                }

                int v = ((T)x).skillID;
                if (0 == (v & 0xFFFFFF80))
                {
                    return 1;
                }
                else if (0 == (v & 0xFFFFC000))
                {
                    return 2;
                }
                else if (0 == (v & 0xFFE00000))
                {
                    return 3;
                }
                else if (0 == (v & 0xF0000000))
                {
                    return 4;
                }
                else
                {
                    return 5;
                }
            }

            public override void writeSingleField(object @ref, OutStream @out)
            {
                if (null == @ref)
                {
                    @out.i8((sbyte)0);
                }
                else
                {
                    @out.v64(((T)@ref).skillID);
                }
            }

            /// <returns> size including subtypes </returns>
            public override int size()
            {
                if (@fixed)
                {
                    return cachedSize;
                }

                int size = 0;
                TypeHierarchyIterator<T, B> ts = new TypeHierarchyIterator<T, B>(this);
                while (ts.hasNext())
                {
                    size += ts.next().staticSize();
                }
                return size;
            }

            public override Stream stream()
            {
                Stream stream = new MemoryStream();
                foreach (var item in this)
                {
                    stream.WriteByte((byte)item);
                }
                return stream;
            }

            public virtual T[] toArray(T[] a)
            {
                T[] rval = new T[size()];
                Array.Copy(a, rval, size());
                DynamicDataIterator<T, B> @is = (DynamicDataIterator < T, B >)iterator();
                for (int i = 0; i < rval.Length; i++)
                {
                    rval[i] = (T)@is.next();
                }
                return rval;
            }

            /// <summary>
            /// Add an existing instance as a new objects.
            /// 
            /// @note Do not use objects managed by other skill files.
            /// </summary>
            public override void add(object e)
            {
                if (@fixed)
                {
                    throw new System.InvalidOperationException("can not fix a pool that contains new objects");
                }

                newObjects.Add((T)e);
            }

            /// <summary>
            /// Delete shall only be called from skill state
            /// </summary>
            /// <param name="target"> the object to be deleted </param>
            public override void delete(SkillObject target)
            {
                if (!target.isDeleted())
                {
                    target.skillID = 0;
                    deletedCount++;
                }
            }

            public override SkillFile Owner
            {
                get
                {
                    return basePool.Owner;
                }
                set
                {
                    basePool.Owner = value;
                }
            }

            public override IEnumerable iterator()
            {
                return new DynamicDataIterator<T, B>(this);
            }

            public override IEnumerator GetEnumerator()
            {
                return new DynamicDataIterator<T, B>(this).GetEnumerator();
            }

            public IEnumerable typeOrderIterator()
            {
                return new TypeOrderIterator<T, B>(this);
            }

            public override object make()
            {
                throw new SkillException("We prevent reflective creation of new instances, because it is bad style!");
            }

            /// <summary>
            /// insert new T instances with default values based on the last block info
            /// 
            /// @note defaults to unknown objects to reduce code size
            /// </summary>
            public override void allocateInstances(Block last)
            {
                int i = last.bpo;
                int high = i + last.staticCount;
                while (i < high)
                {
                    data[i] = (B)(SkillObject)(new SkillObject.SubType(this, i + 1));
                    i += 1;
                }
            }

            internal override void updateAfterCompress(int[] lbpoMap)
            {
                // update data
                data = ((BasePool<B>)basePool).data;

                // update structural knowledge of data
                staticDataInstances += newObjects.Count - deletedCount;
                deletedCount = 0;
                newObjects.Clear();
                newObjects.TrimExcess();

                blocks.Clear();
                blocks.Add(new Block(lbpoMap[TypeID - 32], cachedSize, staticDataInstances));
            }

            /// <summary>
            /// internal use only! adds an unknown field
            /// </summary>
            public override AbstractFieldDeclaration addField<R>(FieldType type, string name)
            {
                return new LazyField<R, T>(type, name, this);
            }

            /// <summary>
            /// used internally for state allocation
            /// </summary>
            public override void addKnownField(string name, StringType @string, Annotation annotation)
            {
                throw new Exception("Arbitrary storage pools know no fields!");
            }

            /// <summary>
            /// used internally for type forest construction
            /// </summary>
            public override AbstractStoragePool makeSubPool(int index, string name)
            {
                return new StoragePool<T, B>(index, name, this, noKnownFields, NoAutoFields);
            }

            /// <summary>
            /// called after a prepare append operation to write empty the new objects
            /// buffer and to set blocks correctly
            /// </summary>
            internal override void updateAfterPrepareAppend(int[] lbpoMap, Dictionary<AbstractFieldDeclaration, Chunk> chunkMap)
            {
                // update data as it may have changed
                this.data = ((BasePool<B>)basePool).data;

                bool newInstances = newDynamicInstances().hasNext();
                bool newPool = blocks.Count == 0;
                bool newField;
                {
                    bool exists = false;
                    foreach (AbstractFieldDeclaration f in dataFields)
                    {
                        if (0 == f.dataChunks.Count)
                        {
                            exists = true;
                            break;
                        }
                    }

                    newField = exists;
                }

                if (newPool || newInstances || newField)
                {

                    // build block chunk
                    int lcount = newDynamicInstancesSize();
                    // //@ note this is the index into the data array and NOT the
                    // written lbpo
                    int lbpo = (0 == lcount) ? 0 : lbpoMap[TypeID - 32];

                    blocks.Add(new Block(lbpo, lcount, newObjects.Count));
                    int blockCount = blocks.Count;
                    staticDataInstances += newObjects.Count;

                    // @note: if this does not hold for p; then it will not hold for
                    // p.subPools either!
                    if (newInstances || !newPool)
                    {
                        // build field chunks
                        foreach (AbstractFieldDeclaration f in dataFields)
                        {
                            if (0 == f.index)
                            {
                                continue;
                            }

                            Chunk c;
                            if (0 == f.dataChunks.Count && 1 != blockCount)
                            {
                                c = new BulkChunk(-1, -1, cachedSize, blockCount);
                            }
                            else if (newInstances)
                            {
                                c = new SimpleChunk(-1, -1, lbpo, lcount);
                            }
                            else
                            {
                                continue;
                            }

                            f.addChunk(c);
                            lock (chunkMap)
                            {
                                chunkMap[f] = c;
                            }
                        }
                    }
                }

                // remove new objects, because they are regular objects by now
                newObjects.Clear();
                newObjects.TrimExcess();
            }

            public override sealed string ToString()
            {
                return name;
            }

            public override api.FieldType cast<K, V>()
            {
                throw new NotImplementedException();
            }
        }
    }
}