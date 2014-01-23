// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonExtension.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;

#if !NETFX_CORE

using System.IO.IsolatedStorage;
#endif

namespace SpeechLabs.Common.Extensions
{

    /// <summary>
    /// 
    /// </summary>
    public static class CommonExtension
    {
        /// <summary>
        /// Converts the Int32 value to a Enum type by its value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="original">The original value that can be converted to Enum of TResult.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public static TResult ToEnum<TResult>(this int original) where TResult : struct
        {
            try
            {
                var typeOfEnum = typeof(TResult);

                if (Enum.IsDefined(typeOfEnum, original))
                {
                    return (TResult)Enum.Parse(typeof(TResult), original.ToString(), true);
                }

                return default(TResult);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

        /// <summary>
        /// Join each elements in <see cref="System.Collections.Generic.IEnumerator&lt;TSource&gt;"/> to <see cref="String"/> by using a Specific formatter.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="startPrefix">The start prefix.</param>
        /// <param name="endPrefix">The end prefix.</param>
        /// <param name="splitChar">The split char.</param>
        /// <returns></returns>
        public static string ToStringLine<TSource>(this IEnumerable<TSource> source, string startPrefix, string endPrefix, string splitChar)
        {
            if (source == null || source.Count() == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach (TSource item in source)
            {
                if (null == item)
                {
                    continue;
                }

                builder.AppendFormat("{0}{1}{2}{3}", startPrefix, item.ToString(), endPrefix, splitChar);
            }

            if (builder.Length <= 0)
            {
                return string.Empty;
            }

            return builder.ToString(0, builder.Length - splitChar.Length);
        }

        /// <summary>
        /// Join each elements in <see cref="System.Collections.Generic.IEnumerator&lt;TSource&gt;"/> to <see cref="String"/> by using a Specific formatter.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="splitChar">The split char.</param>
        /// <returns></returns>
        public static string ToStringLine<TSource>(this IEnumerable<TSource> source, string splitChar)
        {
            if (source == null || source.Count() == 0)
            {
                return string.Empty;
            }

            if (splitChar.Length == 0 || String.IsNullOrEmpty(splitChar))
            {
                splitChar = string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach (TSource i in source)
            {
                if (null == i)
                {
                    continue;
                }

                builder.AppendFormat("{0}{1}", i.ToString(), splitChar);
            }

            if (builder.Length <= 0)
            {
                return string.Empty;
            }

            return builder.ToString(0, builder.Length - 1);
        }

        /// <summary>
        /// Apply a action to <typeparamref name="T"/> in each elements in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this T[] source, Action<T> action)
        {
            if (source != null && source.Length > 0 && action != null)
            {
                foreach (T item in source)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Apply a action to <typeparamref name="T"/> in each elements in <paramref name="source"/>, providing the index of the element that is being processing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null)
            {
                return;
            }

            var enumerator = source.GetEnumerator();
            int length = 0;
            while (enumerator.MoveNext())
            {
                T t = enumerator.Current;
                action(t, length);
                length++;
            }
        }

        /// <summary>
        /// Apply a action to <typeparamref name="T"/> in each elements in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this ICollection source, Action<T> action)
        {
            if (source != null && source.Count > 0 && action != null)
            {
                foreach (T item in source)
                {
                    T t = item;
                    action(t);
                }
            }
        }

        /// <summary>
        /// Determines whether [is null or empty seqence] [the specified source].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty seqence] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmptySeqence<T>(this IEnumerable<T> source)
        {
            return (source == null || source.Count() == 0);
        }

        /// <summary>
        /// Returns specific <see cref="String"/> according to the boolean value of current.
        /// </summary>
        /// <param name="source">if set to <c>true</c> [source].</param>
        /// <param name="whenTrue">Return this value if current instance is true.</param>
        /// <param name="whenFalse">Return this value if current instance is false.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public static string ToString(this bool source, string whenTrue, string whenFalse)
        {
            return source ? whenTrue : whenFalse;
        }

        /// <summary>
        /// Distincts the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> func)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(func));
        }

        /// <summary>
        /// Distincts the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="func">The func.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> func, IEqualityComparer<V> comparer)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(func, comparer));
        }

        /// <summary>
        /// Tries the get value from a IDictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="dictinonary">The dictinonary.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="NullReferenceException"></exception>
        /// <returns></returns>
        public static TResult TryGetValue<TResult>(this IDictionary<string, object> dictinonary, string key)
        {
            object t = default(TResult);
            dictinonary.TryGetValue(key, out t);
            return (TResult)t;
        }

        /// <summary>
        /// Tries the get value from a IDictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="dictinonary">The dictinonary.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="NullReferenceException"></exception>
        /// <returns></returns>
        public static TResult TryGetValue<TResult>(this IDictionary<string, object> dictinonary, string key, TResult defaultValue)
        {
            if (!dictinonary.ContainsKey(key))
            {
                return defaultValue;
            }

            object t = null;
            dictinonary.TryGetValue(key, out t);
            return (TResult)t;
        }

        /// <summary>
        /// Tries the get value from a IDictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="dictinonary">The dictinonary.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="NullReferenceException"></exception>
        /// <returns></returns>
        public static TResult TryGetValue<TResult>(this IDictionary<string, TResult> dictinonary, string key, TResult defaultValue)
        {
            if (!dictinonary.ContainsKey(key))
            {
                return defaultValue;
            }

            TResult t = default(TResult);
            dictinonary.TryGetValue(key, out t);
            return (TResult)t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="valuesProvider"></param>
        /// <param name="keyOfTheInstanceToGet"></param>
        /// <returns></returns>
        public static TResult Extract<TResult>(this Dictionary<string, TResult> valuesProvider, string keyOfTheInstanceToGet, Func<string, TResult> instanceCreator = null)
        {
            if (valuesProvider == null)
            {
                valuesProvider = new Dictionary<string, TResult>();
            }

            TResult result = default(TResult);

            if (valuesProvider.ContainsKey(keyOfTheInstanceToGet))
            {
                result = valuesProvider[keyOfTheInstanceToGet];
            }
            else
            {
                if (instanceCreator == null)
                {
                    result = (TResult)Activator.CreateInstance(
                         Type.GetType(keyOfTheInstanceToGet, false));
                }
                else
                {
                    result = instanceCreator(keyOfTheInstanceToGet);
                }

                valuesProvider.Add(keyOfTheInstanceToGet, result);
            }

            return result;

        }


        /// <summary>
        /// Tries to get the specific object's value of current XElement.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TXType">The type of the X type.</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="xObjectGetter">The x object getter.</param>
        /// <param name="resultGetter">The result getter.</param>
        /// <returns></returns>
        public static TResult TryGet<TResult, TXType>(this XElement obj, Func<XElement, TXType> xObjectGetter, Func<TXType, TResult> resultGetter, Func<XElement, TResult> ifxObjecteNullresultGetter = null)
        {
            var xt = xObjectGetter(obj);

            if (xt == null)
            {
                if (ifxObjecteNullresultGetter != null)
                {
                    var val = ifxObjecteNullresultGetter(obj);

                    return val;
                }

                return default(TResult);
            }

            return resultGetter(xt);
        }


        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="TType">The type of the type.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="resultGetter">The result getter.</param>
        /// <param name="resultIfNull">The result if null.</param>
        /// <returns></returns>
        public static TResult TryGet<TResult>(this XElement obj, Func<XElement, TResult> resultGetter, TResult resultIfNull = default(TResult))
        {
            if (obj == null)
            {
                return resultIfNull;
            }
            else
            {
                return resultGetter(obj);
            }
        }

        /// <summary>
        /// Initailizes the specified obj.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static T Initailize<T>(this T obj) where T : class, new()
        {
            if (obj == null)
            {
                obj = new T();
            }

            return obj;
        }

        public static void AddRange<T>(this Collection<T> collection, params T[] ts)
        {
            if (collection != null)
            {
                foreach (T t in ts)
                {
                    collection.Add(t);
                }
            }
        }

        /// <summary>
        /// Gets the index of key in current <see cref="Dictionary&lt;TKey,TValue&gt;"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static int GetIndexOf<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, int defIndex = 0)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return dict.Keys.ToList()
                    .IndexOf(key);
            }

            return defIndex;
        }

        /// <summary>
        /// test weather the list contains at least n continuous same values
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool ContainsContinuousValue<T>(this IEnumerable<T> list, T value, int n)
        {
            int count = 0;
            foreach (var v in list)
            {
                if (Object.Equals(value, v))
                {
                    if (++count >= n)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 0;
                }
            }

            return false;
        }


#if NETFX_CORE
        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TResult TryGetValue<TResult>(this Windows.Storage.ApplicationDataContainer container, string key, TResult defaultValue)
        {
            if (container.Values != null && container.Values.ContainsKey(key))
            {
                return (TResult)container.Values.TryGetValue(key, defaultValue);
            }

            return defaultValue;
        }
#endif
    }

