namespace TinyMoneyManager.Data.Model
{
    using Microsoft.Phone.Data.Linq;
    using Microsoft.Phone.Data.Linq.Mapping;
    using RapidRepository;
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Xml.Serialization;
    using TinyMoneyManager.Component;
    using System.Linq;

    using NkjSoft.Extensions;
    using System.Collections.Generic;

    [Index(Columns = "Order", Name = "Category_Index_Order"), Table, XmlType]
    public class Category : NotionObject, IRapidEntity, IMoney, IOrderable
    {
        private EntitySet<AccountItem> _accountItems;
        private ItemType _categoryType;
        private decimal _defaultAmount;
        private bool? _favourite;
        private System.Guid _id;
        private bool _isParent;
        private string _name;
        private EntityRef<Category> _parentCategory;
        private System.Guid _parentCategoryId;
        private EntitySet<Category> childrens;
        public const string FavouriteIconPath = "/TinyMoneyManager;component/icons/appBar/favorate-write.rest.png";
        public const string NoFavouriteIconPath = "/TinyMoneyManager;component/images/heart_empty.rest.png";
        private int? order;

        public Category()
            : this(string.Empty)
        {
        }

        public Category(string name)
        {
            this.order = 0;
            System.Action<Category> onAdd = null;
            this._favourite = false;
            this._name = name;
            this._accountItems = new EntitySet<AccountItem>(new System.Action<AccountItem>(this.attach_ToDo), new System.Action<AccountItem>(this.detach_ToDo));
            if (onAdd == null)
            {
                onAdd = delegate(Category c)
                {
                    c.ParentCategory = this;
                };
            }
            this.childrens = new EntitySet<Category>(onAdd, delegate(Category c)
            {
                c.ParentCategory = null;
            });
        }

        private void attach_ToDo(AccountItem toDo)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            toDo.Category = this;
        }

        private void detach_ToDo(AccountItem toDo)
        {
            this.OnNotifyPropertyChanging("AccountItem");
            toDo.Category = null;
        }

        public override string ToString()
        {
            return this.CategoryInfo;
        }

        public static void UpdateDataContext_At_v190(DatabaseSchemaUpdater dataBaseUpdater)
        {
            dataBaseUpdater.AddColumn<Category>("Order");
            dataBaseUpdater.AddIndex<Category>("Category_Index_Order");
        }

        [XmlIgnore, Association(Storage = "_accountItems", OtherKey = "CategoryId", ThisKey = "Id")]
        public EntitySet<AccountItem> AccountItems
        {
            get
            {
                return this._accountItems;
            }
            set
            {
                this._accountItems.Assign(value);
            }
        }

        public string CategoryInfo
        {
            get
            {
                if (this.ParentCategory == null)
                {
                    return this.Name;
                }
                return (this.ParentCategory.Name + ">" + this.Name);
            }
        }

        [Column]
        public ItemType CategoryType
        {
            get
            {
                return this._categoryType;
            }
            set
            {
                this.OnNotifyPropertyChanging("CategoryType");
                this._categoryType = value;
                this.OnNotifyPropertyChanged("CategoryType");
            }
        }

        [XmlIgnore, Association(Storage = "childrens", OtherKey = "ParentCategoryId", ThisKey = "Id")]
        public EntitySet<Category> Childrens
        {
            get
            {
                return this.childrens;
            }
            set
            {
                this.childrens.Assign(value);
            }
        }

        [Column]
        public decimal DefaultAmount
        {
            get
            {
                return this._defaultAmount;
            }
            set
            {
                this.OnNotifyPropertyChanging("DefaultAmount");
                this._defaultAmount = value;
                this.OnNotifyPropertyChanged("DefaultAmount");
                this.OnNotifyPropertyChanged("DefaultAmountString");
            }
        }

        public string DefaultAmountString
        {
            get
            {
                return this._defaultAmount.ToString("F2");
            }
        }

        [Column(CanBeNull = true)]
        public bool? Favourite
        {
            get
            {
                return this._favourite;
            }
            set
            {
                this.OnNotifyPropertyChanging("Favourite");
                if (this._favourite != value)
                {
                    this._favourite = value;
                    this.OnNotifyPropertyChanged("Favourite");
                    this.OnNotifyPropertyChanged("FavouriteIconString");
                }
            }
        }

