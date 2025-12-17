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
using WpfAnimatedGif;
using System.Diagnostics;

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
        Layers targetLayer;
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
            RenderCollage();

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
                    collagenametb.Text = currentCollage.Title;
                    collagewidth.Text = currentCollage.Width.ToString();
                    collageheight.Text = currentCollage.Height.ToString();
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
                        usercontrol.MouseUp += LayerListSelect;
                        layerspanel.Children.Add(usercontrol);
                    }
                    //MessageBox.Show($"{layerspanel.Children.Count} слоев загружено"); //debug
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке коллажа: {ex.Message}, collageid{currentCollage.Id}");
                }
            }
            
        }

        private void RenderCollage()
        {
            if (currentCollage != null && collagecanvas != null)
            {
                try
                {
                    collagecanvas.Children.Clear();
                    currentLayers = currentCollage.Layers.OrderBy(x => x.LayerIndex).ToList();
                    if (!currentLayers.Any()) return;

                    double canvasWidth = collagecanvas.ActualWidth;
                    double canvasHeight = collagecanvas.ActualHeight;

                    if (canvasWidth <= 0 || canvasHeight <= 0) // канвас не отображается
                    {
                        return;
                    }


                    double scaleX = canvasWidth / currentCollage.Width;
                    double scaleY = canvasHeight / currentCollage.Height;
                    double scale = Math.Min(scaleX, scaleY); //чтобы понять в каком размере рендерить коллаж, доступное канвасу место может не совпадать с размерами коллажа
                    //коллаж рендерим по центру
                    double offsetX = (canvasWidth - (currentCollage.Width * scale)) / 2;
                    double offsetY = (canvasHeight - (currentCollage.Height * scale)) / 2;

                    foreach (var layer in currentLayers)
                    {
                        //if (layer.Resource == null) continue;

                        //MessageBox.Show($"ресурс {layer.Resource != null}");
                        //MessageBox.Show($"Path: {layer.Resource.ImagePath}");
                        try
                        {
                            Image layerImage = new Image
                            {
                                Opacity = (double)layer.Opacity,
                                RenderTransformOrigin = new Point(0.5, 0.5)
                            };

                            string imagePath = layer.Resource.ImagePath; 
                            string packUri = "pack://application:,,," + imagePath;

                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(packUri, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            double imageWidth = bitmap.PixelWidth;
                            double imageHeight = bitmap.PixelHeight;
                            layerImage.Width = imageWidth * (double)layer.XScale / 100 * scale;
                            layerImage.Height = imageHeight * (double)layer.YScale / 100 * scale;
                            Uri imageUri = new Uri(imagePath, UriKind.Relative);
                            ImageBehavior.SetAnimatedSource(layerImage, new BitmapImage(imageUri));
                            
                            if (layer.Rotation != 0)
                            {
                                layerImage.RenderTransform = new RotateTransform((double)layer.Rotation);
                            }

                            double xPos = offsetX + (layer.XOffset * scale);
                            double yPos = offsetY + (layer.YOffset * scale);

                            Canvas.SetLeft(layerImage, xPos);
                            Canvas.SetTop(layerImage, yPos);
                            layerImage.Tag = layer.Id;

                            //layerImage.MouseLeftButtonDown += LayerImage_MouseLeftButtonDown;
                            //layerImage.Cursor = Cursors.Hand;

                            collagecanvas.Children.Add(layerImage);

                        }
                        catch (Exception imgEx)
                        {
                            Console.WriteLine($"Ошибка слоя {layer.Id}: {imgEx.Message}");
                        }
                    }
                    //MessageBox.Show($"{collagecanvas.Children.Count} слоев на рендер");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отображении коллажа: {ex.Message}");
                }
            }
        }


        private void LayerListSelect(object sender, MouseButtonEventArgs e)
        {
            if (sender is CollageLayer control)
            {
                int layerId = (int)control.dblayerid;
                targetLayer = currentLayers.FirstOrDefault(l => l.Id == layerId);
                TargetLayerDisplayData();
            }
        }

        private void TargetLayerDisplayData()
        {
            if (targetLayer != null)
            {
                try
                {
                    targetname.Content = targetLayer.Resource.Title;
                    targetwidth.Text = targetLayer.XScale.ToString();
                    targetheight.Text = targetLayer.YScale.ToString();
                    targetopacity.Text = targetLayer.Opacity.ToString();
                    targetrot.Text = targetLayer.Rotation.ToString();
                    targetx.Text = targetLayer.XOffset.ToString();
                    targety.Text = targetLayer.YOffset.ToString();

                    string relativePath = targetLayer.Resource.ImagePath;
                    Uri imageUri = new Uri(relativePath, UriKind.Relative);
                    //Uri imageUri = new Uri("pack://application:,,," + relativePath, UriKind.Absolute);
                    ImageBehavior.SetAnimatedSource(targetimg, new BitmapImage(imageUri));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке слоя: {ex}");
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

        private void collagecanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderCollage();
        }
    }
}