    public static class IsolatedStorageSettingExtensions
    {
#if NETFX_CORE
        /// <summary>
        /// Gets the isolated storage app setting value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="settings">The settings.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TResult GetIsolatedStorageAppSettingValue<TResult>(this Windows.Storage.ApplicationDataContainer settings, string key, TResult defaultValue)
        {
            return settings.TryGetValue<TResult>(key, defaultValue);
        }
#else

        /// <summary>
        /// Gets the isolated storage app setting value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="settings">The settings.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TResult GetIsolatedStorageAppSettingValue<TResult>(this System.IO.IsolatedStorage.IsolatedStorageSettings settings, string key, TResult defaultValue)
        {
            var obj = new object();

            if (settings.TryGetValue(key, out obj))
            {
                return (TResult)obj;
            }
            else
            {
                return defaultValue;
            }

        }


        /// <summary>
        /// Clears the directory.
        /// </summary>
        /// <param name="folderDelete">The folder delete.</param>
        public static void ClearDirectory(string folderDelete)
        {
            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.DirectoryExists(folderDelete))
                {
                    iso.PurgeDirectory(folderDelete);
                }
            }
        }

        internal static void PurgeDirectory(this IsolatedStorageFile isf, string directory)
        {
#if DEBUG
            PurgeDirectoryNotSafe(isf, directory);
#else
            try
            {
                PurgeDirectoryNotSafe(isf, directory);
            }
            catch (Exception ex)
            {
                throw new IOException("Exception occured while directory purge" + directory, ex);
            }
#endif
        }

