using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace DevScope.Framework.Common.Utils
{
    public static class RegistryHelper
    {
        public static RegistryKey GetRegistrySubKey(RegistryKey key, string subKeyName, bool writable)
        {
            RegistryKey subKey = key.OpenSubKey(subKeyName, writable);

            if (subKey == null)
            {
                throw new ApplicationException(string.Format("SubKey not found: {0}.{1}.", key.ToString(), subKeyName));
            }

            return subKey;
        }

        public static object GetRegistry(RegistryKey key, string name)
        {
            object retVale = TryGetRegistry(key, null, name, null);

            if (retVale == null)
            {
                throw new ApplicationException(string.Format("Key not found: {0}.{1}.", key.ToString(), name));
            }

            return retVale;
        }

        public static object GetRegistry(RegistryKey key, string subKey, string name)
        {
            object retVale = TryGetRegistry(key, subKey, name, null);

            if (retVale == null)
            {
                throw new ApplicationException(string.Format("Key not found:  {0}.{1}.{2}.", key.ToString(), subKey, name));
            }

            return retVale;
        }

        public static object TryGetRegistry(RegistryKey key, string name, object defaultValue)
        {
            return TryGetRegistry(key, null, name, defaultValue);
        }

        public static object TryGetRegistry(RegistryKey key, string subkey, string name, object defaultValue)
        {
            try
            {
                RegistryKey rootKey = null;

                if (!string.IsNullOrEmpty(subkey))
                    rootKey = key.OpenSubKey(subkey);
                else
                    rootKey = key;

                if (rootKey == null)
                    return defaultValue;

                object retVale = rootKey.GetValue(name);

                if (retVale == null)
                    return defaultValue;

                return retVale;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void SetRegistry(RegistryKey key, string subkey, string name, object value)
        {
            RegistryKey rootKey = key.OpenSubKey(subkey, true);
            rootKey.SetValue(name, value);
        }

    }
}
