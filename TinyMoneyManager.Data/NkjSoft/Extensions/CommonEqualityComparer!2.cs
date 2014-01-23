namespace NkjSoft.Extensions
{
    using System;
    using System.Collections.Generic;

    public class CommonEqualityComparer<T, V> : System.Collections.Generic.IEqualityComparer<T>
    {
        private System.Collections.Generic.IEqualityComparer<V> comparer;
        private System.Func<T,V> func;

        public CommonEqualityComparer(System.Func<T,V> func) : this(func, System.Collections.Generic.EqualityComparer<V>.Default)
        {
        }

        public CommonEqualityComparer(System.Func<T,V> func, System.Collections.Generic.IEqualityComparer<V> comparer)
        {
            this.func = func;
            this.comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return this.comparer.Equals(this.func(x), this.func(y));
        }

        public int GetHashCode(T obj)
        {
            return this.comparer.GetHashCode(this.func(obj));
        }
    }
}

