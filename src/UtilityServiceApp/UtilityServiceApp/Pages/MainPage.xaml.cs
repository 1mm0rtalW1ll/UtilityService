using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Data.Entity;

namespace UtilityServiceApp.Pages
{
    public partial class MainPage : Page
    {
        private bool _isHistoryMode = false;

        public MainPage()
        {
            InitializeComponent();
            ShowWelcomeMessage();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitFilters();
            LoadData();
        }

        private void InitFilters()
        {
            var executors = AppData.db.Users.Where(u => u.RoleID == 2).ToList();
            executors.Insert(0, new Users { FullName = "Все сотрудники", UserID = 0 });
            ComboFilterExecutor.ItemsSource = executors;

            var buildings = AppData.db.Buildings.ToList();
            buildings.Insert(0, new Buildings { AddressLine = "Все адреса", BuildingID = 0 });
            ComboFilterAddress.ItemsSource = buildings;

            ComboFilterExecutor.SelectedIndex = 0;
            ComboFilterAddress.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                var query = AppData.db.Requests
                    .Include(r => r.Buildings)
                    .Include(r => r.Residents)
                    .Include(r => r.Users)
                    .Include(r => r.Statuses)
                    .AsQueryable();

                if (UserSession.CurrentUser == null)
                {
                    AdminPanel.Visibility = Visibility.Collapsed;
                    FiltersPanel.Visibility = Visibility.Collapsed;

                    ColAddress.Visibility = Visibility.Collapsed;
                    ColFio.Visibility = Visibility.Collapsed;
                    ColPhone.Visibility = Visibility.Collapsed;
                    ColDesc.Visibility = Visibility.Collapsed;
                    ColExecutor.Visibility = Visibility.Collapsed;
                }
                else
                {
                    FiltersPanel.Visibility = Visibility.Visible;

                    if (UserSession.CurrentUser.Roles.RoleName == "Сотрудник")
                    {
                        query = query.Where(r => r.ExecutorID == UserSession.CurrentUser.UserID);
                        AdminPanel.Visibility = Visibility.Collapsed;
                        ColExecutor.Visibility = Visibility.Collapsed;
                        PanelFilterExecutor.Visibility = Visibility.Collapsed;
                    }
                    else if (UserSession.CurrentUser.Roles.RoleName == "Администратор")
                    {
                        AdminPanel.Visibility = Visibility.Visible;
                        PanelFilterExecutor.Visibility = Visibility.Visible;
                    }

                    if (PanelFilterExecutor.Visibility == Visibility.Visible && ComboFilterExecutor.SelectedValue is int execId && execId > 0)
                        query = query.Where(r => r.ExecutorID == execId);

                    if (ComboFilterAddress.SelectedValue is int buildId && buildId > 0)
                        query = query.Where(r => r.BuildingID == buildId);
                }

                if (_isHistoryMode) query = query.Where(r => r.StatusID == 3);

                DgRequests.ItemsSource = _isHistoryMode
                    ? query.OrderByDescending(r => r.CompletionDate).ToList()
                    : query.OrderBy(r => r.StatusID).ThenByDescending(r => r.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadData();

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            _isHistoryMode = true;
            LoadData();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _isHistoryMode = false;
            ComboFilterExecutor.SelectedIndex = 0;
            ComboFilterAddress.SelectedIndex = 0;
            LoadData();
        }

        private void DgRequests_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserSession.CurrentUser?.Roles.RoleName != "Администратор" || _isHistoryMode) return;

            if (DgRequests.SelectedItem is Requests selected)
                NavigationService.Navigate(new EditRequestPage(selected));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new EditRequestPage(null));

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                UserSession.CurrentUser = null;
                NavigationService.Navigate(new LoginPage());
            }
        }

        private async void ShowWelcomeMessage()
        {
            if (UserSession.CurrentUser != null)
            {
                TxtWelcome.Text = UserSession.CurrentUser.FullName;
                WelcomePopup.IsOpen = true;
                await Task.Delay(2500);
                WelcomePopup.IsOpen = false;
            }
        }
    }
}