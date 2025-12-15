using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static bool IsLoggedIn => CurrentUserId > 0;

        // Simple logout method
        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUsername = "noname";
        }
    }

}