        public string FavouriteIconString
        {
            get
            {
                if (!this._favourite.Value)
                {
                    return string.Empty;
                }
                return "/TinyMoneyManager;component/icons/appBar/favorate-write.rest.png";
            }
        }

        [Column(IsPrimaryKey = true, DbType = "UniqueIdentifier", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public System.Guid Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this.OnNotifyPropertyChanging("Id");
                this._id = value;
                this.OnNotifyPropertyChanged("Id");
            }
        }

        public bool IsParent
        {
            get
            {
                this._isParent = this.ParentCategoryId == System.Guid.Empty;
                return this._isParent;
            }
        }

        [Column]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this.OnNotifyPropertyChanging("Name");
                this._name = value;
                this.OnNotifyPropertyChanged("Name");
                this.OnNotifyPropertyChanged("CategoryInfo");
            }
        }

        [Column(CanBeNull = true)]
        public int? Order
        {
            get
            {
                int? order = this.order;
                return new int?(order.HasValue ? order.GetValueOrDefault() : 0);
            }
            set
            {
                if (this.order != value)
                {
                    this.OnNotifyPropertyChanging("Order");
                    this.order = value;
                    this.OnNotifyPropertyChanged("Order");
                }
            }
        }

        [Association(Storage = "_parentCategory", ThisKey = "ParentCategoryId", OtherKey = "Id"), XmlIgnore]
        public Category ParentCategory
        {
            get
            {
                return this._parentCategory.Entity;
            }
            set
            {
                this.OnNotifyPropertyChanging("ParentCategory");
                this._parentCategory.Entity = value;
                if (value != null)
                {
                    this.ParentCategoryId = value.Id;
                }
                this.OnNotifyPropertyChanged("ParentCategory");
                this.OnNotifyPropertyChanged("CategoryInfo");
            }
        }

        [Column]
        public System.Guid ParentCategoryId
        {
            get
            {
                return this._parentCategoryId;
            }
            set
            {
                this.OnNotifyPropertyChanging("ParentCategoryId");
                this._parentCategoryId = value;
                this.OnNotifyPropertyChanged("ParentCategoryId");
            }
        }

        /// <summary>
        /// Gets the money.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public decimal? GetMoney()
        {
            return 0.0m;
        }

        public CurrencyType Currency
        {
            get;
            set;
        }

        public decimal Money
        {
            get;
            set;
        }


        private string _stasticsInfo;

        [XmlIgnore]
        /// <summary>
        /// Gets or sets the stastics info.
        /// </summary>
        /// <value>
        /// The stastics info.
        /// </value>
        public string StasticsInfoOfMonth
        {
            get { return _stasticsInfo; }
            set
            {
                if (value != _stasticsInfo)
                {
                    OnNotifyPropertyChanging("StasticsInfoOfMonth");
                    _stasticsInfo = value;
                    OnAsyncNotifyPropertyChanged("StasticsInfoOfMonth");
                }
            }
        }

        /// <summary>
        /// Updates the data context_ at_v199.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void UpdateDataContext_At_v199(TinyMoneyDataContext db)
        {
            try
            {
                var categories = db.Categories;

                ResetOrderFor(categories);

                db.SubmitChanges();
            }
            catch (Exception)
            {

            }
        }

        public static void ResetOrderFor(IEnumerable<Category> categories, bool fromChildren = true)
        {
            reorderForCategory(categories.Where(p => p.CategoryType == ItemType.Expense), fromChildren);
            reorderForCategory(categories.Where(p => p.CategoryType == ItemType.Income), fromChildren);
        }

        private static void reorderForCategory(IEnumerable<Category> categories, bool fromChildren = false)
        {
            categories
                .Where(p => p.IsParent)
                .ForEach((p, i) =>
                {
                    p.Order = i + 1;

                    List<Category> childrenItems = null;

                    if (fromChildren)
                    {
                        childrenItems = p.Childrens.AsEnumerable().ToList();
                    }
                    else
                    {
                        childrenItems = categories.Where(c => c.ParentCategoryId == p.Id)
                            .ToList();
                    }

                    childrenItems
                        .ForEach((x, y) =>
                        {
                            if (x.Order.GetValueOrDefault() == 0)
                            {
                                x.Order = y + 1;
                            }
                        });
                });
        }

    }
}

