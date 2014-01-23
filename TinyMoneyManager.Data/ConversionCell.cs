namespace TinyMoneyManager.Data
{
    using System;
    using System.Runtime.CompilerServices;
    using TinyMoneyManager.Component;

    public class ConversionCell
    {
        public ConversionCell(CurrencyType currency, decimal rate)
        {
            this.Currency = currency;
            this.ConversionRate = rate;
        }

        public decimal ConversionRate { get; set; }

        public CurrencyType Currency { get; set; }
    }
}

