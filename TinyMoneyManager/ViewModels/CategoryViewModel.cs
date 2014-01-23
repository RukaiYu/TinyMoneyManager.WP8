namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

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

        public void AddRange(System.Collections.Generic.IEnumerable<Category> categories)
        {
            System.Collections.Generic.List<Guid> list = new System.Collections.Generic.List<Guid>();
            if (this.AccountBookDataContext.Categories != null)
            {
                list = (from p in this.AccountBookDataContext.Categories select p.Id).ToList<System.Guid>();
            }
            int num = 0;
            foreach (Category category2 in categories)
            {
                if ((list.Count <= 0) || !list.Contains(category2.Id))
                {
                    Category category3 = new Category
                    {
                        Id = category2.Id,
                        Order = new int?(num++),
                        ParentCategoryId = (category2.ParentCategory == null) ? category2.ParentCategoryId : category2.ParentCategory.Id,
                        Name = category2.Name,
                        CategoryType = category2.CategoryType,
                        DefaultAmount = category2.DefaultAmount
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
                                             where (p.CreateTime.Date > dc.StartDate.Value.Date) && (p.CreateTime.Date <= dc.EndDate.Value.Date)
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
            System.Collections.Generic.IEnumerable<AccountItem> enumerable = source.AsEnumerable<AccountItem>();
            callBack(enumerable.Count<AccountItem>());
            return enumerable.Sum<AccountItem>(p => p.GetMoney()).GetValueOrDefault();
        }

        public override void Delete<T>(T obj)
        {
            base.Delete<T>(obj);
            this.Categories.Remove(obj as Category);
        }

        public bool EnsureUsingCategory(Category category)
        {
            System.Func<Category, Boolean> predicate = null;
            if (category.IsParent)
            {
                if (predicate == null)
                {
                    predicate = p => p.ParentCategoryId == category.Id;
                }
                return (this.Categories.Count<Category>(predicate) > 0);
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
            if ((this.Categories != null) && (this.Categories.Count != 0))
            {
                return this.GetCategories(resultSelector);
            }
            if (resultSelector != null)
            {
                return this.AccountBookDataContext.Categories.Where<Category>(resultSelector).AsEnumerable<Category>();
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
                                                                         orderby p.Order descending
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
    }
}

