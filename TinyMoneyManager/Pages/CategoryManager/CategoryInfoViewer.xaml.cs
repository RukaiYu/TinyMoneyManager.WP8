namespace TinyMoneyManager.Pages.CategoryManager
{
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using NkjSoft.Extensions;
    using NkjSoft.WPhone.Extensions;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using TinyMoneyManager;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Controls;
    using TinyMoneyManager.Data.Model;
    using TinyMoneyManager.Language;
    using TinyMoneyManager.Pages.DialogBox;

    public partial class CategoryInfoViewer : PhoneApplicationPage, INotifyPropertyChanged
    {
        public string actionName;

        private Category current;
        public string currentMonthName;

        private bool hasLoadStatisticsInfo;

        public string lastMonthName;


        public string symbol;

        public event PropertyChangedEventHandler PropertyChanged;

        public CategoryInfoViewer()
        {
            string str;
            this.currentMonthName = string.Empty;
            this.lastMonthName = string.Empty;
            this.actionName = string.Empty;
            this.symbol = string.Empty;
            this.InitializeComponent();
            TiltEffect.SetIsTiltEnabled(this, true);
            Category category = new Category
            {
                Name = AppResources.Loading
            };
            base.Name = str = AppResources.Loading;
            category.ParentCategory = new Category(str);
            category.Order = 0;
            category.DefaultAmount = 0.0M;
            this.current = category;
            base.DataContext = this;
            int index = System.DateTime.Now.Month - 1;
            this.currentMonthName = LocalizedStrings.CultureName.DateTimeFormat.MonthGenitiveNames[index].ToLowerInvariant();
            this.lastMonthName = LocalizedStrings.CultureName.DateTimeFormat.MonthGenitiveNames[index - 1].ToLowerInvariant();
        }

        private void CategoryInfoViewer_EditButton_Click(object sender, System.EventArgs e)
        {
            if (this.Current != null)
            {
                CategoryInfoEditor.Go(this, this.Current, this.Current.CategoryType, PageActionType.Edit);
            }
        }

        private void DetectFarvour()
        {
            ApplicationBarIconButton button = new ApplicationBarIconButton
            {
                IconUri = new Uri("/icons/appbar.{0}.rest.png".FormatWith(new object[] { this.current.Favourite.GetValueOrDefault() ? "heart" : "heart.broken" }), UriKind.RelativeOrAbsolute),
                Text = AppResources.Favourite
            };
            button.Click += new System.EventHandler(this.hearButton_Click);
            base.ApplicationBar.Buttons.Add(button);
        }

        public static void Go(System.Guid id, PhoneApplicationPage page)
        {
            page.NavigateTo("/pages/CategoryManager/CategoryInfoViewer.xaml?id={0}", new object[] { id });
        }

        private void hearButton_Click(object sender, System.EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            if (this.current != null)
            {
                bool valueOrDefault = this.current.Favourite.GetValueOrDefault();
                button.IconUri = new Uri("/icons/appbar.{0}.rest.png".FormatWith(new object[] { valueOrDefault ? "heart.broken" : "heart" }), UriKind.Relative);
                ViewModelLocator.CategoryViewModel.ToggleFavorite(this.current);
            }
        }

        private void InitializeAppBar()
        {
            System.Action<ApplicationBarIconButton>[] setters = new System.Action<ApplicationBarIconButton>[] { delegate (ApplicationBarIconButton p) {
                p.Text = AppResources.Edit;
            } };
            base.ApplicationBar.GetIconButtonFrom(0).SetPropertyValue(setters).Click += new System.EventHandler(this.CategoryInfoViewer_EditButton_Click);
            if ((this.current != null) && !this.current.IsParent)
            {
                ApplicationBarIconButton button = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/icons/appbar.transfering.rest.png", UriKind.RelativeOrAbsolute),
                    Text = AppResources.MoveTo
                };
                button.Click += new System.EventHandler(this.MoveCategoryToButton_Click);
                this.DetectFarvour();
                base.ApplicationBar.Buttons.Add(button);
            }
        }


        private void LoadCurrent()
        {
            System.Guid id = this.GetNavigatingParameter("id", null).ToGuid();
            this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Details.Trim().ToLowerInvariant() }));
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {

                Category item = ViewModelLocator.CategoryViewModel.AccountBookDataContext.Categories.FirstOrDefault<Category>(p => p.Id == id);
                this.loadStatisticsInfo(item);
                this.Dispatcher.BeginInvoke(delegate
                {
                    this.Current = item;
                    this.actionName = LocalizedStrings.GetLanguageInfoByKey(this.current.CategoryType.ToString());
                    this.IncomeOrExpenseDetailsPivot.Header = AppResources.BlankWithFormatter.FormatWith(new object[] { this.actionName, AppResources.Category }).ToLowerInvariant();
                    if (!this.current.IsParent)
                    {
                        this.MoneyCurrencyPanel.Visibility = Visibility.Visible;
                        this.ParentCategoryNameEditor.Visibility = Visibility.Visible;
                        this.OrderPanel.Visibility = Visibility.Visible;
                    }
                    this.InitializeAppBar();
                });
            });
        }

        private void LoadStasticsInfo(Category category)
        {
            int count = 0;
            DetailsCondition dc = new DetailsCondition
            {
                SearchingScope = SearchingScope.CurrentMonth
            };
            decimal val = ViewModelLocator.CategoryViewModel.CountStatistic(category, dc, delegate(int p)
            {
                count = p;
            });
            AccountItem ai = new AccountItem
            {
                SecondInfo = this.currentMonthName,
                ThirdInfo = AppResources.StatisticsInfoFormatterForCategory.FormatWith(new object[] { count, string.Empty, "{0}{1}".FormatWith(new object[] { this.symbol, val.ToMoneyF2() }) })
            };
            count = 0;
            DetailsCondition condition2 = new DetailsCondition
            {
                SearchingScope = SearchingScope.LastMonth
            };
            val = ViewModelLocator.CategoryViewModel.CountStatistic(category, condition2, delegate(int p)
            {
                count = p;
            });
            AccountItem lastMonthai = new AccountItem
            {
                SecondInfo = this.lastMonthName,
                ThirdInfo = AppResources.StatisticsInfoFormatterForCategory.FormatWith(new object[] { count, string.Empty, "{0}{1}".FormatWith(new object[] { this.symbol, val.ToMoneyF2() }) })
            };
            base.Dispatcher.BeginInvoke(delegate
            {
                this.CurrentMonthStaticsInfoPanel.DataContext = ai;
                this.LastMonthStaticsInfoPanel.DataContext = lastMonthai;
                this.hasLoadStatisticsInfo = true;
                this.WorkDone();
            });
            BudgetItem budgetItem = new BudgetItem
            {
                AssociatedCategory = category,
                BudgetType = category.CategoryType
            };
            DetailsCondition detailsCondition = new DetailsCondition
            {
                SearchingScope = SearchingScope.CurrentMonth
            };
            decimal monthlyBudgetAmount = ViewModelLocator.BudgetProjectViewModel.GetBudgetAmountForCategory(budgetItem, detailsCondition);
            base.Dispatcher.BeginInvoke(delegate
            {
                this.BudgetBlock.Text = "{0}{1}".FormatWith(new object[] { this.symbol, monthlyBudgetAmount.ToMoneyF2() });
            });
        }

        private void loadStatisticsInfo(Category category)
        {
            if (!this.hasLoadStatisticsInfo)
            {
                this.BusyForWork(AppResources.NowLoadingFormatter.FormatWith(new object[] { AppResources.Statistics }));
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
                {
                    if (category == null)
                    {
                        this.AlertNotification(AppResources.TheRecordMissing, null);
                        this.SafeGoBack();
                    }
                    this.LoadStasticsInfo(category);
                });
            }
        }

        private void MoneyCurrencyPanel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(this.DefaultAmountBlockTitle.Text, this.Current.DefaultAmountString, delegate(TextBox t)
            {
                t.SelectAll();
                t.InputScope = MoneyInputTextBox.NumberInputScope;
            }, delegate(string s)
            {
                if (s.ToDecimal() <= 0M)
                {
                    this.AlertNotification(AppResources.AmountShouldBeGreatThanZeroMessage, null);
                    return false;
                }
                return true;
            }, delegate(string s)
            {
                this.Current.DefaultAmount = s.ToDecimal();
                ViewModelLocator.CategoryViewModel.Update(this.Current);
            });
        }

        private void MoveCategoryToButton_Click(object sender, System.EventArgs e)
        {
            SelectParentCategoryPage.Go(this, this.Current);
        }

        private void NameEditorButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToEditValueInTextBoxEditorPage(AppResources.Name, this.Current.Name, delegate(TextBox t)
            {
                t.SelectAll();
            }, delegate(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    this.AlertNotification("{0} {1}".FormatWith(new object[] { AppResources.Name, AppResources.EmptyTextMessage }), null);
                    return false;
                }
                return true;
            }, delegate(string s)
            {
                this.Current.Name = s;
                ViewModelLocator.CategoryViewModel.Update(this.Current);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.symbol = AppSetting.Instance.DefaultCurrency.GetCurrentString();
                this.LoadCurrent();
            }
        }

        private void ParentCategoryNameEditor_Click(object sender, RoutedEventArgs e)
        {
            Go(this.current.ParentCategoryId, this);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MainPivot.SelectedIndex == 1)
            {
                this.loadStatisticsInfo(this.current);
            }
        }

        private void StatisticsInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SearchingScope scope = (SearchingScope)System.Enum.Parse(typeof(SearchingScope), (sender as HyperlinkButton).Tag.ToString(), true);
            SummaryDetails details2 = new SummaryDetails();
            AccountItem item = new AccountItem
            {
                Category = this.current
            };
            details2.Tag = item;
            SummaryDetails details = details2;
            DetailsCondition condition2 = new DetailsCondition
            {
                ChartGroupMode = ChartGroupMode.ByCategoryName,
                SearchingScope = scope,
                GroupCategoryMode = this.current.IsParent ? CategorySortType.ByParentCategory : CategorySortType.ByChildCategory
            };
            DetailsCondition searchingCondition = condition2;
            StatsticSummaryItemsViewer.Show(details, searchingCondition, this);
        }

        public Category Current
        {
            get
            {
                return this.current;
            }
            set
            {
                if (this.current != value)
                {
                    this.current = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Current"));
                    }
                }
            }
        }
    }
}

