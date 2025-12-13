using micpix.Server;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

                // Set default asset name to filename without extension
                string fileName = System.IO.Path.GetFileNameWithoutExtension(selectedImagePath);
                assetNameTextBox.Text = fileName;
            }
        }

        private void LoadImageToCanvas(string imagePath)
        {
            try
            {
                // Create and configure image
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
                imageCanvas.Children.Add(vb);

                // Привязка размеров Viewbox к Canvas
                vb.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = imageCanvas });
                vb.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = imageCanvas });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private void UpdateImageInfo(string imagePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(imagePath);
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));

                // Get file format from extension
                imageFormat = System.IO.Path.GetExtension(imagePath).ToUpper().TrimStart('.');

                // Get dimensions
                imageDimensions = $"{bitmap.PixelWidth}x{bitmap.PixelHeight}";

                // Get file size
                fileSize = (fileInfo.Length / 1024f).ToString("0.0") + " KB";

                // Reset frame count
                frameCount = "-";

                // If it's a GIF, get frame count
                if (imageFormat.ToLower() == "gif")
                {
                    int frames = GetGifFrameCount(imagePath);
                    frameCount = frames.ToString();
                }

                // Update UI labels
                formatLabel.Content = imageFormat;
                dimensionsLabel.Content = imageDimensions;
                sizeLabel.Content = fileSize;
                lengthLabel.Content = frameCount; // Now showing just the frame count number
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading image info: {ex.Message}");
            }
        }

        private int GetGifFrameCount(string gifPath)
        {
            try
            {
                // Read GIF file as binary and count frame separators
                byte[] gifData = File.ReadAllBytes(gifPath);
                int frameCount = 0;

                // Look for GIF frame markers (0x2C - Image Descriptor)
                for (int i = 13; i < gifData.Length - 1; i++)
                {
                    if (gifData[i] == 0x2C && gifData[i + 1] == 0x00)
                    {
                        frameCount++;
                    }
                }

                // Return at least 1 frame
                return frameCount > 0 ? frameCount : 1;
            }
            catch
            {
                return 1; // Default to 1 frame if we can't read it
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Please select an image first.");
                return;
            }

            if (string.IsNullOrEmpty(assetNameTextBox.Text))
            {
                MessageBox.Show("Please enter an asset name.");
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetFileName(selectedImagePath);
                string projectPath = GetProjectPath();

                // Debug: Check if we're getting the right path
                //MessageBox.Show($"Project path: {projectPath}");

                string destinationDir = System.IO.Path.Combine(projectPath, "Source");
                string destinationPath = System.IO.Path.Combine(destinationDir, fileName);
                string imagePathForDb = "/Source/" + fileName;

                // Check if filename already exists in database
                using (var db = new Class1())
                {
                    bool filenameExists = db.ResourcesSet.Any(r => r.ImagePath == imagePathForDb);
                    if (filenameExists)
                    {
                        MessageBox.Show($"A file with the name '{fileName}' already exists in the database. Please rename the file or choose a different one.");
                        return;
                    }

                    bool assetNameExists = db.ResourcesSet.Any(r => r.Title == assetNameTextBox.Text);
                    if (assetNameExists)
                    {
                        MessageBox.Show($"An asset with the name '{assetNameTextBox.Text}' already exists. Please choose a different name.");
                        return;
                    }
                }

                // Create directory if it doesn't exist
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                    MessageBox.Show($"Created directory: {destinationDir}");
                }

                // Check if file already exists in destination
                if (File.Exists(destinationPath))
                {
                    var result = MessageBox.Show($"A file named '{fileName}' already exists in the Source folder. Do you want to overwrite it?",
                                                "File Exists",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                // Copy the file with error handling
                try
                {
                    File.Copy(selectedImagePath, destinationPath, true);
                
                    //MessageBox.Show($"File copied to: {destinationPath}");
                }
                catch (Exception copyEx)
                {
                    MessageBox.Show($"Error copying file: {copyEx.Message}");
                    return;
                }

                // Verify the file was copied
                if (!File.Exists(destinationPath))
                {
                    MessageBox.Show("File copy failed - destination file doesn't exist.");
                    return;
                }



                // Save to database
                using (var db = new Class1())
                {
                    var resource = new Resources()
                    {
                        Title = assetNameTextBox.Text,
                        Author = db.UserSet.First(), //placeholder
                        ImagePath = imagePathForDb,
                        UploadDate = DateTime.Now
                    };

                    db.ResourcesSet.Add(resource);
                    db.SaveChanges();
                }

                MessageBox.Show("Image uploaded successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading image: {ex.Message}");
            }
            
        }

        private string GetProjectPath()
        {
            try
            {
                // Try different possible project structures
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Option 1: For debug/bin folder structure
                string path1 = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, @"..\..\..\"));

                // Option 2: For different folder structure
                string path2 = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, @"..\..\"));

                // Option 3: Just use the current directory
                string path3 = Directory.GetCurrentDirectory();

                // Check which one exists and has a Source folder or can create one
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