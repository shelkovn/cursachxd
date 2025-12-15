using micpix.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1;

namespace micpix.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private LoginWindow loginWindow;

        public RegistrationWindow()
        {
            InitializeComponent();

            pageheader.LoginAction = () =>
            {

                if (loginWindow == null || !loginWindow.IsLoaded)
                {
                    loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    loginWindow.Activate();
                    this.Close();
                }

            };
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernametextbox.Text.Trim();
            string password = passwordbox.Password;
            string confirmPassword = passwordbox2.Password;

            // проверить введенные данные
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // попытка добавить пользователя в базу
            try
            {
                using (var db = new Class1())
                {
                    bool usernameExists = db.UserSet.Any(u => u.Username == username);
                    if (usernameExists)
                    {
                        MessageBox.Show("Имя пользователя уже занято", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        usernametextbox.Focus();
                        usernametextbox.SelectAll();
                        return;
                    }

                    var newUser = new Users
                    {
                        Username = username,
                        RegistrationDate = DateTime.Now
                    };

                    db.UserSet.Add(newUser);
                    db.SaveChanges();

                    // зашифровать пароль
                    var (hash, salt) = PasswordHelper.HashPassword(password);

                    var credential = new UserCredential
                    {
                        UserId = newUser.Id,
                        PasswordHash = hash,
                        Salt = salt
                    };

                    db.UserCredentials.Add(credential);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешна!\nТеперь вы можете войти в систему.",
                                  "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
