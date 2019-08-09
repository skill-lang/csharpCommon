using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using SkillException = de.ust.skill.common.csharp.api.SkillException;
using FieldDeclaration = de.ust.skill.common.csharp.api.FieldDeclaration;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.restrictions;
using de.ust.skill.common.csharp.streams;
using de.ust.skill.common.csharp.api;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;
using System;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use FieldDeclaration with an unknown types T and Obj
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class AbstractFieldDeclaration : FieldDeclaration
        {

            /// <summary>
            /// regular field constructor
            /// </summary>
            protected AbstractFieldDeclaration(api.FieldType type, string name, AbstractStoragePool owner)
            {
                this.type = type;
                this.name = name;
                this.owner = owner;
                owner.dataFields.Add(this);
                index = owner.dataFields.Count;
            }

            /// <summary>
            /// auto field constructor
            /// </summary>
            protected AbstractFieldDeclaration(api.FieldType type, string name, int index, AbstractStoragePool owner)
            {
                this.type = type;

                this.name = name;
                this.owner = owner;

                this.index = index;
                owner.autoFields[-index] = (IAutoField)this;
            }

            /// <summary>
            /// skill name of this
            /// </summary>
            internal readonly string name;

            public abstract override string Name { get; }

            /// <summary>
            /// @note types may change during file parsing. this may seem like a hack,
            ///       but it makes file parser implementation a lot easier, because there
            ///       is no need for two mostly similar type hierarchy implementations
            /// </summary>
            public api.FieldType type;

            public abstract override api.FieldType Type { get; }

            public abstract override IGeneralAccess Owner { get; }

            /// <summary>
            /// index as used in the file
            /// 
            /// @note index is > 0, if the field is an actual data field
            /// @note index = 0, if the field is SKilLID (if supported by generator;
            ///       deprecated)
            /// @note index is smaller= 0, if the field is an auto field (or SKilLID)
            /// 
            /// @note fieldIDs should be file-global, so that remaining HashMaps with
            ///       field keys can be replaced
            /// </summary>
            internal readonly int index;

            /// <summary>
            /// the enclosing storage pool
            /// </summary>
            public AbstractStoragePool owner;

            public abstract void addRestriction(IFieldRestriction r);

            /// <summary>
            /// Check consistency of restrictions on this field.
            /// </summary>
            public abstract void check();

            internal readonly List<Chunk> dataChunks = new List<Chunk>();

            public abstract void addChunk(Chunk chunk);

            /// <summary>
            /// Make offsets absolute.
            /// </summary>
            /// <returns> the end of this chunk </returns>
            public abstract long addOffsetToLastChunk(FileInputStream @in, long offset);

            public abstract Chunk lastChunk();

            /// <summary>
            /// reset Chunks before writing a file
            /// </summary>
            public abstract void resetChunks(int lbpo, int newSize);

            /// <summary>
            /// Read data from a mapped input stream and set it accordingly. This is
            /// invoked at the very end of state construction and done massively in
            /// parallel.
            /// </summary>
            public abstract void rsc(int i, int end, MappedInStream @in);

            /// <summary>
            /// Read data from a mapped input stream and set it accordingly. This is
            /// invoked at the very end of state construction and done massively in
            /// parallel.
            /// </summary>
            public abstract void rbc(BulkChunk target, MappedInStream @in);

            /// <summary>
            /// offset cache; calculated by osc/obc; reset is done by the caller
            /// (simplifies obc)
            /// </summary>
            public long offset;

            /// <summary>
            /// offset calculation as preparation of writing data belonging to the owners
            /// last block
            /// </summary>
            public abstract void osc(int i, int end);

            /// <summary>
            /// offset calculation as preparation of writing data belonging to the owners
            /// last block
            /// </summary>
            public abstract void obc(BulkChunk c);

            /// <summary>
            /// write data into a map at the end of a write/append operation
            /// 
            /// @note this will always write the last chunk, as, in contrast to read, it
            ///       is impossible to write to fields in parallel
            /// @note only called, if there actually is field data to be written
            /// </summary>
            public abstract void wsc(int i, int end, MappedOutStream @out);

            /// <summary>
            /// write data into a map at the end of a write/append operation
            /// 
            /// @note this will always write the last chunk, as, in contrast to read, it
            ///       is impossible to write to fields in parallel
            /// @note only called, if there actually is field data to be written
            /// </summary>
            public abstract void wbc(BulkChunk c, MappedOutStream @out);

            /// <summary>
            /// Coordinates reads and prevents from state corruption using the barrier.
            /// </summary>
            /// <param name="barrier"> takes one permit in the caller thread and returns one in the
            ///            reader thread (per block) </param>
            /// <param name="readErrors"> errors will be reported in this queue </param>
            /// <returns> number of jobs started </returns>
            public abstract int finish(Semaphore barrier, List<SkillException> readErrors, FileInputStream @in);

            /// <summary>
            /// punch a hole into the type system that eases implementation of maps
            /// of interfaces
            /// 
            /// @note the hole is only necessary, because interfaces cannot inherit from classes
            /// </summary>
            protected internal static Dictionary<K1, V1> castMap<K1, V1, K2, V2>(Dictionary<K2, V2> arg)
            {
                IEnumerable<DictionaryEntry> CastDict(IDictionary d)
                {
                    foreach (DictionaryEntry entry in d)
                    {
                        yield return entry;
                    }
                }

                if (arg == null)
                {
                    return null;
                }

                return CastDict(arg).ToDictionary(entry => (K1)entry.Key, entry => (V1)entry.Value);
            }
        }
    }
}