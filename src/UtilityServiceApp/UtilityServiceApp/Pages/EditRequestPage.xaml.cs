using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UtilityServiceApp.Pages
{
    public partial class EditRequestPage : Page
    {
        private Requests _current = new Requests();

        public EditRequestPage(Requests selected)
        {
            InitializeComponent();
            LoadData();

            if (selected != null)
            {
                _current = selected;
                TxtHeader.Text = $"Редактирование заявки №{_current.RequestID}";
                PanelCompletion.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Visible;

                if (_current.Residents != null)
                {
                    ComboResident.Text = _current.Residents.FullName;
                    TxtPhone.Text = _current.Residents.Phone;
                }
            }
            else
            {
                _current.CreatedAt = DateTime.Now;
                _current.StatusID = 1;
                TxtHeader.Text = "Новая заявка";
                PanelCompletion.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }

            DataContext = _current;
        }

        private void LoadData()
        {
            try
            {
                ComboBuilding.ItemsSource = AppData.db.Buildings.ToList();
                ComboResident.ItemsSource = AppData.db.Residents.ToList();
                ComboExecutor.ItemsSource = AppData.db.Users.Where(u => u.RoleID == 2).ToList();
                ComboStatus.ItemsSource = AppData.db.Statuses.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void ComboResident_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboResident.SelectedItem is Residents selected)
            {
                TxtPhone.Text = selected.Phone;
            }
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (ComboBuilding.SelectedItem == null) errors.AppendLine("Выберите дом!");
            if (string.IsNullOrWhiteSpace(_current.ApartmentNumber)) errors.AppendLine("Укажите квартиру!");
            if (string.IsNullOrWhiteSpace(ComboResident.Text)) errors.AppendLine("Введите ФИО жильца!");
            if (string.IsNullOrWhiteSpace(TxtPhone.Text)) errors.AppendLine("Укажите номер телефона!");
            if (string.IsNullOrWhiteSpace(_current.Description)) errors.AppendLine("Заполните описание!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var residentFromList = ComboResident.SelectedItem as Residents;
                if (residentFromList != null)
                {
                    _current.ResidentID = residentFromList.ResidentID;
                    residentFromList.Phone = TxtPhone.Text;
                }
                else
                {
                    Residents newRes = new Residents
                    {
                        FullName = ComboResident.Text,
                        Phone = TxtPhone.Text,
                        BuildingID = (int)ComboBuilding.SelectedValue,
                        ApartmentNumber = _current.ApartmentNumber
                    };
                    AppData.db.Residents.Add(newRes);
                    AppData.db.SaveChanges();
                    _current.ResidentID = newRes.ResidentID;
                }

                if (_current.RequestID == 0)
                    AppData.db.Requests.Add(_current);

                AppData.db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены", "Успех");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Удалить заявку №{_current.RequestID}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    AppData.db.Requests.Remove(_current);
                    AppData.db.SaveChanges();
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}