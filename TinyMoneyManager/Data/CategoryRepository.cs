namespace TinyMoneyManager.Data
{
    using System;
    using TinyMoneyManager.Component;

    public class CategoryRepository
    {
        public CategoryRepository Instance
        {
            get
            {
                return ViewModelLocator.instanceLoader.LoadSingelton<CategoryRepository>("CategoryRepository");
            }
        }
    }
}

