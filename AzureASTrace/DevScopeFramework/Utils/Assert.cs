using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DevScope.Framework.Common.Utils
{
    public class Assert
    {
        #region Configuration

        private static bool enabled = true;
        public static bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        #endregion

        // Methods
        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, string.Empty, null);


        }

        public static void AreEqual<T>(T expected, T actual)
        {
            AreEqual<T>(expected, actual, string.Empty, null);
        }

        public static void AreEqual(double expected, double actual, double delta)
        {
            AreEqual(expected, actual, delta, string.Empty, null);
        }

        public static void AreEqual(object expected, object actual, string message)
        {
            AreEqual(expected, actual, message, null);
        }

        public static void AreEqual(float expected, float actual, float delta)
        {
            AreEqual(expected, actual, delta, string.Empty, null);
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            AreEqual<T>(expected, actual, message, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase)
        {
            AreEqual(expected, actual, ignoreCase, string.Empty, (object[])null);
        }

        public static void AreEqual(double expected, double actual, double delta, string message)
        {
            AreEqual(expected, actual, delta, message, null);
        }

        public static void AreEqual(object expected, object actual, string message, params object[] parameters)
        {
            AreEqual<object>(expected, actual, message, parameters);
        }

        public static void AreEqual(float expected, float actual, float delta, string message)
        {
            AreEqual(expected, actual, delta, message, null);
        }

        public static void AreEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            if (!object.Equals(expected, actual))
            {
                string str;
                if (((actual != null) && (expected != null)) && actual.GetType() != expected.GetType())
                {
                    str = string.Format(Messages.AreEqualCaseFailMsg, message, ReplaceNulls(expected), expected.GetType().FullName, ReplaceNulls(actual), actual.GetType().FullName);
                }
                else
                {
                    str = string.Format(Messages.AreEqualFailMsg, message, ReplaceNulls(expected), ReplaceNulls(actual));
                }

                HandleFail("Assert.AreEqual", str, parameters);
            }
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture)
        {
            AreEqual(expected, actual, ignoreCase, culture, string.Empty, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, string message)
        {
            AreEqual(expected, actual, ignoreCase, message, (object[])null);
        }

        public static void AreEqual(double expected, double actual, double delta, string message, params object[] parameters)
        {
            if (Math.Abs((double)(expected - actual)) > delta)
            {
                string str = string.Format(Messages.AreEqualDeltaFailMsg, message, expected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat));
                HandleFail("Assert.AreEqual", str, parameters);
            }
        }

        public static void AreEqual(float expected, float actual, float delta, string message, params object[] parameters)
        {
            if (Math.Abs((float)(expected - actual)) > delta)
            {
                string str = string.Format(Messages.AreEqualDeltaFailMsg, message, expected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat));
                HandleFail("Assert.AreEqual", str, parameters);
            }
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            AreEqual(expected, actual, ignoreCase, culture, message, null);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            AreEqual(expected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            CheckParameterNotNull(culture, "Assert.AreEqual", "culture", string.Empty, new object[0]);

            if (string.Compare(expected, actual, ignoreCase, culture) != 0)
            {
                string str;
                if (!ignoreCase && (string.Compare(expected, actual, true, culture) == 0))
                {
                    str = string.Format(Messages.AreEqualCaseFailMsg, message, ReplaceNulls(expected), ReplaceNulls(actual));
                }
                else
                {
                    str = string.Format(Messages.AreEqualFailMsg, message, ReplaceNulls(expected), ReplaceNulls(actual));
                }
                HandleFail("Assert.AreEqual", str, parameters);
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            AreNotEqual<T>(notExpected, actual, string.Empty, null);
        }

        public static void AreNotEqual(object notExpected, object actual)
        {
            AreNotEqual(notExpected, actual, string.Empty, null);
        }

        public static void AreNotEqual(double notExpected, double actual, double delta)
        {
            AreNotEqual(notExpected, actual, delta, string.Empty, null);
        }

        public static void AreNotEqual(object notExpected, object actual, string message)
        {
            AreNotEqual(notExpected, actual, message, null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message)
        {
            AreNotEqual<T>(notExpected, actual, message, null);
        }

        public static void AreNotEqual(float notExpected, float actual, float delta)
        {
            AreNotEqual(notExpected, actual, delta, string.Empty, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase)
        {
            AreNotEqual(notExpected, actual, ignoreCase, string.Empty, (object[])null);
        }

        public static void AreNotEqual(double notExpected, double actual, double delta, string message)
        {
            AreNotEqual(notExpected, actual, delta, message, null);
        }

        public static void AreNotEqual(float notExpected, float actual, float delta, string message)
        {
            AreNotEqual(notExpected, actual, delta, message, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message)
        {
            AreNotEqual(notExpected, actual, ignoreCase, message, (object[])null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message, params object[] parameters)
        {
            if (object.Equals(notExpected, actual))
            {
                string str = string.Format(Messages.AreNotEqualFailMsg, message, ReplaceNulls(notExpected), ReplaceNulls(actual));
                HandleFail("Assert.AreNotEqual", str, parameters);
            }
        }

        public static void AreNotEqual(object notExpected, object actual, string message, params object[] parameters)
        {
            AreNotEqual<object>(notExpected, actual, message, parameters);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture)
        {
            AreNotEqual(notExpected, actual, ignoreCase, culture, string.Empty, null);
        }

        public static void AreNotEqual(double notExpected, double actual, double delta, string message, params object[] parameters)
        {
            if (Math.Abs((double)(notExpected - actual)) <= delta)
            {
                string str = string.Format(Messages.AreNotEqualDeltaFailMsg, message, notExpected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat));
                HandleFail("Assert.AreNotEqual", str, parameters);
            }
        }

        public static void AreNotEqual(float notExpected, float actual, float delta, string message, params object[] parameters)
        {
            if (Math.Abs((float)(notExpected - actual)) <= delta)
            {
                string str = string.Format(Messages.AreNotEqualDeltaFailMsg, message, notExpected.ToString(CultureInfo.CurrentCulture.NumberFormat), actual.ToString(CultureInfo.CurrentCulture.NumberFormat), delta.ToString(CultureInfo.CurrentCulture.NumberFormat));
                HandleFail("Assert.AreNotEqual", str, parameters);
            }
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            AreNotEqual(notExpected, actual, ignoreCase, culture, message, null);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            AreNotEqual(notExpected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            CheckParameterNotNull(culture, "Assert.AreNotEqual", "culture", string.Empty, new object[0]);
            if (string.Compare(notExpected, actual, ignoreCase, culture) == 0)
            {
                string str = string.Format(Messages.AreNotEqualFailMsg, message, ReplaceNulls(notExpected), ReplaceNulls(actual));
                HandleFail("Assert.AreNotEqual", str, parameters);
            }
        }

        public static void AreNotSame(object notExpected, object actual)
        {
            AreNotSame(notExpected, actual, string.Empty, null);
        }

        public static void AreNotSame(object notExpected, object actual, string message)
        {
            AreNotSame(notExpected, actual, message, null);
        }

        public static void AreNotSame(object notExpected, object actual, string message, params object[] parameters)
        {
            if (object.ReferenceEquals(notExpected, actual))
            {
                HandleFail("Assert.AreNotSame", message, parameters);
            }
        }

        public static void AreSame(object expected, object actual)
        {
            AreSame(expected, actual, string.Empty, null);
        }

        public static void AreSame(object expected, object actual, string message)
        {
            AreSame(expected, actual, message, null);
        }

        public static void AreSame(object expected, object actual, string message, params object[] parameters)
        {
            if (!object.ReferenceEquals(expected, actual))
            {
                string str = message;
                if ((expected is ValueType) && (actual is ValueType))
                {
                    str = string.Format(Messages.AreSameGivenValues, message);
                }
                HandleFail("Assert.AreSame", str, parameters);
            }
        }

        internal static void CheckParameterNotNull(object param, string assertionName, string parameterName, string message, params object[] parameters)
        {
            if (param == null)
            {
                HandleFail(assertionName, string.Format(Messages.NullParameterToAssert, parameterName, message), parameters);
            }
        }

        public static new bool Equals(object objA, object objB)
        {
            Fail(string.Format(Messages.DoNotUseAssertEquals));
            return false;
        }

        public static void Fail()
        {
            Fail(string.Empty, null);
        }

        public static void Fail(string message)
        {
            Fail(message, null);
        }

        public static void Fail(string message, params object[] parameters)
        {
            HandleFail("Assert.Fail", message, parameters);
        }

        internal static void HandleFail(string assertionName, string message, params object[] parameters)
        {
            //Se não tiver activo não da erro
            if (!Enabled)
                return;

            string str = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                if (parameters == null)
                {
                    str = message;
                }
                else
                {
                    str = string.Format(CultureInfo.CurrentCulture, message, parameters);
                }
            }

            throw new AssertFailedException(string.Format(Messages.AssertionFailed, assertionName, str));
        }

        public static void Inconclusive()
        {
            Inconclusive(string.Empty, null);
        }

        public static void Inconclusive(string message)
        {
            Inconclusive(message, null);
        }

        public static void Inconclusive(string message, params object[] parameters)
        {
            //Se não tiver activo não da erro
            if (!Enabled)
                return;

            string str = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                if (parameters == null)
                {
                    str = message;
                }
                else
                {
                    str = string.Format(CultureInfo.CurrentCulture, message, parameters);
                }
            }
            throw new AssertInconclusiveException(string.Format(Messages.AssertionFailed, "Assert.Inconclusive", str));
        }

        public static void IsFalse(bool condition)
        {
            IsFalse(condition, string.Empty, null);
        }

        public static void IsFalse(bool condition, string message)
        {
            IsFalse(condition, message, null);
        }

        public static void IsFalse(bool condition, string message, params object[] parameters)
        {
            if (condition)
            {
                HandleFail("Assert.IsFalse", message, parameters);
            }
        }

        public static void IsInstanceOfType(object value, Type expectedType)
        {
            IsInstanceOfType(value, expectedType, string.Empty, null);
        }

        public static void IsInstanceOfType(object value, Type expectedType, string message)
        {
            IsInstanceOfType(value, expectedType, message, null);
        }

        public static void IsInstanceOfType(object value, Type expectedType, string message, params object[] parameters)
        {
            if (expectedType == null)
            {
                HandleFail("Assert.IsInstanceOfType", message, parameters);
            }
            if (!expectedType.IsInstanceOfType(value))
            {
                string str = string.Format(Messages.IsInstanceOfFailMsg, message, expectedType.ToString(), (value == null) ? ((object)Messages.Common_NullInMessages) : ((object)value.GetType().ToString()));
                HandleFail("Assert.IsInstanceOfType", str, parameters);
            }
        }

        public static void IsNotInstanceOfType(object value, Type wrongType)
        {
            IsNotInstanceOfType(value, wrongType, string.Empty, null);
        }

        public static void IsNotInstanceOfType(object value, Type wrongType, string message)
        {
            IsNotInstanceOfType(value, wrongType, message, null);
        }

        public static void IsNotInstanceOfType(object value, Type wrongType, string message, params object[] parameters)
        {
            if (wrongType == null)
            {
                HandleFail("Assert.IsNotInstanceOfType", message, parameters);
            }
            if ((value != null) && wrongType.IsInstanceOfType(value))
            {
                string str = string.Format(Messages.IsNotInstanceOfFailMsg, message, wrongType.ToString(), value.GetType().ToString());
                HandleFail("Assert.IsNotInstanceOfType", str, parameters);
            }
        }

        public static void IsNullOrEmpty(string value, string message = null, params object[] parameters)
        {
            if (!string.IsNullOrEmpty(value))
            {
                HandleFail("Assert.IsNullOrEmpty", message, parameters);
            }
        }

        public static void IsNotNullOrEmpty(string value, string message = null, params object[] parameters)
        {
            if (string.IsNullOrEmpty(value))
            {
                HandleFail("Assert.IsNotNullOrEmpty", message, parameters);
            }
        }

        public static void IsNotNull(object value)
        {
            IsNotNull(value, string.Empty, null);
        }

        public static void IsNotNull(object value, string message)
        {
            IsNotNull(value, message, null);
        }

        public static void IsNotNull(object value, string message, params object[] parameters)
        {
            if (value == null)
            {
                HandleFail("Assert.IsNotNull", message, parameters);
            }
        }

        public static void IsNull(object value)
        {
            IsNull(value, string.Empty, null);
        }

        public static void IsNull(object value, string message)
        {
            IsNull(value, message, null);
        }

        public static void IsNull(object value, string message, params object[] parameters)
        {
            if (value != null)
            {
                HandleFail("Assert.IsNull", message, parameters);
            }
        }

        public static void IsTrue(bool condition)
        {
            IsTrue(condition, string.Empty, null);
        }

        public static void IsTrue(bool condition, string message)
        {
            IsTrue(condition, message, null);
        }

        public static void IsTrue(bool condition, string message, params object[] parameters)
        {
            if (!condition)
            {
                HandleFail("Assert.IsTrue", message, parameters);
            }
        }

        public static string ReplaceNullChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\0')
                {
                    list.Add(i);
                }
            }
            if (list.Count <= 0)
            {
                return input;
            }
            StringBuilder builder = new StringBuilder(input.Length + list.Count);
            int startIndex = 0;
            foreach (int num3 in list)
            {
                builder.Append(input.Substring(startIndex, num3 - startIndex));
                builder.Append(@"\0");
                startIndex = num3 + 1;
            }
            builder.Append(input.Substring(startIndex));
            return builder.ToString();
        }

        internal static string ReplaceNulls(object input)
        {
            if (input == null)
            {
                return Messages.Common_NullInMessages;
            }

            string str = input.ToString();

            if (str == null)
            {
                return Messages.Common_ObjectString;
            }

            return ReplaceNullChars(str);
        }

    }

    public class AssertFailedException : Exception
    {
        // Methods
        public AssertFailedException()
        {
        }

        public AssertFailedException(string msg)
            : base(msg)
        {
        }

        internal AssertFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }

    public class AssertInconclusiveException : Exception
    {
        // Methods
        public AssertInconclusiveException()
        {
        }

        public AssertInconclusiveException(string msg)
            : base(msg)
        {
        }

        internal AssertInconclusiveException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public static class Messages
    {
        public const string AreEqualCaseFailMsg = "Expected:<{1}>. Case is different for actual value:<{2}>. {0}";
        public const string AreEqualFailMsg = "Expected:<{1}>. Actual:<{2}>. {0}";
        public const string AreNotEqualFailMsg = "Expected any value except:<{1}>. Actual:<{2}>. {0}";
        public const string AreSameGivenValues = "Do not pass value types to AreSame(). Values converted to Object will never be the same. Consider using AreEqual(). {0}";
        public const string AreEqualDeltaFailMsg = "Expected a difference no greater than <{3}> between expected value <{1}> and actual value <{2}>. {0}";
        public const string AreNotEqualDeltaFailMsg = "Expected a difference greater than <{3}> between expected value <{1}> and actual value <{2}>. {0}";
        public const string NullParameterToAssert = "The parameter '{0}' is invalid. The value cannot be null. {1}.";
        public const string DoNotUseAssertEquals = "Assert.Equals should not be used for Assertions. Please use Assert.AreEqual & overloads instead.";
        public const string Common_NullInMessages = "(null)";
        public const string Common_ObjectString = "(object)";
        public const string IsInstanceOfFailMsg = "{0}Expected type:<{1}>. Actual type:<{2}>.";
        public const string AssertionFailed = "{0} failed. {1}";
        public const string IsNotInstanceOfFailMsg = "Wrong Type:<{1}>. Actual type:<{2}>. {0}";
    }
}
