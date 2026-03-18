using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UtilityServiceApp.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
         
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(PbPassword.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = AppData.db.Users.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == PbPassword.Password);

                if (user != null)
                {
                    UserSession.CurrentUser = user;
                    NavigationService.Navigate(new MainPage());
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            UserSession.CurrentUser = null;

            NavigationService.Navigate(new MainPage());
        }
    }
}