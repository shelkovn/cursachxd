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
using System.IO;
using WpfApp1;
using WpfAnimatedGif;
using System.Diagnostics;
using ImageMagick;

namespace micpix.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для CollageMakerWindow.xaml
    /// </summary>
    public partial class CollageMakerWindow : Window
    {
        private resourceuploadwindow uploadWindow;
        public MainWindow mainwindow;
        static Class1 db = new Class1();
        public Collages currentCollage; //= db.Collages.Include(с => с.Layers).ThenInclude(l => l.Resource).First(); // ЗАГЛУШКА
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
            InitializeLayerTextHandlers();
            pageheader.LoginAction = () =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вернуться на главную? Несохраненные изменения будут утеряны",
                    "Подтвердите действие",
                    MessageBoxButton.YesNo
                );
                if (result == MessageBoxResult.Yes)
                {
                    if (mainwindow != null)
                    {
                        mainwindow.Show();
                        this.Close();
                    }
                    else
                    {
                        mainwindow = new MainWindow();
                        mainwindow.Show();
                        this.Close();
                    }
                }
            };
        }

        public void LoadCollageLayers()
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
                    if (currentLayers.Any())
                    {
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
                    }
                    //MessageBox.Show($"{layerspanel.Children.Count} слоев загружено"); //debug
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке коллажа: {ex.Message}, collageid {currentCollage.Id}");
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

                    Rectangle border = new Rectangle
                    {
                        Width = currentCollage.Width * scale,
                        Height = currentCollage.Height * scale,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection() { 5, 5 }, 
                        Opacity = 0.7
                    };

                    Canvas.SetLeft(border, offsetX);
                    Canvas.SetTop(border, offsetY);
                    Panel.SetZIndex(border, -10);
                    collagecanvas.Children.Add(border);

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
                                RenderTransformOrigin = new Point(0.5, 0.5),
                                Stretch = Stretch.Fill
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

        private static async Task RenderCollageGifAsync(Collages collage, IProgress<double> progress = null, bool updatepreview = false, int outputFps = 30, string outputPath = null)
        {
            await Task.Run(async () =>
            {
                var layers = collage.Layers.OrderBy(x => x.LayerIndex).ToList();
                if (!layers.Any()) return;

                var gifCollections = new List<MagickImageCollection>();
                var gifDurations = new List<int>();

                string projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

                foreach (var layer in layers)
                {
                    try
                    {
                        //string imagePath = layer.Resource.ImagePath;
                        //string packUri = "pack://application:,,," + imagePath;
                        string fullPath = System.IO.Path.Combine(projectDir, layer.Resource.ImagePath.TrimStart('/').Replace('/', '\\'));
                        var collection = new MagickImageCollection(fullPath);
                        gifCollections.Add(collection);
                        int gifDurationMs = 0;
                        //MessageBox.Show($"{layer.Resource.Title} кадров {collection.Count()}");
                        foreach (var frame in collection)
                        {
                            gifDurationMs += (int)frame.AnimationDelay * 10;
                        }
                        gifDurations.Add(gifDurationMs);
                    }
                    catch
                    {
                        // для не гифок
                        //string imagePath = layer.Resource.ImagePath;
                        //string packUri = "pack://application:,,," + imagePath;
                        string fullPath = System.IO.Path.Combine(projectDir, layer.Resource.ImagePath.TrimStart('/').Replace('/', '\\'));
                        var staticImage = new MagickImage(fullPath);
                        var collection = new MagickImageCollection();
                        collection.Add(staticImage);
                        gifCollections.Add(collection);
                        gifDurations.Add(1000);
                    }
                }

                int lcmDuration = FindLCM(gifDurations.Where(d => d > 0).ToArray());
                int maxDuration = 15000; // 15 секунд максимум
                lcmDuration = Math.Min(lcmDuration, maxDuration);

                int outputFrameDelayMs = 1000 / outputFps;
                int totalOutputFrames = (int)Math.Ceiling((double)lcmDuration / outputFrameDelayMs);

                //MessageBox.Show($"frames {totalOutputFrames}");

                var result = new MagickImageCollection();
                for (int frameIndex = 0; frameIndex < totalOutputFrames; frameIndex++)
                {
                    int currentTimeMs = frameIndex * outputFrameDelayMs;

                    var frameCanvas = new MagickImage(MagickColors.Transparent, (uint)collage.Width, (uint)collage.Height);
                    frameCanvas.BackgroundColor = MagickColors.Transparent;
                    frameCanvas.VirtualPixelMethod = VirtualPixelMethod.Transparent;
                    frameCanvas.Alpha(AlphaOption.On);
                    for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
                    {
                        var layer = layers[layerIndex];
                        var collection = gifCollections[layerIndex];
                        int frameToUse = GetFrameIndexAtTime(collection, currentTimeMs);

                        using (var layerFrame = (MagickImage)collection[frameToUse].Clone())
                        {
                            layerFrame.BackgroundColor = MagickColors.Transparent;
                            layerFrame.VirtualPixelMethod = VirtualPixelMethod.Transparent;
                            if (layer.XScale != 100 || layer.YScale != 100)
                            {
                                int newWidth = (int)(layerFrame.Width * (double)layer.XScale / 100);
                                int newHeight = (int)(layerFrame.Height * (double)layer.YScale / 100);
                                layerFrame.Resize(new MagickGeometry((uint)newWidth, (uint)newHeight)
                                {
                                    IgnoreAspectRatio = true  // THIS IS THE KEY
                                });//(uint)newWidth, (uint)newHeight);
                            }

                            if (layer.Rotation != 0)
                            {
                                layerFrame.Rotate((double)layer.Rotation);
                            }

                            if (layer.Opacity < 1.00m)
                            {
                                layerFrame.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, (double)layer.Opacity);
                            }

                            frameCanvas.Composite(layerFrame,
                                layer.XOffset,
                                layer.YOffset,
                                CompositeOperator.Over);
                        }
                    }

                    frameCanvas.AnimationDelay = (uint)outputFrameDelayMs / 10; // 1/100 секунды
                    frameCanvas.Trim();
                    frameCanvas.GifDisposeMethod = GifDisposeMethod.Background;
                    result.Add(frameCanvas);
                    if (progress != null)
                    {
                        double currentProgress = (double)frameIndex / totalOutputFrames;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progress.Report(currentProgress);
                        });

                        await Task.Delay(1).ConfigureAwait(false);
                    }
                }

                result.Coalesce();
                result.Optimize();
                string fullOutputPath;
                if (outputPath == null)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    outputPath = $"/Source/Previews/Collage_{collage.Id}_{timestamp}.gif";
                    fullOutputPath = System.IO.Path.Combine(projectDir, outputPath.TrimStart('/').Replace('/', '\\'));
                }
                else
                {
                    fullOutputPath = System.IO.Path.Combine(projectDir, outputPath.TrimStart('/').Replace('/', '\\'));
                }
                result.Write(fullOutputPath);
                if (updatepreview)
                {
                    try
                    {
                        var resultGif = new ResultGIFs
                        {
                            CollageId = collage.Id,
                            FilePath = outputPath, 
                            FrameCount = totalOutputFrames,
                            CreatedAt = DateTime.Now
                        };

                        db.ResultGIFs.Add(resultGif);
                        db.SaveChanges();

                    }
                    catch (Exception dbEx)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Ошибка при добавлении в базу: {dbEx.Message}");
                        });
                    }
                }
                progress?.Report(1);
                MessageBox.Show($"Успешно сохранено в {fullOutputPath}");
                // очистить память
                foreach (var collection in gifCollections)
                {
                    collection.Dispose();
                }
            });
        }

        private static int GetFrameIndexAtTime(MagickImageCollection gif, int timeMs)
        {
            if (gif.Count <= 1)
                return 0;

            int totalDurationMs = 0;
            foreach (var frame in gif)
            {
                totalDurationMs += (int)frame.AnimationDelay * 10;
            }

            timeMs = timeMs % totalDurationMs;
            int accumulatedMs = 0;
            for (int i = 0; i < gif.Count; i++) // считает время после каждого кадра, пока не найдет, время какого кадра > времени гифки, возвращает номер кадра
            {
                accumulatedMs += (int)gif[i].AnimationDelay * 10;
                if (timeMs < accumulatedMs)
                    return i;
            }

            return gif.Count - 1; // последний кадр если ничего не получилось
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
                        uploaddate = item.UploadDate.ToString("dd MMM"),
                        itemid = item.Id
                    };
                    userControl.MouseLeftButtonUp += AddResource;
                    assetspanel.Children.Add(userControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void AddResource(object sender, MouseButtonEventArgs e)
        {
            if (sender is MainpageElement control)
            {
                int resourceid = (int)control.itemid;

                using (var db = new Class1()) // Create new context for this operation
                {
                    // Load resources and collage with the SAME context
                    Resources res = db.ResourcesSet.Find(resourceid);
                    var collage = db.Collages.Find(currentCollage.Id);

                    var layer = new Layers()
                    {
                        ResourceId = resourceid,
                        Resource = res,
                        Collage = collage,
                        CollageId = currentCollage.Id,
                        CreatedAt = DateTime.Now,
                        LayerIndex = db.Layers.Where(l => l.CollageId == currentCollage.Id).Count(),
                        Opacity = 1,
                        XOffset = 0,
                        YOffset = 0,
                        XScale = 100, 
                        YScale = 100,
                        Rotation = 0,
                    };

                    db.Layers.Add(layer);
                    db.SaveChanges();

                    if (currentCollage.Layers == null)
                        currentCollage.Layers = new List<Layers>();
                    currentCollage.Layers.Add(layer);

                    targetLayer = layer;
                }

                LoadCollageLayers();
                TargetLayerDisplayData();
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

        //математика чтобы найти оптимальное число кадров чтобы все гифки зациклились ровно
        private static int LCM(int a, int b)
        {
            if (a == 0 || b == 0) return 0;
            return Math.Abs(a * b) / GCD(a, b);
        }

        private static int GCD(int a, int b)
        {
            if (a == 0 && b == 0) return 1; 
            if (a == 0) return b;
            if (b == 0) return a;

            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Math.Abs(a);
        }

        private static int FindLCM(int[] numbers)
        {
            if (numbers.Length == 0) return 0;

            int lcm = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
            {
                lcm = LCM(lcm, numbers[i]);
            }
            return lcm;
        }

        private async void SaveResultButton(object sender, RoutedEventArgs e)
        {
            if (currentCollage == null || !currentLayers.Any())
            {
                MessageBox.Show("Сперва создайте коллаж");
                return;
            }

            try
            {
                var dbCollage = db.Collages
                    .Include(c => c.Layers)
                    .FirstOrDefault(c => c.Id == currentCollage.Id);

                if (dbCollage == null)
                {
                    MessageBox.Show("Коллаж не найден");
                    return;
                }

                int updatedCount = 0;
                foreach (var uiLayer in currentLayers)
                {
                    var dbLayer = dbCollage.Layers.FirstOrDefault(l => l.Id == uiLayer.Id);

                    if (dbLayer != null)
                    {
                        dbLayer.LayerIndex = uiLayer.LayerIndex;
                        dbLayer.XOffset = uiLayer.XOffset;
                        dbLayer.YOffset = uiLayer.YOffset;
                        dbLayer.XScale = uiLayer.XScale;
                        dbLayer.YScale = uiLayer.YScale;
                        dbLayer.Rotation = uiLayer.Rotation;
                        dbLayer.Opacity = uiLayer.Opacity;

                        updatedCount++;
                    }
                }
                dbCollage.UpdatedAt = DateTime.Now;
                dbCollage.Title = collagenametb.Text.Trim();
                db.SaveChanges();

                //MessageBox.Show($"Обновлено {updatedCount} слоев");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении слоев: {ex.Message}");
            }

            var progress = new Progress<double>(percent =>
            {
                LoadingProgressBar.Value = percent * 100;
                StatusText.Content = $"Сохранение: {percent:P0}";
            });

            await RenderCollageGifAsync(currentCollage, progress, true); 
        }

        private void InitializeLayerTextHandlers()
        {
            targetx.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (int.TryParse(targetx.Text, out int x))
                {
                    targetLayer.XOffset = x;
                    UpdateLayerInDatabase();
                    RenderCollage(); 
                }
                else
                {
                    targetx.Text = new string(targetx.Text.Where(char.IsDigit).ToArray());
                }
            };

            targety.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (int.TryParse(targety.Text, out int y))
                {
                    targetLayer.YOffset = y;
                    UpdateLayerInDatabase();
                    RenderCollage();
                }
                else
                {
                    targety.Text = new string(targety.Text.Where(char.IsDigit).ToArray());
                }
            };

            targetwidth.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (decimal.TryParse(targetwidth.Text, out decimal scaleX))
                {
                    scaleX = Math.Clamp(scaleX, 0, 1500);
                    targetLayer.XScale = scaleX;
                    UpdateLayerInDatabase();
                    RenderCollage();
                }
                else
                {
                    targetwidth.Text = new string(targetwidth.Text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                }
            };

            targetheight.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (decimal.TryParse(targetheight.Text, out decimal scaleY))
                {
                    scaleY = Math.Clamp(scaleY, 0, 1500);
                    targetLayer.YScale = scaleY;
                    UpdateLayerInDatabase();
                    RenderCollage();
                }
                else
                {
                    targetheight.Text = new string(targetheight.Text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                }
            };

            targetrot.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (decimal.TryParse(targetrot.Text, out decimal rotation))
                {
                    rotation = rotation % 360;
                    if (rotation < 0) rotation += 360;
                    targetLayer.Rotation = rotation;
                    UpdateLayerInDatabase();
                    RenderCollage();
                }
                else
                {
                    targetrot.Text = new string(targetrot.Text.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());
                }
            };

            targetopacity.TextChanged += (s, e) =>
            {
                if (targetLayer == null) return;
                if (decimal.TryParse(targetopacity.Text, out decimal opacity))
                {
                    // Limit to 0.00 - 1.00
                    opacity = Math.Clamp(opacity, 0, 1);
                    targetLayer.Opacity = opacity;
                    UpdateLayerInDatabase();
                    RenderCollage();
                }
                else
                {
                    targetopacity.Text = new string(targetopacity.Text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                }
            };
        }

        private void UpdateLayerInDatabase()
        {
            if (targetLayer == null) return;

            try
            {
                using (var db = new Class1())
                {
                    var dbLayer = db.Layers.Find(targetLayer.Id);
                    if (dbLayer != null)
                    {
                        dbLayer.XOffset = targetLayer.XOffset;
                        dbLayer.YOffset = targetLayer.YOffset;
                        dbLayer.XScale = targetLayer.XScale;
                        dbLayer.YScale = targetLayer.YScale;
                        dbLayer.Rotation = targetLayer.Rotation;
                        dbLayer.Opacity = targetLayer.Opacity;

                        db.SaveChanges();

                        var collageLayer = currentCollage?.Layers?.FirstOrDefault(l => l.Id == targetLayer.Id);
                        if (collageLayer != null)
                        {
                            collageLayer.XOffset = targetLayer.XOffset;
                            collageLayer.YOffset = targetLayer.YOffset;
                            collageLayer.XScale = targetLayer.XScale;
                            collageLayer.YScale = targetLayer.YScale;
                            collageLayer.Rotation = targetLayer.Rotation;
                            collageLayer.Opacity = targetLayer.Opacity;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка слоя: {ex.Message}");
            }
        }

    }
}
