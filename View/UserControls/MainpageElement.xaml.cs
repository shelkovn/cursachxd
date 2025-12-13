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
    /// Логика взаимодействия для MainpageElement.xaml
    /// </summary>
    public partial class MainpageElement : UserControl
    {
        public MainpageElement()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string imgsrc { get; set; } = null!;
        public string author { get; set; } = null!;
        public string uploaddate { get; set; } = null!;
        public string title { get; set; } = null!;
        public MainpageElement(string imgsrc, string author, string uploaddate, bool contentLoaded)
        {
            this.imgsrc = imgsrc;
            this.author = author;
            this.uploaddate = uploaddate;
            this.title = title;
            _contentLoaded = contentLoaded;
        }
    }
}
