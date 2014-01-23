namespace TinyMoneyManager.Component
{
    using System;

    public interface IMoney
    {
        decimal? GetMoney();

        CurrencyType Currency { get; set; }

        decimal Money { get; set; }
    }
}

