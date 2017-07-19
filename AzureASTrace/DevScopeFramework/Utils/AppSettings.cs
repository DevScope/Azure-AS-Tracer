using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DevScope.Framework.Common.Utils
{
    public static class AppSettingsHelper
    {
        public static string GetAppSetting(string settingName, bool mustExist = true, string defaultValue = null)
        {
            string settingValue = System.Configuration.ConfigurationManager.AppSettings[settingName];

            if (settingValue == null)
            {
                if (mustExist)
                    throw new ApplicationException(string.Format("Setting '{0}' is not defined.", settingName));
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return settingValue;
            }
        }

        public static ConnectionStringSettings GetConnection(string connectionStringName, bool mustExist = true)
        {
            var conn = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName];

            if (mustExist && conn == null)
            {
                throw new ApplicationException(string.Format("ConnectionString '{0}' is not defined.", connectionStringName));
            }

            return conn;
        } 
    }
}
