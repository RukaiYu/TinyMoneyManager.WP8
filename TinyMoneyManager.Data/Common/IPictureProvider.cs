namespace TinyMoneyManager.Data.Common
{
    using System;

    public interface IPictureProvider
    {
        string Comments { get; set; }

        string FileName { get; set; }

        System.Guid PictureId { get; set; }

        string Tag { get; set; }
    }
}

