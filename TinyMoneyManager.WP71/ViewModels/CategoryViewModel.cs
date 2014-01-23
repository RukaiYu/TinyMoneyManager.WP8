namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using NkjSoft.Extensions;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.ViewModels.Common;


    public class CategorySummaryGroup : ObjectGroupingViewModel<string, Category>
    {
        public CategorySummaryGroup(string key)
            : base(key)
        {

        }

        public override string TotalAmount
        {
            get
            {
                return string.Empty;
            }
        }
    }

    public class CategoryViewModel : NkjSoftViewModelBase
    {
        public System.Action<PageActionType, Category> AddOrRemoveFavouriteCallback;

        private ObservableCollection<Category> _parents;

        public ObservableCollection<Category> Parents
        {
            get { return _parents; }
            set
            {
                if (value != _parents)
                {
                    OnNotifyPropertyChanging("Parents");
                    _parents = value;
                    OnNotifyPropertyChanged("Parents");
                }
            }
        }

        public CategoryViewModel()
        {
            this.Parents = new ObservableCollection<Category>();
            this.Categories = new ObservableCollection<Category>();
            base.IsDataLoaded = false;
        }

        public void AddCategory(Category category)
        {
            if (category.Order.GetValueOrDefault() == 0)
            {
                var maxOrder = category.Order;

                if (category.IsParent)
                {
                    maxOrder = AccountBookDataContext.Categories
                        .Where(p => p.CategoryType == category.CategoryType && p.ParentCategoryId == System.Guid.Empty)
                        .Max(p => p.Order).GetValueOrDefault();
                }
                else
                {
                    maxOrder = category.ParentCategory.Childrens.Max(p => p.Order).GetValueOrDefault();
                }

                maxOrder = maxOrder + 1;

                category.Order = maxOrder;
            }

            this.Categories.Add(category);
            if (category != null && category.IsParent)
            {
                this.Parents.Add(category);
            }

            this.AccountBookDataContext.Categories.InsertOnSubmit(category);
            this.AccountBookDataContext.SubmitChanges();
        }

        public void AddFavouriteItem(Category item)
        {
            this.RaiseAddOrRemoveFavouriteCallback(PageActionType.Add, item);
            this.AccountBookDataContext.SubmitChanges();
        }

        public void AddRange(IEnumerable<Category> categories)
        {
            System.Collections.Generic.List<Guid> list = new System.Collections.Generic.List<Guid>();
            if (this.AccountBookDataContext.Categories != null)
            {
                list = (from p in this.AccountBookDataContext.Categories select p.Id).ToList<System.Guid>();
            }

            foreach (Category category2 in categories)
            {
                if ((list.Count <= 0) || !list.Contains(category2.Id))
                {
                    Category category3 = new Category
                    {
                        Id = category2.Id,
                        ParentCategoryId = (category2.ParentCategory == null) ? category2.ParentCategoryId : category2.ParentCategory.Id,
                        Name = category2.Name,
                        CategoryType = category2.CategoryType,
                        DefaultAmount = category2.DefaultAmount,
                        Order = category2.Order,
                    };
                    Category item = category3;
                    this.Categories.Add(item);
                    this.AccountBookDataContext.Categories.InsertOnSubmit(item);
                }
            }
            this.AccountBookDataContext.SubmitChanges();
        }

        public decimal CountForSettleAmountForBudgetProject(SearchingScope scope = SearchingScope.CurrentMonth, params Category[] categorys)
        {
            DetailsCondition dc = new DetailsCondition
            {
                SearchingScope = scope
            };
            decimal num = 0.0M;
            foreach (Category category in categorys)
            {
                num += this.CountStatistic(category, dc, null);
            }
            return num;
        }

        public decimal CountStatistic(Category category, DetailsCondition dc, System.Action<Int32> callBack = null)
        {
            IQueryable<AccountItem> source = from p in this.AccountBookDataContext.AccountItems.AsQueryable<AccountItem>()
                                             where (p.CreateTime.Date >= dc.StartDate.Value.Date) && (p.CreateTime.Date <= dc.EndDate.Value.Date)
                                             select p;
            if (callBack == null)
            {
                callBack = delegate(int i)
                {
                };
            }
            if (category.IsParent)
            {
                System.Collections.Generic.List<Guid> childIds = (from p in this.AccountBookDataContext.Categories
                                                                  where p.ParentCategoryId == category.Id
                                                                  select p.Id).ToList<System.Guid>();
                source = from p in source
                         where childIds.Contains(p.CategoryId)
                         select p;
            }
            else
            {
                source = from p in source
                         where p.CategoryId == category.Id
                         select p;
            }

            callBack(source.Count());

            return source.AsEnumerable().Sum(p => p.GetMoney()).GetValueOrDefault();
        }

        public override void Delete<T>(T obj)
        {
            base.Delete<T>(obj);
            this.Categories.Remove(obj as Category);
        }

        public bool EnsureUsingCategory(Category category)
        {
            if (category.IsParent)
            {
                return (this.AccountBookDataContext.AccountItems.Count(p => p.Category != null && p.Category.ParentCategoryId == category.Id) > 0);
            }

            return (this.AccountBookDataContext.AccountItems.Count<AccountItem>(p => (p.CategoryId == category.Id)) > 0);
        }

        public bool Exists(System.Func<Category, Boolean> func)
        {
            return (this.Categories.Count<Category>(func) > 0);
        }

        public System.Collections.Generic.IEnumerable<Category> GetCategories(System.Func<Category, Boolean> resultSelector)
        {
            if (resultSelector != null)
            {
                return this.Categories.Where<Category>(resultSelector);
            }
            return (System.Collections.Generic.IEnumerable<Category>)this.Categories.GetEnumerator();
        }

        public System.Collections.Generic.IEnumerable<Category> GetChildCategories(Category parent)
        {
            return (from p in this.Categories
                    where p.ParentCategoryId == parent.Id
                    orderby p.Order descending
                    select p);
        }

        public System.Collections.Generic.IEnumerable<Category> GetDataFromDatabase(System.Func<Category, Boolean> resultSelector = null)
        {
            //if ((this.Categories != null) && (this.Categories.Count != 0))
            //{
            //    return this.GetCategories(resultSelector);
            //}

            if (resultSelector != null)
            {
                return this.AccountBookDataContext.Categories
                    .Where<Category>(resultSelector)
                    .OrderBy(p => p.Order)
                    .AsEnumerable<Category>();
            }

            return this.AccountBookDataContext.Categories;
        }

        public override void LoadData()
        {
            if (!base.IsDataLoaded)
            {
                if (!base.IsDataLoaded)
                {
                    IQueryable<Category> queryable = from category in this.AccountBookDataContext.Categories select category;
                    this.Categories = new ObservableCollection<Category>(from p in queryable
                                                                         orderby p.Order
                                                                         select p);
                }
                base.IsDataLoaded = true;
            }
        }

        protected void RaiseAddOrRemoveFavouriteCallback(PageActionType action, Category category)
        {
            if (this.AddOrRemoveFavouriteCallback != null)
            {
                this.AddOrRemoveFavouriteCallback(action, category);
            }
        }

        public void Remove(Category category)
        {
            this.Categories.Remove(category);
            this.AccountBookDataContext.Categories.DeleteOnSubmit(category);
            this.AccountBookDataContext.SubmitChanges();
        }

        internal void RemoveFavouriteItem(Category item)
        {
            item.Favourite = false;
            this.AccountBookDataContext.SubmitChanges();
        }

        public void ToggleFavorite(Category category)
        {
            this.RaiseAddOrRemoveFavouriteCallback(category.Favourite.Value ? PageActionType.Delete : PageActionType.Add, category);
            category.Favourite = new bool?(!category.Favourite.Value);
            this.AccountBookDataContext.SubmitChanges();
        }

        public void Update(Category category)
        {
            this.AccountBookDataContext.SubmitChanges();
        }

        public ObservableCollection<Category> Categories { get; set; }

        public bool HasLoadParents { get; set; }

        /// <summary>
        /// Loads the categories summary.
        /// </summary>
        /// <param name="itemType">Type of the item.</param>
        /// <returns></returns>
        internal List<CategorySummaryGroup> LoadCategoriesSummary(ItemType itemType, ref bool hasRows, bool showAllCategories = false)
        {
            var allCategories = AccountBookDataContext.Categories.AsQueryable();

            if (!showAllCategories)
            {
                allCategories = allCategories.Where(p => p.Favourite.GetValueOrDefault());
            }

            var result = new List<CategorySummaryGroup>();

            DetailsCondition dc = new DetailsCondition();
            dc.SearchingScope = SearchingScope.CurrentMonth;

            var allHaveRows = false;

            if (itemType == ItemType.All || itemType == ItemType.Expense)
            {
                var group = new CategorySummaryGroup(AppResources.Expense);

                var items = allCategories.Where(p => p.CategoryType == ItemType.Expense)
                    .ToList();

                dc.IncomeOrExpenses = itemType;

                allHaveRows = items.Count > 0;

                foreach (var category in items)
                {
                    CountStatistic(dc, category);
                }

                group.AddRange(items);

                result.Add(group);
            }

            if (itemType == ItemType.All || itemType == ItemType.Income)
            {
                var group = new CategorySummaryGroup(AppResources.Income);

                var items = allCategories.Where(p => p.CategoryType == ItemType.Income)
                    .ToList();


                if (!allHaveRows)
                {
                    allHaveRows = items.Count > 0;
                }

                foreach (var category in items)
                {
                    CountStatistic(dc, category);
                }

                group.AddRange(items);

                result.Add(group);
            }

            hasRows = allHaveRows;

            return result;
        }

        /// <summary>
        /// Counts the statistic.
        /// </summary>
        /// <param name="dc">The dc.</param>
        /// <param name="category">The category.</param>
        public void CountStatistic(DetailsCondition dc, Category category)
        {
            IQueryable<AccountItem> source = from p in this.AccountBookDataContext.AccountItems.AsQueryable<AccountItem>()
                                             where (p.CreateTime.Date > dc.StartDate.Value.Date) && (p.CreateTime.Date <= dc.EndDate.Value.Date)
                                             select p;

            if (category.IsParent)
            {
                System.Collections.Generic.List<Guid> childIds = (from p in this.AccountBookDataContext.Categories
                                                                  where p.ParentCategoryId == category.Id
                                                                  select p.Id).ToList<System.Guid>();
                source = from p in source
                         where childIds.Contains(p.CategoryId)
                         select p;
            }
            else
            {
                source = from p in source
                         where p.CategoryId == category.Id
                         select p;
            }

            System.Collections.Generic.IEnumerable<AccountItem> enumerable = source.AsEnumerable<AccountItem>();

            var sum = enumerable.Sum<AccountItem>(p => p.GetMoney()).GetValueOrDefault();
            var sumMoney = AppSetting.Instance.DefaultCurrency.GetCurrencySymbolWithMoney(sum);

            category.StasticsInfoOfMonth = AppResources.StatisticsInfoFormatterForCategory
                .FormatWith(enumerable.Count<AccountItem>(), string.Empty, sumMoney);
        }

    }
}

