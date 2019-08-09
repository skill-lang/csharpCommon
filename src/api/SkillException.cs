using System;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// Top level implementation of all SKilL related exceptions.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class SkillException : Exception
        {

            public SkillException()
            {
            }

            public SkillException(string message) : base(message)
            {
            }

            public SkillException(Exception cause) : base(cause.ToString())
            {
            }

            public SkillException(string message, Exception cause) : base(message, cause)
            {
            }

        }
    }
}