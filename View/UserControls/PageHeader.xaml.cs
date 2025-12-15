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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace micpix.View.UserControls
{
    /// <summary>
    /// Логика взаимодействия для PageHeader.xaml
    /// </summary>
    public partial class PageHeader : UserControl
    {
        public Action LoginAction { get; set; }

        public string pagetitle
        {
            get { return (string)GetValue(pagetitleProperty); }
            set { SetValue(pagetitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for pagetitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty pagetitleProperty =
            DependencyProperty.Register("pagetitle", typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));



        public string logintext
        {
            get { return (string)GetValue(logintextProperty); }
            set { SetValue(logintextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for logintext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty logintextProperty =
            DependencyProperty.Register("logintext", typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));


        public string logosrc
        {
            get { return (string)GetValue(logosrcProperty); }
            set { SetValue(logosrcProperty, value); }
        }

        // Using a DependencyProperty as the backing store for logosrc.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty logosrcProperty =
            DependencyProperty.Register("logosrc", typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));



        public PageHeader()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoginClick(object sender, MouseButtonEventArgs e)
        {
            LoginAction?.Invoke();
        }
    }
}