        private static void PurgeDirectoryNotSafe(IsolatedStorageFile isf, string directory)
        {
            if (!isf.DirectoryExists(directory))
            {
                return;
            }

            // delete files
            foreach (string file in isf.GetFileNames(directory + "\\*"))
            {
                isf.DeleteFile(Path.Combine(directory, file));
            }

            // delete sub directories
            foreach (string subDirectory in isf.GetDirectoryNames(directory + "\\*"))
            {
                isf.PurgeDirectory(Path.Combine(directory, subDirectory));
            }

            // delete directory
            isf.DeleteDirectory(directory);
        }


        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string ReadString(string path)
        {
            string content;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                content = isf.ReadString(path);
            }

            return content;
        }

        /// <summary>
        /// Reads the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static Stream ReadStream(string path)
        {
            Stream stream = null;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(path))
                {
                    stream = isf.ReadStream(path);
                }
            }

            return stream;
        }

        internal static string ReadString(this IsolatedStorageFile isf, string path)
        {
            string s;
#if DEBUG
            s = ReadStringNotSafe(isf, path);
#else
            try
            {
                s = ReadStringNotSafe(isf, path);
            }
            catch (Exception ex)
            {
                throw new IOException("Exception occured while reading string from file " + path, ex);
            }
#endif
            return s;
        }

        private static string ReadStringNotSafe(IsolatedStorageFile isf, string path)
        {
            string s;
            using (IsolatedStorageFileStream isfs = isf.OpenFile(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(isfs))
                {
                    s = sr.ReadToEnd();
                    sr.Close();
                }

                isfs.Close();
            }

            return s;
        }

        internal static Stream ReadStream(this IsolatedStorageFile isf, string path)
        {
            MemoryStream ms;
#if DEBUG
            ms = ReadStreamNotSafe(isf, path);
#else
            try
            {
                ms = ReadStreamNotSafe(isf, path);
            }
            catch (Exception ex)
            {
                throw new IOException("Exception occured while reading string from file " + path, ex);
            }
#endif

            return ms;
        }

        /// <summary>
        /// Reads the stream not safe.
        /// </summary>
        /// <param name="isf">The isf.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static MemoryStream ReadStreamNotSafe(IsolatedStorageFile isf, string path)
        {
            MemoryStream ms;
            using (IsolatedStorageFileStream isfs = isf.OpenFile(path, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[isfs.Length];
                isfs.Read(data, 0, data.Length);

                ms = new MemoryStream(data);

                isfs.Close();
            }

            return ms;
        }

        /// <summary>
        /// Items the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static bool ItemExists(string path)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return iso.FileExists(path);
            }
        }

        /// <summary>
        /// Creates the directory structure.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public static void CreateDirectoryStructure(string fileName)
        {
            // Get the IsoStore
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var folder = Path.GetDirectoryName(fileName);
                if (!isoStore.DirectoryExists(folder))
                {
                    isoStore.CreateDirectory(folder);
                }
            }
        }

        /// <summary>
        /// Saves to iso store.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        public static void SaveToIsoStore(string fileName, Stream stream)
        {
            byte[] data = ReadFully(stream, (int)stream.Length);
            stream.Close();

            SaveToIsoStore(fileName, data);
        }

        /// <summary>
        /// Saves to iso store.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">The data.</param>
        public static void SaveToIsoStore(string fileName, byte[] data)
        {
            CreateDirectoryStructure(fileName);

            // Get the IsoStore
            var isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                if (isoStore.FileExists(fileName))
                {
                    isoStore.DeleteFile(fileName);
                }

                // Write the file
                using (var bw = new BinaryWriter(isoStore.CreateFile(fileName), Encoding.UTF8))
                {
                    bw.Write(data);
                    bw.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }


        public static void DeleteFile(string file)
        {
            // Get the IsoStore
            var isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            if (isoStore.FileExists(file))
            {
                isoStore.DeleteFile(file);
            }

        }
#endif





    }


    /// <summary>
    /// Implement the <see cref="IEqualityComparer&lt;T&gt;"/>, this can be used to call extenstion method: Distinct.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private Func<T, V> func;
        private IEqualityComparer<V> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonEqualityComparer&lt;T, V&gt;"/> class.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <param name="comparer">The comparer.</param>
        public CommonEqualityComparer(Func<T, V> func, IEqualityComparer<V> comparer)
        {
            this.func = func;
            this.comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonEqualityComparer&lt;T, V&gt;"/> class.
        /// </summary>
        /// <param name="func">The func.</param>
        public CommonEqualityComparer(Func<T, V> func)
            : this(func, EqualityComparer<V>.Default)
        {
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(T x, T y)
        {
            return this.comparer.Equals(this.func(x), this.func(y));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
        /// </exception>
        public int GetHashCode(T obj)
        {
            return this.comparer.GetHashCode(this.func(obj));
        }
    }

#if NETFX_CORE

    public class Deployment
    {
        public class Current
        {
            public class Dispatcher
            {
                public static Windows.UI.Xaml.Controls.Page CurrentPage { get; set; }

                public static async void BeginInvoke(Action action)
                {
                    await CurrentPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
                     {
                         if (action != null)
                         {
                             try
                             {
                                 action();
                             }
                             catch (Exception)
                             {
                             }
                         }
                     }));
                }
            }
        }
    }
#endif

}
