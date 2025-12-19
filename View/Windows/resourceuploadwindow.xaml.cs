using micpix.Server;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp1;

namespace micpix.View.Windows
{
    public partial class resourceuploadwindow : Window
    {
        private string selectedImagePath = "";
        private string imageFormat = "";
        private string imageDimensions = "";
        private string fileSize = "";
        private string frameCount = "-";
        public Image d;

        public resourceuploadwindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                LoadImageToCanvas(selectedImagePath);
                UpdateImageInfo(selectedImagePath);

                // по умолчанию название загруженного файла
                string fileName = System.IO.Path.GetFileNameWithoutExtension(selectedImagePath);
                assetNameTextBox.Text = fileName;
            }
        }

        private void LoadImageToCanvas(string imagePath)
        {
            try
            {
                // превью изображения (для гиф первый кадр)
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();

                Viewbox vb = new Viewbox
                {
                    //Stretch = Stretch.Uniform,
                    Width = imageCanvas.ActualWidth,
                    Height = imageCanvas.ActualHeight
                };
                System.Windows.Controls.Image img = new System.Windows.Controls.Image{Source = bitmap};
                vb.Child = img;
                imageCanvas.Children.Clear();
                imageCanvas.Children.Add(vb);

                vb.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = imageCanvas });
                vb.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = imageCanvas });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void UpdateImageInfo(string imagePath) //заполнить таблицу с информацией об элементе
        {
            try
            {
                FileInfo fileInfo = new FileInfo(imagePath);
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                imageFormat = System.IO.Path.GetExtension(imagePath).ToUpper().TrimStart('.');
                imageDimensions = $"{bitmap.PixelWidth}x{bitmap.PixelHeight}";
                fileSize = (fileInfo.Length / 1024f).ToString("0.0") + " KB";
                frameCount = "-";

                // попытка посчитать кадры если это гиф
                if (imageFormat.ToLower() == "gif")
                {
                    int frames = GetGifFrameCount(imagePath);
                    frameCount = frames.ToString();
                }
                formatLabel.Content = imageFormat;
                dimensionsLabel.Content = imageDimensions;
                sizeLabel.Content = fileSize;
                lengthLabel.Content = frameCount;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private int GetGifFrameCount(string gifPath)
        {
            try
            {
                // попытка прочитать гиф по байтам чтобы узнать количество кадров
                byte[] gifData = File.ReadAllBytes(gifPath);
                int frameCount = 0;

                // 0x2C - Image Descriptor
                for (int i = 1; i < gifData.Length - 1; i++)
                {
                    if (gifData[i] == 0x2C && gifData[i + 1] == 0x00)
                    {
                        frameCount++;
                    }
                }

                // если кадры пустые, вернуть 1 
                return frameCount > 0 ? frameCount : 1;
            }
            catch
            {
                return 1; // при любой ошибке вернуть 1
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Выберите изображение.");
                return;
            }

            if (string.IsNullOrEmpty(assetNameTextBox.Text))
            {
                MessageBox.Show("Введите имя элемента.");
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetFileName(selectedImagePath);
                string projectPath = GetProjectPath();
                string destinationDir = System.IO.Path.Combine(projectPath, "Source");
                string destinationPath = System.IO.Path.Combine(destinationDir, fileName);
                string imagePathForDb = "/Source/" + fileName;

                // проверка на дубликаты по имени или пути файла
                using (var db = new Class1())
                {
                    bool filenameExists = db.ResourcesSet.Any(r => r.ImagePath == imagePathForDb);
                    if (filenameExists)
                    {
                        MessageBox.Show($"Файл с именем '{fileName}' уже существует. Пожалуйста, измените название.");
                        return;
                    }

                    bool assetNameExists = db.ResourcesSet.Any(r => r.Title == assetNameTextBox.Text);
                    if (assetNameExists)
                    {
                        MessageBox.Show($"Элемент с названием '{assetNameTextBox.Text}' уже существует. Пожалуйста, измените название.");
                        return;
                    }
                }

                // создать директорию если нужной папки не существует
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                    MessageBox.Show($"Создан путь: {destinationDir}");
                }

                if (File.Exists(destinationPath))
                {
                    var result = MessageBox.Show($"Файл с именем '{fileName}' уже находится в директории. Перезаписать?",
                                                "Файл уже существует",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                // переносим файл в директорию
                try
                {
                    File.Copy(selectedImagePath, destinationPath, true);
                
                    //MessageBox.Show($"File copied to: {destinationPath}");
                }
                catch (Exception copyEx)
                {
                    MessageBox.Show($"Ошибка при загрузке: {copyEx.Message}");
                    return;
                }

                // проверка что файл успешно скопировался
                if (!File.Exists(destinationPath))
                {
                    MessageBox.Show("Ошибка - файла не загрузился.");
                    return;
                }

                // добавить в БД
                using (var db = new Class1())
                {
                    var currentUser = db.UserSet.FirstOrDefault(u => u.Id == App.CurrentUserId);

                    //MessageBox.Show($"Загрузка файлов от имени {currentUser.Username}, id {App.CurrentUserId}"); //debug

                    var resource = new Resources()
                    {
                        Title = assetNameTextBox.Text,
                        Author = currentUser, 
                        AuthorId = App.CurrentUserId,
                        ImagePath = imagePathForDb,
                        UploadDate = DateTime.Now
                    };

                    db.ResourcesSet.Add(resource);
                    db.SaveChanges();
                }

                MessageBox.Show($"Спасибо за новый элемент, {App.CurrentUsername}");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
            
        }

        private string GetProjectPath()
        {
            try
            {
                //перебор различных возможных путей в поиске места для папки для элементов
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string path1 = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, @"..\..\..\"));
                string path2 = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, @"..\..\"));
                string path3 = Directory.GetCurrentDirectory();
                if (Directory.Exists(path1))
                    return path1;
                else if (Directory.Exists(path2))
                    return path2;
                else
                    return path3;
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }
  
    }
}