using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMoneyManager.Controls.CalcutorKeyBoard;

namespace TinyMoneyManager.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public interface IKeyHandler
    {
        void HandleChar(CustomizedKey charWord);

        event EventHandler<EventArgs> Confirmed;

        double Result { get; set; }
    }

    public static class KeyHandlerExtension
    {
        /// <summary>
        /// Creates the key.
        /// </summary>
        /// <param name="keyHandler">The key handler.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static Controls.KeyBoardKey CreateKey(this IKeyHandler keyHandler, CustomizedKey key, int rowIndex = 0, int colIndex = 0)
        {
            var keyControl = new KeyBoardKey(key, keyHandler);

            keyControl.GridRowIndex = rowIndex;
            keyControl.GridColumnIndex = colIndex;

            return keyControl;
        }
    }
}
