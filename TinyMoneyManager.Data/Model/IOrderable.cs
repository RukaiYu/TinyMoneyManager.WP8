using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyMoneyManager.Data.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOrderable
    {
        int? Order { get; set; }
    }
}
