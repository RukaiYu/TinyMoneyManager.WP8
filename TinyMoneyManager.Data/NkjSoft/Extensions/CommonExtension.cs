namespace NkjSoft.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;

    public static class CommonExtension
    {
        public static System.Globalization.CultureInfo CultureForConverter;

        public static void AddRange<T>(this System.Collections.Generic.List<T> collection, params T[] ts)
        {
            if (collection != null)
            {
                foreach (T local in ts)
                {
                    collection.Add(local);
                }
            }
        }

        public static void AddRange<T>(this System.Collections.ObjectModel.Collection<T> collection, System.Collections.Generic.IEnumerable<T> items)
        {
            foreach (T local in items)
            {
                collection.Add(local);
            }
        }

        public static void AddRange<T>(this System.Collections.ObjectModel.Collection<T> collection, params T[] ts)
        {
            if (collection != null)
            {
                foreach (T local in ts)
                {
                    collection.Add(local);
                }
            }
        }

        public static TSource Aggregate<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TSource, Int32, TSource> func)
        {
            int num = 0;
            using (System.Collections.Generic.IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                enumerator.MoveNext();
                num++;
                TSource current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    current = func(current, enumerator.Current, num++);
                }
                return current;
            }
        }

        public static System.Collections.Generic.IEnumerable<T> Distinct<T, V>(this System.Collections.Generic.IEnumerable<T> source, System.Func<T, V> func)
        {
            return source.Distinct<T>(new CommonEqualityComparer<T, V>(func));
        }

        public static System.Collections.Generic.IEnumerable<T> Distinct<T, V>(this System.Collections.Generic.IEnumerable<T> source, System.Func<T, V> func, System.Collections.Generic.IEqualityComparer<V> comparer)
        {
            return source.Distinct<T>(new CommonEqualityComparer<T, V>(func, comparer));
        }

        public static void ForEach<T>(this System.Collections.Generic.IEnumerable<T> collection, System.Action<T> action)
        {
            foreach (T local in collection)
            {
                action(local);
            }
        }

        public static void ForEach<T>(this System.Collections.Generic.IEnumerable<T> source, System.Action<T, Int32> action)
        {
            if (source != null)
            {
                System.Collections.Generic.IEnumerator<T> enumerator = source.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    T current = enumerator.Current;
                    action(current, i);
                }
            }
        }

        public static void ForEach<T>(this System.Collections.ICollection source, System.Action<T> action)
        {
            if (((source != null) && (source.Count > 0)) && (action != null))
            {
                foreach (T local in source)
                {
                    T local2 = local;
                    action(local2);
                }
            }
        }

        public static void ForEach<T>(this T[] source, System.Action<T> action)
        {
            if (((source != null) && (source.Length > 0)) && (action != null))
            {
                foreach (T local in source)
                {
                    action(local);
                }
            }
        }

        public static void ForEach(this int times, System.Action<Int32> actionToForeach)
        {
            for (int i = 0; i < times; i++)
            {
                actionToForeach(i);
            }
        }

        public static T Initailize<T>(this T obj) where T : class, new()
        {
            if (obj == null)
            {
                obj = System.Activator.CreateInstance<T>();
            }
            return obj;
        }

        public static T Initailize<T>(this T obj, System.Func<T> constructor) where T : class, new()
        {
            if (obj == null)
            {
                obj = constructor();
            }
            return obj;
        }

        public static bool IsNullOrEmptySeqence<T>(this System.Collections.Generic.IEnumerable<T> source)
        {
            if (source != null)
            {
                return (source.Count<T>() == 0);
            }
            return true;
        }

        public static System.Collections.Generic.List<T> Range<T>(this System.Collections.Generic.List<T> source, int start, int max, System.Func<Int32, T> addMethod)
        {
            System.Collections.Generic.List<T> list = source;
            if (source == null)
            {
                list = new System.Collections.Generic.List<T>();
            }
            for (int i = start; i < max; i++)
            {
                list.Add(addMethod(i));
            }
            return list;
        }

        public static System.DateTime ToDateTime(this string val, System.DateTime? def = new System.DateTime?())
        {
            System.DateTime valueOrDefault = def.GetValueOrDefault();
            System.DateTime.TryParse(val, out valueOrDefault);
            return valueOrDefault;
        }

        public static decimal ToDecimal(this string val)
        {
            decimal result = 0.0M;
            decimal.TryParse(val, out result);
            return result;
        }

        public static decimal ToDecimal(this string val, System.Func<Decimal> callBackWhenFailed)
        {
            decimal result = 0.0M;
            if (!decimal.TryParse(val, out result) && (callBackWhenFailed != null))
            {
                result = callBackWhenFailed();
            }
            return result;
        }

        public static double ToDouble(this string val)
        {
            double result = 0.0;
            double.TryParse(val, out result);
            return result;
        }

        public static TResult ToEnum<TResult>(this int original) where TResult : struct
        {
            TResult local;
            try
            {
                local = (TResult)System.Enum.Parse(typeof(TResult), original.ToString(), true);
            }
            catch (System.ArgumentNullException exception)
            {
                throw exception;
            }
            catch (System.ArgumentException exception2)
            {
                throw exception2;
            }
            return local;
        }

        public static string ToggleString(this bool toggle, string trueToShow, string falseToShow)
        {
            if (toggle)
            {
                return trueToShow;
            }
            return falseToShow;
        }

        public static string ToMoneyF2(this decimal val)
        {
            return val.ToMoneyF2((CultureForConverter ?? System.Threading.Thread.CurrentThread.CurrentCulture));
        }

        public static string ToMoneyF2(this decimal val, System.Globalization.CultureInfo culture)
        {
            return val.ToString("N", culture);
        }

        public static string ToString(this bool source, string opeartion)
        {
            return string.Format("{0}{1}!", opeartion, source ? "成功" : "失败");
        }

        public static string ToString(this bool source, string whenTrue, string whenFalse)
        {
            if (!source)
            {
                return whenFalse;
            }
            return whenTrue;
        }

        public static string ToStringLine<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, string splitChar)
        {
            if ((source == null) || (source.Count<TSource>() == 0))
            {
                return string.Empty;
            }
            if ((splitChar.Length == 0) || string.IsNullOrEmpty(splitChar))
            {
                splitChar = string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (TSource local in source)
            {
                if (local != null)
                {
                    builder.AppendFormat("{0}{1}", new object[] { local.ToString(), splitChar });
                }
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return builder.ToString(0, builder.Length - 1);
        }

        public static string ToStringLine<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, string splitChar, bool withEnd)
        {
            if (source == null)
            {
                return string.Empty;
            }
            if ((splitChar.Length == 0) || string.IsNullOrEmpty(splitChar))
            {
                splitChar = string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (TSource local in source)
            {
                if (local != null)
                {
                    builder.AppendFormat("{0}{1}", new object[] { local.ToString(), splitChar });
                }
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            if (!withEnd)
            {
                return builder.ToString(0, builder.Length - 1);
            }
            return builder.ToString();
        }

        public static string ToStringLine<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, string itemStringFormatter, string splitChar, System.Func<TSource, Object[]> argsSelector)
        {
            if ((source == null) || (source.Count<TSource>() == 0))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(splitChar))
            {
                splitChar = string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (TSource local in source)
            {
                if (local != null)
                {
                    builder.AppendFormat(itemStringFormatter, argsSelector(local));
                    builder.Append(splitChar);
                }
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return builder.ToString(0, builder.Length - 1);
        }

        public static string ToStringLine<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, string startPrefix, string endPrefix, string splitChar)
        {
            if ((source == null) || (source.Count<TSource>() == 0))
            {
                return string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (TSource local in source)
            {
                if (local != null)
                {
                    builder.AppendFormat("{0}{1}{2}{3}", new object[] { startPrefix, local.ToString(), endPrefix, splitChar });
                }
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return builder.ToString(0, builder.Length - splitChar.Length);
        }

        public static TResult TryGet<TResult, TXType>(this XElement obj, System.Func<XElement, TXType> xObjectGetter, System.Func<TXType, TResult> resultGetter)
        {
            TXType arg = xObjectGetter(obj);
            if (arg == null)
            {
                return default(TResult);
            }
            return resultGetter(arg);
        }

        public static TResult TryGetValue<TResult>(this System.Collections.Generic.IDictionary<String, Object> dictinonary, string key)
        {
            object obj2 = default(TResult);
            dictinonary.TryGetValue(key, out obj2);
            return (TResult)obj2;
        }
    }
}

