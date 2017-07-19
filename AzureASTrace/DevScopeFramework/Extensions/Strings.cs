using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DevScope.Framework.Common.Extensions
{
    public static class Strings
    {
        public static double ParseNumeric(this string valueStr)
        {
            if (string.IsNullOrEmpty(valueStr))
            {
                return default(double);
            }

            double retNum;

            //Try parsing in the current culture
            if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out retNum)
                ||
                //Then in neutral language
                double.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out retNum))
            {
                return retNum;
            }

            return retNum;
        }

        public static bool StartsWithCI(this string text, string textToCompare)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            return text.StartsWith(textToCompare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithCI(this string text, string textToCompare)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            return text.EndsWith(textToCompare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsCI(this string text, string textToCompare)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            return text.Equals(textToCompare, StringComparison.OrdinalIgnoreCase);            
        }

        public static bool ContainsCI(this string text, string textToCompare)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text.IndexOf(textToCompare, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;            
        }

        public static string ReplaceCaseInsensitive(this string text, string textToReplace, string replaceText)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(textToReplace))
                throw new ArgumentNullException("textToReplace");

            return Regex.Replace(text, Regex.Escape(textToReplace), replaceText, RegexOptions.IgnoreCase);
        }

        public static string FormatText(this string text, params object[] args)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            return string.Format(text, args);
        }

        /// <summary>
        /// Truncates the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns></returns>
        public static string TruncateString(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;            

            const string suffix = "...";

            string truncatedString = text;

            if (maxLength <= 0)
                return truncatedString;

            int strLength = maxLength - suffix.Length;

            if (strLength <= 0)
                return truncatedString;

            if (text == null || text.Length <= maxLength)
                return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;

            return truncatedString;
        }

        public static string RemoveAccentuation(this string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);

            return System.Text.Encoding.UTF8.GetString(tempBytes);
        }

        public static string Left(this string text, int length)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            if (length < 0)            
                throw new ArgumentOutOfRangeException("length", length, "length must be > 0");

            if (length == 0 || text.Length == 0)
            {
                return "";
            }
            else if (text.Length <= length)
            {
                return text;
            }
            else
            {
                return text.Substring(0, length);
            }
        }

        public static string GetMd5Hash(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException("input");

            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = new MD5CryptoServiceProvider();

            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string ToSentenceCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            return Regex.Replace(text, "[a-z][A-Z]", m => string.Concat(m.Value[0], ' ', char.ToUpper(m.Value[1])));
        }

        public static bool EqualsTerms(this string text, string searchTerms)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(searchTerms))
                throw new ArgumentNullException("searchTerms");

            var searchTermsArray = searchTerms.Split(' ');

            if (searchTermsArray.Length == 0)
            {
                return false;
            }

            foreach (var term in searchTermsArray)
            {
                if (string.IsNullOrEmpty(term))
                {
                    continue;
                }

                if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            
            // Ref: http://msdn.microsoft.com/en-us/library/system.globalization.textinfo.totitlecase.aspx

            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            return currentCulture.TextInfo.ToTitleCase(text);
        }
    }
}
