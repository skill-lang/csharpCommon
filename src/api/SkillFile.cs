using System.IO;
using System.Collections.Generic;

using SkillObject = de.ust.skill.common.csharp.@internal.SkillObject;
using AbstractStoragePool = de.ust.skill.common.csharp.@internal.AbstractStoragePool;

namespace de.ust.skill.common.csharp
{
    namespace api
    {

        /// <summary>
        /// A SKilL file that can be used to access types stored in a skill file and persist changes.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface SkillFile
        {

            /// <returns> access to known strings </returns>
            StringAccess Strings();

            /// <returns> true, iff the argument object is managed by this state
            /// @note will return true, if argument is null
            /// @note this operation is kind of expensive </returns>
            bool contains(SkillObject target);

            /// <summary>
            /// ensure that the argument instance will be deleted on next flush
            /// </summary>
            void delete(SkillObject target);

            /// <returns> iterator over all user types </returns>
            IEnumerable<IAccess> allTypes();

            /// <returns> stream over all user types </returns>
            List<AbstractStoragePool> allTypesStream();

            /// <summary>
            /// Set a new path for the file. This will influence the next flush/close operation.
            /// </summary>
            /// <exception cref="IOException"> if new path can not be used for some reason
            /// @note (on implementation) memory maps for lazy evaluation must have been created before invocation of this method </exception>
            void changePath(string path);

            /// <returns> the current path pointing to the file </returns>
            string currentPath();

            /// <summary>
            /// Set a new mode.
            /// 
            /// @note not fully implemented
            /// </summary>
            void changeMode(Mode writeMode);

            /// <summary>
            /// Force all lazy string and field data to be loaded from disk.
            /// </summary>
            void loadLazyData();

            /// <summary>
            /// Checks consistency of the current state of the file.
            /// 
            /// @note if check is invoked manually, it is possible to fix the inconsistency and re-check without breaking the
            ///       on-disk representation </summary>
            /// <exception cref="SkillException"> if an inconsistency is found </exception>
            void check();

            /// <summary>
            /// Check consistency and write changes to disk.
            /// 
            /// @note this will not sync the file to disk, but it will block until all in-memory changes are written to buffers.
            /// @note if check fails, then the state is guaranteed to be unmodified compared to the state before flush </summary>
            /// <exception cref="SkillException"> if check fails </exception>
            void flush();

            /// <summary>
            /// Same as flush, but will also sync and close file, thus the state must not be used afterwards.
            /// </summary>
            void close();
        }

        /// <summary>
        /// Modes for file handling.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public enum Mode
        {
            None, Create, Read, Write, Append, ReadOnly
        }

        /// <summary>
        /// Actual mode after processing.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class ActualMode
        {
            public readonly Mode open;
            public readonly Mode close;

            public ActualMode(params Mode[] modes)
            {
                // determine open mode
                // @note read is preferred over create, because empty files are
                // legal and the file has been created by now if it did not exist
                // yet
                // @note write is preferred over append, because usage is more
                // inuitive
                Mode openMode = Mode.None, closeMode = Mode.None;
                foreach (Mode m in modes)
                {
                    switch (m)
                    {
                        case Mode.Create:
                        case Mode.Read:
                            if (Mode.None == openMode)
                            {
                                openMode = m;
                            }
                            else if (openMode != m)
                            {
                                throw new IOException("You can either create or read a file.");
                            }
                            break;
                        case Mode.Append:
                        case Mode.Write:
                            if (Mode.None == closeMode)
                            {
                                closeMode = m;
                            }
                            else if (closeMode != m)
                            {
                                throw new IOException("You can either write or append to a file.");
                            }
                            break;
                        case Mode.ReadOnly:
                            if (Mode.None == closeMode)
                            {
                                closeMode = m;
                            }
                            else if (closeMode != m)
                            {
                                throw new IOException("You cannot combine ReadOnly with another write mode.");
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (Mode.None == openMode)
                {
                    openMode = Mode.Read;
                }
                if (Mode.None == closeMode)
                {
                    closeMode = Mode.Write;
                }

                open = openMode;
                close = closeMode;
            }
        }
    }
}