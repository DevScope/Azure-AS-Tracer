using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevScope.Framework.Common.Utils
{
    public static class VersionHelper
    {
        public static bool IsValidVersionNumber(string version)
        {
            return
                Regex.IsMatch(version, @"^[0-9]+\.[0-9]+\.[0-9]+$") ||
                Regex.IsMatch(version, @"^[0-9]+\.[0-9]$") ||
                Regex.IsMatch(version, @"^[0-9]$");
        }

        private static int[] GetVersionNumbers(string version)
        {
            string[] newVersionNumbers = version.Split('.');
            int[] newVersionN = new int[3] { 0, 0, 0 };
            if (newVersionNumbers.Length > 0)
                newVersionN[0] = int.Parse("0" + newVersionNumbers[0]);
            if (newVersionNumbers.Length > 1)
                newVersionN[1] = int.Parse("0" + newVersionNumbers[1]);
            if (newVersionNumbers.Length > 2)
                newVersionN[2] = int.Parse("0" + newVersionNumbers[2]);

            return newVersionN;
        }

        public static int CompareVersion(string newVersion, string currentVersion)
        {
            int[] newVersionN = GetVersionNumbers(newVersion);
            int[] currentVersionN = GetVersionNumbers(currentVersion);

            if (newVersionN[0] > currentVersionN[0])
            {
                return 1;
            }
            else if (newVersionN[0] < currentVersionN[0])
            {
                return -1;
            }
            else
            {
                if (newVersionN[1] > currentVersionN[1])
                {
                    return 1;
                }
                else if (newVersionN[1] < currentVersionN[1])
                {
                    return -1;
                }
                else
                {
                    if (newVersionN[2] > currentVersionN[2])
                    {
                        return 1;
                    }
                    else if (newVersionN[2] < currentVersionN[2])
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }
}
