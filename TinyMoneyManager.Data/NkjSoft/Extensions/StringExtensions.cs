namespace NkjSoft.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class StringExtensions
    {
        public const int asciiEnd = 0x7f;
        public const int asciiStart = 0;
        public const string unicodeEndfix = ";";
        public const string unicodePrefix = "&#";

        public static T DefaultIfNull<T>(this string source, T defaultValue)
        {
            if (source.IsNullOrEmpty())
            {
                return defaultValue;
            }
            return (T) System.Convert.ChangeType(source, typeof(T), null);
        }

        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        public static TResult GetValue<TKey, TResult>(this System.Collections.Generic.Dictionary<TKey,TResult> dict, TKey key)
        {
            TResult local = default(TResult);
            dict.TryGetValue(key, out local);
            return local;
        }

        public static TResult GetValueOrPut<TKey, TResult>(this System.Collections.Generic.Dictionary<TKey,TResult> dict, TKey key, TResult valToPutIfNoExist)
        {
            TResult local = valToPutIfNoExist;
            if (!dict.TryGetValue(key, out local))
            {
                dict[key] = valToPutIfNoExist;
            }
            return local;
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool ToBoolean(this object obj, bool def = false)
        {
            if ((obj == null) || string.IsNullOrEmpty(obj.ToString()))
            {
                obj = "false";
            }
            bool result = def;
            bool.TryParse(obj.ToString(), out result);
            return result;
        }

        public static T ToEnum<T>(this string source)
        {
            return source.ToEnum<T>(default(T));
        }

        public static T ToEnum<T>(this string source, T def)
        {
            if (System.Enum.IsDefined(typeof(T), source))
            {
                return (T) System.Enum.Parse(typeof(T), source, true);
            }
            return def;
        }

        public static System.Guid ToGuid(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return Guid.Empty;
            }

            return new System.Guid(source);
        }

        public static int ToInt32(this string source)
        {
            int result = 0;
            int.TryParse(source, out result);
            return result;
        }

        public static string ToUnicodeInt64Value(this string source)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (char ch in source)
            {
                if ((ch >= '\0') && (ch <= '\x007f'))
                {
                    builder.Append(ch);
                }
                else
                {
                    char result = '\0';
                    char.TryParse(ch.ToString(), out result);
                    builder.Append("&#").Append((long) result).Append(";");
                }
            }
            return builder.ToString();
        }
    }
}

