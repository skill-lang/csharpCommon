using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using StringAccess = de.ust.skill.common.csharp.api.StringAccess;
using de.ust.skill.common.csharp.@internal.exceptions;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// @author Simon Glaub, Timm Felden
        /// @note String pools use magic index 0 for faster translation of string ids to
        ///       strings.
        /// @note String pool may contain duplicates, if strings have been added. This is
        ///       a necessary behavior, if add should be an O(1) operation and Strings
        ///       are loaded from file lazily.
        /// </summary>
        public class StringPool : StringAccess
        {
            private FileInputStream input;

            /// <summary>
            /// the set of all known strings, i.e. strings which do not have an ID as
            /// well as strings that already have one
            /// </summary>
            private readonly HashSet<string> knownStrings = new HashSet<string>();

            /// <summary>
            /// ID ⇀ (absolute offset, length) will be used if idMap contains a null
            /// reference
            /// 
            /// @note there is a fake entry at ID 0
            /// </summary>
            public readonly List<Position> stringPositions;

            public sealed class Position
            {
                public Position(long l, int i)
                {
                    absoluteOffset = l;
                    length = i;
                }

                public long absoluteOffset;
                public int length;
            }

            /// <summary>
            /// get string by ID
            /// </summary>
            public readonly List<string> idMap;

            /// <summary>
            /// DO NOT CALL IF YOU ARE NOT GENERATED OR INTERNAL CODE!
            /// </summary>
            public StringPool(FileInputStream input)
            {
                this.input = input;
                stringPositions = new List<Position>();
                stringPositions.Add(new Position(-1L, -1));
                idMap = new List<string>();
                idMap.Add(null);
            }

            public bool IsReadOnly => throw new NotImplementedException();

            public int Count
            {
                get
                {
                    return knownStrings.Count;
                }
            }

            public string get(int index)
            {
                if (0L == index)
                {
                    return null;
                }

                string result;
                try
                {
                    result = idMap[index];
                }
                catch (System.ArgumentOutOfRangeException e)
                {
                    throw new InvalidPoolIndexException(index, stringPositions.Count, "string", e);
                }
                if (null != result)
                {
                    return result;
                }

                // we have to load the string from disk
                // @note this block has to be synchronized in order to enable parallel
                // decoding of field data
                // @note this is correct, because string pool is the only one who can do
                // parallel operations on input!
                lock (this)
                {
                    Encoding utf8 = Encoding.UTF8;
                    Position off = stringPositions[index];
                    input.push(off.absoluteOffset);
                    byte[] bytes = input.bytes(off.length);
                    char[] chars = utf8.GetChars(bytes);
                    input.pop();

                    try
                    {
                        result = new String(chars);
                    }
                    catch (InvalidOperationException e)
                    {
                        // as if that would ever happen
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                    idMap[index] = result;
                    knownStrings.Add(result);
                }
                return result;
            }

            public virtual void prepareAndWrite(FileOutputStream @out, StateWriter ws)
            {
                Dictionary<string, int> serializationIDs = ws.stringIDs;

                // throw away id map, as it is no longer valid
                idMap.Clear();
                idMap.Add(null);

                // create inverse map
                foreach (string s in knownStrings)
                { 
                    if (s != null && !serializationIDs.ContainsKey(s))
                    {
                        serializationIDs[s] = idMap.Count;
                        idMap.Add(s);
                    }
                }

                // count
                // @note idMap access performance hack
                @out.v64(idMap.Count - 1);

                // @note idMap access performance hack
                if (1 != idMap.Count)
                {
                    Encoding utf8 = Encoding.UTF8;
                    // offsets
                    BinaryWriter end = new BinaryWriter(new MemoryStream(4 * (idMap.Count - 1)));
                    int off = 0;
                    for (int i = 1; i < idMap.Count; i++)
                    {
                        off += utf8.GetBytes(idMap[i]).Length;
                        byte[] bytes = BitConverter.GetBytes(off);
                        Array.Reverse(bytes);
                        end.Write(bytes);
                    }
                    end.BaseStream.Position = 0;
                    @out.put(end);
                    
                    // data
                    for (int i = 1; i < idMap.Count; i++)
                    {
                        byte[] bytes = utf8.GetBytes(idMap[i]);
                        @out.put(bytes);
                    }
                }
            }

            /// <summary>
            /// prepares serialization of the string pool and appends new Strings to the
            /// output stream.
            /// </summary>
            public virtual void prepareAndAppend(FileOutputStream @out, StateAppender @as)
            {
                Dictionary<string, int> serializationIDs = @as.stringIDs;

                // create inverse map
                for (int i = 1; i < idMap.Count; i++)
                {
                    serializationIDs[idMap[i]] = i;
                }

                List<byte[]> todo = new List<byte[]>();

                // Insert new strings to the map;
                // this is the place where duplications with lazy strings will be
                // detected and eliminated
                // this is also the place, where new instances are appended to the
                // output file
                Encoding utf8 = Encoding.UTF8;
                foreach (string s in knownStrings)
                {
                    if (!serializationIDs.ContainsKey(s))
                    {
                        serializationIDs[s] = idMap.Count;
                        idMap.Add(s);

                        todo.Add(utf8.GetBytes(s));
                    }
                }

                // count
                int count = todo.Count;
                @out.v64(count);

                int off = 0;
                // end
                BinaryWriter end = new BinaryWriter(new MemoryStream(4 * count));
                foreach (byte[] s in todo)
                {
                    off += s.Length;
                    byte[] bytes = BitConverter.GetBytes(off);
                    Array.Reverse(bytes);
                    end.Write(bytes);
                }
                @out.put(((MemoryStream)end.BaseStream).ToArray());

                // data
                foreach (byte[] s in todo)
                {
                    Array.Reverse(s);
                    @out.put(s);
                }

            }

            public bool Empty
            {
                get
                {
                    return Count == 0;
                }
            }

            public bool Contains(string o)
            {
                return knownStrings.Contains(o);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return knownStrings.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return knownStrings.GetEnumerator();
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                knownStrings.CopyTo(array, arrayIndex);
            }


            public void Add(string e)
            {
                knownStrings.Add(e);
            }

            public bool Remove(string o)
            {
                return knownStrings.Remove(o);
            }

            public void Clear()
            {
                knownStrings.Clear();
            }

            public virtual bool hasInStream()
            {
                return null != input;
            }

            public virtual FileInputStream InStream
            {
                get
                {
                    return input;
                }
            }
        }
    }
}