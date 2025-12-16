using micpix.Server;
using micpix.View.UserControls;
using micpix.View.Windows;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для CollageMakerWindow.xaml
    /// </summary>
    public partial class CollageMakerWindow : Window
    {
        private resourceuploadwindow uploadWindow;
        static Class1 db = new Class1();
        Collages currentCollage = db.Collages.Include(с => с.Layers).ThenInclude(l => l.Resource).First(); // ЗАГЛУШКА
        IEnumerable<Layers> currentLayers;
        IEnumerable<Resources> resset = db.ResourcesSet.Include(r => r.Author);
        IEnumerable<Resources> resset_filtered = db.ResourcesSet.Include(r => r.Author);
        public CollageMakerWindow()
        {
            InitializeComponent();
            try
            {
                using var db = new Class1();
                ObservableCollection<Resources> resset = new ObservableCollection<Resources>(db.ResourcesSet.Include(r => r.Author));
                ObservableCollection<Resources> resset_filtered = new ObservableCollection<Resources>(db.ResourcesSet.Include(r => r.Author));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
            LoadResources();
            LoadCollageLayers();

            pageheader.LoginAction = () =>
            {
                MessageBox.Show($"Здесь обязательно будет страница профиля для {App.CurrentUsername}", "Когда нибудь попозже");
            };
        }

        private void LoadCollageLayers()
        {
            if (currentCollage != null)
            {
                try
                {
                    layerspanel.Children.Clear();
                    currentLayers = currentCollage.Layers.OrderBy(x => x.LayerIndex);
                    foreach (var layer in currentLayers)
                    {
                        var usercontrol = new CollageLayer()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            layerindex = $"#{layer.LayerIndex.ToString()}",
                            imgsrc = layer.Resource.ImagePath,
                            assetname = layer.Resource.Title,
                            opacityvalue = layer.Opacity,
                            dblayerid = layer.Id
                        };
                        layerspanel.Children.Add(usercontrol);
                    }
                    MessageBox.Show($"{layerspanel.Children.Count} слоев загружено");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке коллажа: {ex.Message}, collageid{currentCollage.Id}");
                }
            }
            
        }

        private void LoadResources()
        {
            try
            {

                assetspanel.Children.Clear();

                foreach (var item in resset_filtered)
                {
                    var userControl = new MainpageElement()
                    {
                        Margin = new Thickness(15),
                        imgsrc = item.ImagePath,
                        title = item.Title,
                        author = item.Author.Username, //"rem", //PLACEHOLDER item.Author,
                        uploaddate = item.UploadDate.ToString("dd MMM") // Format date
                    };

                    assetspanel.Children.Add(userControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void SearchAssets(object sender, TextChangedEventArgs e)
        {
            string querytext = restextbox.Text;

            if (querytext.Trim() != null)
            {
                resset = db.ResourcesSet.Include(r => r.Author);

                resset_filtered = resset.Where(resource =>

                    resource.Title.Contains(querytext, StringComparison.OrdinalIgnoreCase)

                );
            }
            else
            {
                resset = db.ResourcesSet.Include(r => r.Author);
                resset_filtered = resset;
            }
            LoadResources();
        }

        private void Uploader_Window_Click(object sender, RoutedEventArgs e)
        {
            if (uploadWindow == null || !uploadWindow.IsLoaded)
            {
                uploadWindow = new resourceuploadwindow();
                uploadWindow.Show();
            }
            else
            {
                uploadWindow.Activate();
            }
        }
    }
}
