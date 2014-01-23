namespace TinyMoneyManager.ViewModels
{
    using System;

    public class NoteListItem
    {
        public string id;
        public string type;

        public NoteListItem(string id, string type)
        {
            this.id = id;
            this.type = type;
        }
    }
}

