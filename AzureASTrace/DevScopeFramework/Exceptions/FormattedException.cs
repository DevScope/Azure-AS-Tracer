using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Exceptions
{
    [Serializable]
    public abstract class FormattedException : ApplicationException
    {
        public FormattedException() { }

        public FormattedException(string message)
            : base(message)
        {
        }

         public FormattedException(string message, params object[] msgParams)
            : base(string.Format(message, msgParams))
        {
        }

        public FormattedException(Exception innerEx, string message, params object[] msgParams)
            : base(string.Format(message, msgParams), innerEx)
        {
        }

        public FormattedException(Exception innerEx, string message)
            : base(message, innerEx)
        {
        }

        public FormattedException(Exception innerEx)
            : base(string.Empty, innerEx)
        {
        }
    }
}
