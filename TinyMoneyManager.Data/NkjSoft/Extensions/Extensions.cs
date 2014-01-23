namespace NkjSoft.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Extensions
    {
        public static bool IsEmpty<T>(this System.Collections.Generic.List<T> source)
        {
            if (source != null)
            {
                return (source.Count == 0);
            }
            return true;
        }

        public static System.Text.StringBuilder ToStringBuilder<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            return source.ToStringBuilder<T>("{0}\r\n", new object[0]);
        }

        public static System.Text.StringBuilder ToStringBuilder<T>(this System.Collections.Generic.IEnumerable<T> source, string formatter, params object[] args)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (T local in source)
            {
                builder.AppendFormat(formatter, new object[] { local, args });
            }
            builder.Remove(builder.Length - 1, 1);
            return builder;
        }

        public static string ToStringLine<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            return source.ToStringBuilder<T>().ToString();
        }
    }
}

