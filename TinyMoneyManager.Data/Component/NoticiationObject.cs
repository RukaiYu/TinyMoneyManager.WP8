namespace TinyMoneyManager.Component
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class NoticiationObject : NotionObject
    {
        protected NoticiationObject()
        {
        }

        public virtual System.DateTime DueDate { get; set; }

        public virtual System.DateTime StartDate { get; set; }
    }
}

