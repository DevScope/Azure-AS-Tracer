using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Exceptions
{
    [Serializable]
    public class UserException : FormattedException
    {
        public UserException() { }

        public UserException(string message)
            : base(message)
        {
        }

        public UserException(string message, params object[] msgParams)
            : base(message, msgParams)
        {
        }

        public UserException(Exception innerEx, string message, params object[] msgParams)
            : base(innerEx, message, msgParams)
        {
        }

        public UserException(Exception innerEx, string message)
            : base(innerEx, message)
        {
        }

        public UserException(Exception innerEx)
            : base(innerEx, string.Empty)
        {
        }
    }
}
