namespace TinyMoneyManager.Component.Common
{
    using System;

    public interface ITitleInfoListener
    {
        void NotifyFormat();

        string NavigateUri { get; set; }
    }
}

