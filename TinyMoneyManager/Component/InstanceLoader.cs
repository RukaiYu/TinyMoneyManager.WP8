namespace TinyMoneyManager.Component
{
    using System;
    using System.Collections.Generic;

    public class InstanceLoader
    {
        private System.Collections.Generic.Dictionary<String, Object> dict = new System.Collections.Generic.Dictionary<String, Object>();

        public T LoadSingelton<T>(string key) where T : new()
        {
            if (!this.dict.ContainsKey(key))
            {
                this.dict[key] = (default(T) == null) ? System.Activator.CreateInstance<T>() : default(T);
            }
            return (T)this.dict[key];
        }

        public void Reset(string key, object obj)
        {
            this.dict[key] = obj;
        }
    }
}

