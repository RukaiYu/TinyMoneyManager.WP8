namespace TinyMoneyManager.Pages.DialogBox.PictureManager
{
    using System;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    /// <summary>
    /// 
    /// </summary>
    public class PictureActionHandler : PageActionHandler<PictureInfo>
    {
        /// <summary>
        /// Called when [update picture].
        /// </summary>
        /// <param name="picInfo">The pic info.</param>
        public void OnUpdatePicture(PictureInfo picInfo)
        {
            base.OnUpdate(picInfo);
        }
    }
}

