using System;
using System.IO;
using System.Collections.Generic;

using SkillException = de.ust.skill.common.csharp.api.SkillException;
using Mode = de.ust.skill.common.csharp.api.Mode;
using de.ust.skill.common.csharp.@internal.exceptions;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.restrictions;
using de.ust.skill.common.csharp.streams;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// The parser implementation is based on the denotational semantics given in
        /// TR14§6.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class FileParser
        {
            private sealed class LFEntry
            {
                public readonly AbstractStoragePool p;
                public readonly int count;

                public LFEntry(AbstractStoragePool p, int count)
                {
                    this.p = p;
                    this.count = count;
                }
            }

            protected FileInputStream @in;

            // ERROR REPORTING
            protected int blockCounter = 0;
            protected HashSet<string> seenTypes = new HashSet<string>();

            // this barrier is strictly increasing inside of each block and reset to 0
            // at the beginning of each block
            protected int blockIDBarrier = 0;

            // strings
            protected readonly StringPool Strings;

            // types
            protected readonly List<AbstractStoragePool> types;
            protected readonly Dictionary<string, AbstractStoragePool> poolByName = new Dictionary<string, AbstractStoragePool>();
            protected readonly Annotation annotation;
            protected readonly StringType stringType;

            /// <summary>
            /// creates a new storage pool of matching name
            /// 
            /// @note implementation depends heavily on the specification
            /// </summary>
            protected abstract AbstractStoragePool newPool(string name, AbstractStoragePool superPool, HashSet<TypeRestriction> restrictions);

            protected FileParser(FileInputStream @in, int IRSize)
            {
                types = new List<AbstractStoragePool>(IRSize);
                this.@in = @in;
                Strings = new StringPool(@in);
                stringType = new StringType(Strings);
                annotation = new Annotation(types);

                //System.Diagnostics.Debug.WriteLine("Length: " + @in.input.BaseStream.Length);
                // parse blocks
                while (!@in.eof())
                {
                    //System.Diagnostics.Debug.WriteLine("String Position: " + @in.input.BaseStream.Position);
                    stringBlock();
                    //System.Diagnostics.Debug.WriteLine("Type Position: " + @in.input.BaseStream.Position);
                    typeBlock();
                }
            }

            protected void stringBlock()
            {
                try
                {
                    int count = @in.v32();
                    if (0 != count)
                    {
                        // read offsets
                        int[] offsets = new int[count];
                        for (int i = 0; i < count; i++)
                        {
                            offsets[i] = @in.i32();
                        }

                        // store offsets
                        int last = 0;
                        for (int i = 0; i < count; i++)
                        {
                            Strings.stringPositions.Add(new StringPool.Position(@in.position() + last, offsets[i] - last));
                            Strings.idMap.Add(null);
                            last = offsets[i];
                        }
                        @in.jump(@in.position() + last);
                    }
                }
                catch (Exception e)
                {
                    throw new ParseException(@in, blockCounter, e, "corrupted string block");
                }
            }

            // deferred pool resize requests
            private readonly List<AbstractStoragePool> resizeQueue = new List<AbstractStoragePool>();
            // pool ⇒ local field count
            private readonly List<LFEntry> localFields = new List<LFEntry>();

            // field data updates: pool x fieldID
            private readonly List<DataEntry> fieldDataQueue = new List<DataEntry>();

            private sealed class DataEntry
            {
                public DataEntry(AbstractStoragePool owner, int fieldID)
                {
                    this.owner = owner;
                    this.fieldID = fieldID;
                }

                internal readonly AbstractStoragePool owner;
                internal readonly int fieldID;
            }

            private long offset = 0L;

            /// <summary>
            /// Turns a field type into a preliminary type information. In case of user
            /// types, the declaration of the respective user type may follow after the
            /// field declaration.
            /// </summary>
            public virtual FieldType fieldType()
            {
                int typeID = @in.v32();
                switch (typeID)
                {
                    case 0:
                        return new ConstantI8(@in.i8());
                    case 1:
                        return new ConstantI16(@in.i16());
                    case 2:
                        return new ConstantI32(@in.i32());
                    case 3:
                        return new ConstantI64(@in.i64());
                    case 4:
                        return new ConstantV64(@in.v64());
                    case 5:
                        return annotation;
                    case 6:
                        return BoolType.get();
                    case 7:
                        return I8.get();
                    case 8:
                        return I16.get();
                    case 9:
                        return I32.get();
                    case 10:
                        return I64.get();
                    case 11:
                        return V64.get();
                    case 12:
                        return F32.get();
                    case 13:
                        return F64.get();
                    case 14:
                        return stringType;
                    case 15:
                        return new ConstantLengthArray<object>(@in.v32(), fieldType());
                    case 17:
                        return new VariableLengthArray<object>(fieldType());
                    case 18:
                        return new ListType<object>(fieldType());
                    case 19:
                        return new SetType<object>(fieldType());
                    case 20:
                        return new MapType<object, object>(fieldType(), fieldType());
                    default:
                        if (typeID >= 32)
                        {
                            return types[typeID - 32];
                        }

                        throw new ParseException(@in, blockCounter, null, "Invalid type ID: {0:D}", typeID);
                }
            }

            private HashSet<TypeRestriction> typeRestrictions()
            {
                HashSet<TypeRestriction> rval = new HashSet<TypeRestriction>();
                // parse count many entries
                for (int i = @in.v32(); i != 0; i--)
                {
                    int id = @in.v32();
                    switch (id)
                    {
                        case 0:
                        // Unique
                        case 1:
                        // Singleton
                        case 2:
                            // Monotone
                            break;

                        default:
                            if (id <= 5 || 1 == (id % 2))
                            {
                                throw new ParseException(@in, blockCounter, null, "Found unknown type restriction {0:D}. Please regenerate your binding, if possible.", id);
                            }
                            Console.Error.WriteLine("Skiped unknown skippable type restriction. Please update the SKilL implementation.");
                            break;
                    }
                }
                return rval;
            }

            private HashSet<IFieldRestriction> fieldRestrictions(FieldType t)
            {
                HashSet<IFieldRestriction> rval = new HashSet<IFieldRestriction>();
                for (int count = @in.v32(); count != 0; count--)
                {
                    int id = @in.v32();
                    switch (id)
                    {

                        case 0:
                            {
                                if (t is ReferenceType)
                                {
                                    rval.Add(NonNull.get());
                                }
                                else
                                {
                                    throw new ParseException(@in, blockCounter, null, "Nonnull restriction on non-refernce type: {0}.", t.ToString());
                                }
                                break;
                            }

                        case 1:
                            {
                                // default
                                if (t is ReferenceType)
                                {
                                    // TODO typeId -> ref
                                    @in.v32();
                                }
                                else
                                {
                                    // TODO other values
                                    (t).readSingleField(@in);
                                }
                                break;
                            }

                        case 3:
                            {
                                IFieldRestriction r = Range.make(t.TypeID, @in);
                                if (null == r)
                                {
                                    throw new ParseException(@in, blockCounter, null, "Type {0} can not be range restricted!", t.ToString());
                                }
                                rval.Add(r);
                                break;
                            }

                        case 5:
                            {
                                // TODO coding
                                // string.get
                                @in.v32();
                                break;
                            }

                        case 7:
                            {
                                // TODO CLP
                                break;
                            }

                        case 9:
                            {
                                for (int c = @in.v32(); c != 0; c--)
                                {
                                    // type IDs
                                    @in.v32();
                                }
                                break;
                            }

                        default:
                            if (id <= 9 || 1 == (id % 2))
                            {
                                throw new ParseException(@in, blockCounter, null, "Found unknown field restriction {0:D}. Please regenerate your binding, if possible.", id);
                            }
                            Console.Error.WriteLine("Skipped unknown skippable type restriction. Please update the SKilL implementation.");
                            break;
                    }
                }
                return rval;
            }

            private void typeDefinition()
            {
                // read type part
                string name;
                try
                {
                    string n = Strings.get(@in.v32());
                    name = n;
                }
                catch (InvalidPoolIndexException e)
                {
                    throw new ParseException(@in, blockCounter, e, "corrupted type header");
                }
                if (null == name)
                {
                    throw new ParseException(@in, blockCounter, null, "corrupted file: nullptr in typename");
                }

                // type duplication error detection
                if (seenTypes.Contains(name))
                {
                    throw new ParseException(@in, blockCounter, null, "Duplicate definition of type {0}", name);
                }
                seenTypes.Add(name);

                // try to parse the type definition
                try
                {
                    int count = @in.v32();

                    AbstractStoragePool definition = null;
                    if (poolByName.ContainsKey(name))
                    {
                        definition = (AbstractStoragePool)poolByName[name];
                    }
                    else
                    {
                        // restrictions
                        HashSet<TypeRestriction> rest = typeRestrictions();
                        // super
                        AbstractStoragePool superDef;
                        {
                            int superID = @in.v32();
                            if (0 == superID)
                            {
                                superDef = null;
                            }
                            else if (superID > types.Count)
                            {
                                throw new ParseException(@in, blockCounter, null, "Type {0} refers to an ill-formed super type.\n" + "          found: {1:D}; current number of other types {2:D}", name, superID, types.Count);
                            }
                            else
                            {
                                superDef = (AbstractStoragePool)types[superID - 1];
                            }
                        }

                        // allocate pool
                        try
                        {
                            definition = (AbstractStoragePool)newPool(name, superDef, rest);
                            if (definition.superPool != superDef)
                            {
                                throw new ParseException(@in, blockCounter, null, "The file contains a super type {0} but {1} is specified to be a base type.", superDef == null ? "<none>" : superDef.Name, name);
                            }
                            poolByName[name] = definition;
                        }
                        catch (System.InvalidCastException e)
                        {
                            throw new ParseException(@in, blockCounter, e, "The super type of {0} stored in the file does not match the specification!", name);
                        }
                    }
                    if (blockIDBarrier < definition.TypeID)
                    {
                        blockIDBarrier = definition.TypeID;
                    }
                    else
                    {
                        throw new ParseException(@in, blockCounter, null, "Found unordered type block. Type {0} has id {1:D}, barrier was {2:D}.", name, definition.TypeID, blockIDBarrier);
                    }

                    // in contrast to prior implementation, bpo is the position inside
                    // of data, even if there are no actual
                    // instances. We need this behavior, because that way we can cheaply
                    // calculate the number of static instances
                    int bpo = definition.basePool.getCachedSize() + (null == definition.superPool ? 0 : (0 != count ? (int)@in.v64() : definition.superPool.lastBlock().bpo));

                    // ensure that bpo is in fact inside of the parents block
                    if (null != definition.superPool)
                    {
                        Block b = definition.superPool.lastBlock();
                        if (bpo < b.bpo || b.bpo + b.count < bpo)
                        {
                            throw new ParseException(@in, blockCounter, null, "Found broken bpo.");
                        }
                    }

                    // static count and cached size are updated in the resize phase
                    // @note we assume that all dynamic instance are static instances as
                    // well, until we know for sure
                    definition.Blocks.Add(new Block(bpo, count, count));
                    definition.staticDataInstances += count;

                    // prepare resize
                    resizeQueue.Add(definition);

                    localFields.Add(new LFEntry(definition, (int)@in.v64()));
                }
                catch (IOException e)
                {
                    throw new ParseException(@in, blockCounter, e, "unexpected end of file");
                }
            }

            protected void typeBlock()
            {
                // reset counters and queues
                seenTypes.Clear();
                blockIDBarrier = 0;
                resizeQueue.Clear();
                localFields.Clear();
                fieldDataQueue.Clear();
                offset = 0L;

                // parse type
                for (int count = @in.v32(); count != 0; count--)
                {
                    typeDefinition();
                }

                // resize pools by updating cachedSize and staticCount
                // @note instances will be allocated just before field deserialization
                foreach (AbstractStoragePool p in resizeQueue)
                {
                    Block b = p.lastBlock();
                    p.cachedSize += b.count;

                    if (0 != b.count)
                    {
                        // calculate static count of our parent
                        AbstractStoragePool parent = p.superPool;
                        if (null != parent)
                        {
                            Block sb = parent.lastBlock();
                            // assumed static instances, minus what static instances
                            // would be, if p were the first sub pool.
                            int delta = sb.staticCount - (b.bpo - sb.bpo);
                            // if positive, then we have to subtract it from the assumed
                            // static count (local and global)
                            if (delta > 0)
                            {
                                sb.staticCount -= delta;
                                parent.staticDataInstances -= delta;
                            }
                        }
                    }
                }

                // parse fields
                foreach (LFEntry lfe in localFields)
                {
                    AbstractStoragePool p = lfe.p;

                    // read field part
                    int legalFieldIDBarrier = 1 + p.dataFields.Count;

                    Block lastBlock = p.Blocks[p.Blocks.Count - 1];

                    for (int fieldCounter = lfe.count; fieldCounter != 0; fieldCounter--)
                    {
                        int ID = @in.v32();
                        if (ID > legalFieldIDBarrier || ID <= 0)
                        {
                            throw new ParseException(@in, blockCounter, null, "Found an illegal field ID: {0:D}", ID);
                        }

                        long end;
                        if (ID == legalFieldIDBarrier)
                        {
                            // new field
                            string fieldName = Strings.get(@in.v32());
                            if (null == fieldName)
                            {
                                throw new ParseException(@in, blockCounter, null, "corrupted file: nullptr in fieldname");
                            }

                            FieldType t = fieldType();
                            HashSet<IFieldRestriction> rest = fieldRestrictions(t);
                            end = @in.v64();
                          
                            try
                            {
                                AbstractFieldDeclaration f;
                                // try to allocate simple chunks, because working on
                                // them is cheaper
                                switch (t.TypeID)
                                {
                                    case 14:
                                        f = p.addField<string>(t, fieldName);
                                        break;
                                    case 6:
                                        f = p.addField<bool>(t, fieldName);
                                        break;
                                    case 0:
                                    case 7:
                                        f = p.addField<sbyte>(t, fieldName);
                                        break;
                                    case 1:
                                    case 8:
                                        f = p.addField<short>(t, fieldName);
                                        break;
                                    case 2:
                                    case 9:
                                        f = p.addField<int>(t, fieldName);
                                        break;
                                    case 3:
                                    case 4:
                                    case 10:
                                    case 11:
                                        f = p.addField<long>(t, fieldName);
                                        break;
                                    case 12:
                                        f = p.addField<float>(t, fieldName);
                                        break;
                                    case 13:
                                        f = p.addField<double>(t, fieldName);
                                        break;
                                    case 5:
                                        f = p.addField<SkillObject>(t, fieldName);
                                        break;
                                    default:
                                        f = p.addField<object>(t, fieldName);
                                        break;
                                }

                                foreach (IFieldRestriction r in rest)
                                {
                                    f.addRestriction(r);
                                }
                                f.addChunk(1 == p.Blocks.Count ? (Chunk)new SimpleChunk(offset, end, lastBlock.bpo, lastBlock.count)
                                        : new BulkChunk(offset, end, p.cachedSize, p.Blocks.Count));
                            }
                            catch (SkillException e)
                            {
                                // transform to parse exception with propper values
                                throw new ParseException(@in, blockCounter, null, e.Message);
                            }
                            legalFieldIDBarrier += 1;

                        }
                        else
                        {
                            // field already seen
                            end = @in.v64();
                            p.dataFields[ID - 1].addChunk(new SimpleChunk(offset, end, lastBlock.bpo, lastBlock.count));

                        }
                        offset = end;
                        fieldDataQueue.Add(new DataEntry(p, ID));
                    }
                }

                processFieldData();
            }

            private void processFieldData()
            {
                // we have to add the file offset to all begins and ends we encounter
                long fileOffset = @in.position();
                long dataEnd = fileOffset;

                // process field data declarations in order of appearance and update
                // offsets to absolute positions
                foreach (DataEntry e in fieldDataQueue)
                {
                    AbstractFieldDeclaration f = e.owner.dataFields[e.fieldID - 1];

                    // make begin/end absolute
                    long end = f.addOffsetToLastChunk(@in, fileOffset);
                    dataEnd = Math.Max(dataEnd, end);
                }
                @in.jump(dataEnd);
            }

            /// <summary>
            /// helper for pool creation in generated code; optimization for all pools
            /// that do not have auto fields
            /// </summary>
            protected internal static IAutoField[] noAutoFields<T>() where T : SkillObject
            {
                return StoragePool<SkillObject, SkillObject>.NoAutoFields;
            }

            /// <summary>
            /// creates a matching skill state out of this file parser's state
            /// </summary>
            public virtual State read<State>(Type cls, Mode writeMode) where State : SkillState
            {
                // the generated state has exactly one constructor
                try
                {
                    State r = (State)cls.GetConstructors()[0].Invoke(new object[] { poolByName, Strings, stringType, annotation, types, @in, writeMode });

                    r.check();
                    return r;
                }
                catch (SkillException e)
                {
                    throw new ParseException(@in, blockCounter, e, "Post serialization check failed!");
                }
                catch (Exception e)
                {
                    throw new ParseException(@in, blockCounter, e, "State instantiation failed!");
                }
            }
        }
    }
}