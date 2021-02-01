#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

#endregion

namespace RedisOidcDemoHost.Redis
{
    /// <summary>
    ///     Provides utility methods for converting string values to other data types.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        ///     Removes dashes ("-") from the given object value represented as a string and returns an empty string ("")
        ///     when the instance type could not be represented as a string.
        ///     <para>
        ///         Note: This will return the type name of given instance if the runtime type of the given instance is not a
        ///         string!
        ///     </para>
        /// </summary>
        /// <param name="value">The object instance to un-dash when represented as its string value.</param>
        /// <returns></returns>
        public static string UnDash(this object value)
        {
            return (value as string ?? string.Empty).UnDash();
        }

        /// <summary>
        ///     Removes dashes ("-") from the given string value.
        /// </summary>
        /// <param name="value">The string value that optionally contains dashes.</param>
        /// <returns></returns>
        public static string UnDash(this string value)
        {
            return (value ?? string.Empty).Replace("-", string.Empty);
        }

        /// <summary>
        ///     Checks whether a string value is null or empty.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> is null or is a zero-length string (""); otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        ///     Converts a string to an integer.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        public static int AsInt(this string value)
        {
            return value.AsInt(0);
        }

        /// <summary>
        ///     Converts a string to an integer and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The value to return if <paramref name="value" /> is null or is an invalid value.</param>
        public static int AsInt(this string value, int defaultValue)
        {
            return !int.TryParse(value, out var result) ? defaultValue : result;
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.Decimal" /> number.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        public static decimal AsDecimal(this string value)
        {
            return value.As<decimal>();
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.Decimal" /> number and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The value to return if <paramref name="value" /> is null or invalid.</param>
        public static decimal AsDecimal(this string value, decimal defaultValue)
        {
            return value.As(defaultValue);
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.Single" /> number.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        public static float AsFloat(this string value)
        {
            return value.AsFloat(0.0f);
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.Single" /> number and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The value to return if <paramref name="value" /> is null.</param>
        public static float AsFloat(this string value, float defaultValue)
        {
            if (!float.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.DateTime" /> value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        public static DateTime AsDateTime(this string value)
        {
            return value.AsDateTime(new DateTime());
        }

        /// <summary>
        ///     Converts a string to a <see cref="T:System.DateTime" /> value and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">
        ///     The value to return if <paramref name="value" /> is null or is an invalid value. The default
        ///     is the minimum time value on the system.
        /// </param>
        public static DateTime AsDateTime(this string value, DateTime defaultValue)
        {
            return !DateTime.TryParse(value, out var result) ? defaultValue : result;
        }

        /// <summary>
        ///     Converts a string to a strongly typed value of the specified data type.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="TValue">The data type to convert to.</typeparam>
        public static TValue As<TValue>(this string value)
        {
            return value.As(default(TValue));
        }

        /// <summary>
        ///     Converts a string to a Boolean (true/false) value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        public static bool AsBool(this string value)
        {
            return value.AsBool(false);
        }

        /// <summary>
        ///     Converts a string to a Boolean (true/false) value and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The value to return if <paramref name="value" /> is null or is an invalid value.</param>
        public static bool AsBool(this string value, bool defaultValue)
        {
            if (!bool.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        ///     Converts a string to the specified data type and specifies a default value.
        /// </summary>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The value to return if <paramref name="value" /> is null.</param>
        /// <typeparam name="TValue">The data type to convert to.</typeparam>
        public static TValue As<TValue>(this string value, TValue defaultValue)
        {
            try
            {
                var converter1 = TypeDescriptor.GetConverter(typeof(TValue));
                if (converter1.CanConvertFrom(typeof(string)))
                    return (TValue) converter1.ConvertFrom(value);
                var converter2 = TypeDescriptor.GetConverter(typeof(string));
                if (converter2.CanConvertTo(typeof(TValue)))
                    return (TValue) converter2.ConvertTo(value, typeof(TValue));
            }
            catch
            {
                // ignored
            }

            return defaultValue;
        }

        /// <summary>
        ///     Checks whether a string can be converted to the Boolean (true/false) type.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsBool(this string value)
        {
            return bool.TryParse(value, out _);
        }

        /// <summary>
        ///     Checks whether a string can be converted to an integer.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsInt(this string value)
        {
            return int.TryParse(value, out _);
        }

        /// <summary>
        ///     Checks whether a string can be converted to the <see cref="T:System.Decimal" /> type.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsDecimal(this string value)
        {
            return value.Is<decimal>();
        }

        /// <summary>
        ///     Checks whether a string can be converted to the <see cref="T:System.Single" /> type.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsFloat(this string value)
        {
            return float.TryParse(value, out _);
        }

        /// <summary>
        ///     Checks whether a string can be converted to the <see cref="T:System.DateTime" /> type.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The string value to test.</param>
        public static bool IsDateTime(this string value)
        {
            return DateTime.TryParse(value, out _);
        }

        /// <summary>
        ///     Checks whether a string can be converted to the specified data type.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="value" /> can be converted to the specified type; otherwise, false.
        /// </returns>
        /// <param name="value">The value to test.</param>
        /// <typeparam name="TValue">The data type to convert to.</typeparam>
        public static bool Is<TValue>(this string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TValue));

            try
            {
                if (value != null)
                {
                    if (!converter.CanConvertFrom(typeof(string)))
                        return false;
                }

                converter.ConvertFrom(value);
                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static bool CompareString(this string sourceString, string otherString)
        {
            return string.Equals(sourceString, otherString, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string RemoveTrailingSlash(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return sourceString;

            return sourceString.EndsWith("/") ? sourceString.Substring(0, sourceString.Length - 1) : sourceString;
        }

        public static string AddTrailingSlash(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return sourceString;

            return sourceString.EndsWith("/") ? sourceString : $"{sourceString}/";
        }

        public static string AddLeadingSlash(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return sourceString;

            return sourceString.StartsWith("/") ? sourceString : $"/{sourceString}";
        }

        public static string RemoveLeadingSlash(this string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return sourceString;

            return sourceString.StartsWith("/") ? sourceString.Substring(1, sourceString.Length - 1) : sourceString;
        }

        public static string MakeSafeUrlPart(this string sourceString)
        {
            return string.IsNullOrEmpty(sourceString)
                ? sourceString
                : sourceString.AddLeadingSlash().RemoveTrailingSlash();
        }

        public static string MaskRight(this string sourceString, string maskChar, int rightPadding = 6,
            int numberOfDigits = 4)
        {
            return MaskRight(sourceString, Convert.ToChar(maskChar), rightPadding, numberOfDigits);
        }

        public static string MaskRight(this string sourceString, char maskChar, int rightPadding = 6,
            int numberOfDigits = 4)
        {
            if (string.IsNullOrEmpty(sourceString)) return string.Empty;

            if (sourceString.Length <= numberOfDigits) return sourceString;

            var concat = string.Concat(
                sourceString.Substring(0, numberOfDigits),
                "".PadRight(rightPadding, maskChar)
            );

            return concat;
        }

        public static string MaskLeft(this string sourceString, char maskChar, int leftPadding = 6,
            int numberOfDigits = 4)
        {
            if (string.IsNullOrEmpty(sourceString)) return string.Empty;

            if (sourceString.Length <= numberOfDigits) return sourceString;

            var concat = string.Concat(
                "".PadLeft(leftPadding, maskChar),
                sourceString.Substring(sourceString.Length - numberOfDigits)
            );

            return concat;
        }

        public static string MaskLeft(this string sourceString, string maskChar, int leftPadding = 6,
            int numberOfDigits = 4)
        {
            return MaskLeft(sourceString, Convert.ToChar(maskChar), leftPadding, numberOfDigits);
        }

        public static string PadLeft(this string sourceString, char maskChar, int lengthFixedString)
        {
            if (string.IsNullOrEmpty(sourceString)) return string.Empty;

            if (sourceString.Length > lengthFixedString) return sourceString;

            var leftPadding = lengthFixedString - sourceString.Length;

            var concat = string.Concat(
                "".PadLeft(leftPadding, maskChar),
                sourceString
            );

            return concat;
        }

        public static string PadRight(this string sourceString, char maskChar, int lengthFixedString)
        {
            if (string.IsNullOrEmpty(sourceString)) return string.Empty;

            if (sourceString.Length > lengthFixedString) return sourceString;

            var rightPadding = lengthFixedString - sourceString.Length;

            var concat = string.Concat(
                sourceString,
                "".PadLeft(rightPadding, maskChar)
            );

            return concat;
        }

        public static string CompressString(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(text);

                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        //msi.CopyTo(gs);
                        CopyTo(msi, gs);
                    }

                    var msoArray = mso.ToArray();
                    return Convert.ToBase64String(msoArray);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        [Obsolete("Use DecompressString instead.")]
        public static string UncompressString(this string base64EncodedString)
        {
            return DecompressString(base64EncodedString);
        }

        public static string DecompressString(this string base64EncodedString)
        {
            if (string.IsNullOrEmpty(base64EncodedString)) return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(base64EncodedString);
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }

                    return Encoding.UTF8.GetString(mso.ToArray());
                }

            }
            catch
            {
                return string.Empty;
            }

        }

        private static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }

        }

        public static string ChainWithPipe(this string inputString, params string[] parts)
        {
            return inputString.ChainWith(char.Parse("|"), true, parts);
        }

        public static string ChainWithComma(this string inputString, params string[] parts)
        {
            return inputString.ChainWith(char.Parse(","), true, parts);
        }

        public static string ChainWithHyphen(this string inputString, params string[] parts)
        {
            return inputString.ChainWith(char.Parse("-"), true, parts);
        }

        public static string ChainWithSpace(this string inputString, params string[] parts)
        {
            return inputString.ChainWith(char.Parse(" "), true, parts);
        }

        public static string ChainWithPipe(this string inputString, bool excludeNull, params string[] parts)
        {
            return inputString.ChainWith(char.Parse("|"), excludeNull, parts);
        }

        public static string ChainWithComma(this string inputString, bool excludeNull, params string[] parts)
        {
            return inputString.ChainWith(char.Parse(","), excludeNull, parts);
        }

        public static string ChainWithHyphen(this string inputString, bool excludeNull, params string[] parts)
        {
            return inputString.ChainWith(char.Parse("-"), excludeNull, parts);
        }

        public static string ChainWithSpace(this string inputString, bool excludeNull, params string[] parts)
        {
            return inputString.ChainWith(char.Parse(" "), excludeNull, parts);
        }

        public static string ChainWith(this string inputString, char character, bool excludeNull, params string[] parts)
        {
            if (parts.Length == 0) return inputString;

            var sb = new StringBuilder(inputString);

            var includeStringTest = new Func<string, bool>(s =>
            {
                if (excludeNull && string.IsNullOrEmpty(s)) return false;
                if (!excludeNull && string.IsNullOrEmpty(s)) return true;
                return !string.IsNullOrEmpty(s);
            });

            foreach (var part in parts.Where(includeStringTest))
            {
                sb.Append($"{character}{part}");
            }

            return sb.ToString();
        }

        public static string ChainWith(this string inputString, string stringBlock, bool excludeNull,
            params string[] parts)
        {
            if (parts.Length == 0) return inputString;

            var sb = new StringBuilder(inputString);

            var includeStringTest = new Func<string, bool>(s =>
            {
                if (excludeNull && string.IsNullOrEmpty(s)) return false;
                if (!excludeNull && string.IsNullOrEmpty(s)) return true;
                return !string.IsNullOrEmpty(s);
            });

            foreach (var part in parts.Where(includeStringTest))
            {
                sb.Append($"{stringBlock}{part}");
            }

            return sb.ToString();
        }

        public static string ChainWith(this IEnumerable<string> inputStrings, string stringBlock, bool excludeNull)
        {
            if (inputStrings == null) return string.Empty;

            var enumerable = inputStrings as string[] ?? inputStrings.ToArray();

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (enumerable.Length == 0) return string.Empty;
            if (enumerable.Length == 1) return enumerable[0];

            var parts = enumerable.Skip(1).ToArray();

            var sb = new StringBuilder(enumerable[0]);

            var includeStringTest = new Func<string, bool>(s =>
            {
                if (excludeNull && string.IsNullOrEmpty(s)) return false;
                if (!excludeNull && string.IsNullOrEmpty(s)) return true;
                return !string.IsNullOrEmpty(s);
            });

            foreach (var part in parts.Where(includeStringTest))
            {
                sb.Append($"{stringBlock}{part}");
            }

            return sb.ToString();
        }

        public static SecureString ToSecureString(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) return default(SecureString);

            using (var secureString = new SecureString())
            {
                foreach (var chr in inputString.ToCharArray())
                {
                    secureString.AppendChar(chr);
                }

                secureString.MakeReadOnly();

                return secureString;
            }
        }


        public static string FromSecureString(this SecureString inputString)
        {
            if (inputString == default(SecureString)) return string.Empty;

            string result;
            var length = inputString.Length;
            var pointer = IntPtr.Zero;
            var chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(inputString);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }

            return result;
        }

        public static string ToBase64(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}