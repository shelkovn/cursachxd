using micpix.Server;
using micpix.View.UserControls;
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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private RegistrationWindow regWindow;
        public LoginWindow()
        {
            InitializeComponent();

            pageheader.LoginAction = () =>
            {
                
                if (regWindow == null || !regWindow.IsLoaded)
                {
                    regWindow = new RegistrationWindow();
                    regWindow.Show();
                    this.Close();
                }
                else
                {
                    regWindow.Activate();
                    this.Close();
                }
                
            };

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernametextbox.Text.Trim();
            string password = passwordbox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                using (var db = new Class1())
                {
                    var user = db.UserSet.FirstOrDefault(u => u.Username == username);

                    if (user == null)
                    {
                        MessageBox.Show("Неверное имя пользователя");
                        return;
                    }

                    var credential = db.UserCredentials.FirstOrDefault(c => c.UserId == user.Id);

                    if (credential == null)
                    {
                        MessageBox.Show("Ошибка аккаунта, нет данных входа");
                        return;
                    }

                    bool isValid = PasswordHelper.VerifyPassword(
                        password,
                        credential.PasswordHash,
                        credential.Salt
                    );

                    if (isValid)
                    {
                        // Записать текущего пользователя
                        App.CurrentUserId = user.Id; 
                        App.CurrentUsername = user.Username; 

                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
