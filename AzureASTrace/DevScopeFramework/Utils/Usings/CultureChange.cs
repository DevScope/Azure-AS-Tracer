using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Utils
{
    public class CultureChange : IDisposable
    {
        public CultureInfo PreviousCulture { get; set; }

        public CultureChange(string culture)
        {
            this.PreviousCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.PreviousCulture != null)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = this.PreviousCulture;
            }
        }

        #endregion
    }
}
