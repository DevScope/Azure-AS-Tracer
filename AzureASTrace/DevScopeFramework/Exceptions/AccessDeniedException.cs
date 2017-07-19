using System;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace DevScope.Framework.Common.Exceptions
{    
    /* Se derivasse de 'SecurityException' o WCF automaticamente mascarava o detalhe da msg e simplesmente indicava AccessDenied
     * Assim posso simplesmente fazer o raise destas excepções que serão devidamente tratadas no AddIn sem mostrar o detalhe
     */     
    public class AccessDeniedException : FormattedException 
    {
        public AccessDeniedException() { }

        public AccessDeniedException(string message)
            : base(message)
        {
        }

        public AccessDeniedException(string message, params object[] msgParams)
            : base(message, msgParams)
        {
        }

        public AccessDeniedException(Exception innerEx, string message, params object[] msgParams)
            : base(innerEx, message, msgParams)
        {
        }

        public AccessDeniedException(Exception innerEx, string message)
            : base(innerEx, message)
        {
        }

        public AccessDeniedException(Exception innerEx)
            : base(innerEx, string.Empty)
        {
        }
    }
}
