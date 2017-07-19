using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Globalization;

namespace DevScope.Framework.Common.Extensions
{
    public static class Generic
    {       
        public static bool IsNumeric(this object expression, bool integer = false)
        {
            if (expression == null)
            {
                return false;
            }

            var valueStr = Convert.ToString(expression);

            if (!integer)
            {                
                double retNum;

                //Try parsing in the current culture
                if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out retNum)
                    ||
                    //Then in neutral language
                    double.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out retNum))
                {
                    return true;
                }
            }
            else
            {
                int retNum;

                //Try parsing in the current culture
                if (int.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out retNum)
                    ||
                    //Then in neutral language
                    int.TryParse(valueStr, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out retNum))
                {
                    return true;
                }
            }

            return false;
        }        

        public static bool IsBaseType(this object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is string || value is DateTime)
            {
                return true;
            }
            else
            {
                var type = value.GetType();

                if (type.IsValueType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If DBNull returns null.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        //public static object IgnoreDBNull(this object value)
        //{
        //    if (value == DBNull.Value)
        //        return null;
                        
        //    return value;
        //}

        public static T IgnoreDBNull<T>(this T value)
        {
            if ((object)value == DBNull.Value)
                return default(T);

            return (T)value;
        }

        /// <summary>
        /// If null returns DBNull
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object AsDBNull(this object value)
        {
            if (value == null)
                return DBNull.Value;

            return value;
        }


    }
}
