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
    /// Логика взаимодействия для CollageLayer.xaml
    /// </summary>
    public partial class CollageLayer : UserControl
    {


        public string imgsrc
        {
            get { return (string)GetValue(imgsrcProperty); }
            set { SetValue(imgsrcProperty, value); }
        }

        // Using a DependencyProperty as the backing store for imgsrc.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty imgsrcProperty =
            DependencyProperty.Register("imgsrc", typeof(string), typeof(CollageLayer), new PropertyMetadata("/Source/icon.png"));



        public string layerindex
        {
            get { return (string)GetValue(layerindexProperty); }
            set { SetValue(layerindexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for layerindex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty layerindexProperty =
            DependencyProperty.Register("layerindex", typeof(string), typeof(CollageLayer), new PropertyMetadata("#0"));



        public string assetname
        {
            get { return (string)GetValue(assetnameProperty); }
            set { SetValue(assetnameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for assetname.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty assetnameProperty =
            DependencyProperty.Register("assetname", typeof(string), typeof(CollageLayer), new PropertyMetadata("unknown"));



        public int opacityvalue
        {
            get { return (int)GetValue(opacityvalueProperty); }
            set { SetValue(opacityvalueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for opacityvalue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty opacityvalueProperty =
            DependencyProperty.Register("opacityvalue", typeof(int), typeof(CollageLayer), new PropertyMetadata(100));



        public CollageLayer()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
