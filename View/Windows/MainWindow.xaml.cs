using micpix.Server;
using micpix.View.UserControls;
using micpix.View.Windows;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ResourceUploadWindow uploadWindow;
        public CollageMakerWindow collageWindow;
        private LoginWindow loginWindow;
        static AppDbContext db = new AppDbContext ();
        int categoryId;
        ILookup<int?, Categories> lookup;
        string resquerytext;
        IEnumerable<Resources> resset = db.ResourcesSet.Include(r => r.Author);
        IEnumerable<Resources> resset_filtered = db.ResourcesSet.Include(r => r.Author);
        IEnumerable<ResultGIFs> collageset = db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author).GroupBy(g => g.CollageId).Select(g => g.OrderByDescending(x => x.CreatedAt).First());
        IEnumerable<ResultGIFs> collageset_filtered = db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author).GroupBy(g => g.CollageId).Select(g => g.OrderByDescending(x => x.CreatedAt).First());
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                using var db = new AppDbContext();
                //ObservableCollection<Resources> resset = new ObservableCollection<Resources>(db.ResourcesSet.Include(r => r.Author));
                //ObservableCollection<Resources> resset_filtered = new ObservableCollection<Resources>(db.ResourcesSet.Include(r => r.Author));
                //ObservableCollection<ResultGIFs> collageset = new ObservableCollection<ResultGIFs>(db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author));
                //ObservableCollection<ResultGIFs> collageset_filtered = new ObservableCollection<ResultGIFs>(db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }

            pageheader.LoginAction = () =>
            {
                if (App.IsLoggedIn)
                {
                    MessageBox.Show($"Здесь обязательно будет страница профиля для {App.CurrentUsername}", "Когда нибудь попозже");

                }
                else
                {
                    if (loginWindow == null || !loginWindow.IsLoaded)
                    {
                        loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        loginWindow.Activate();
                        this.Hide();
                    }
                }
            };
            LoadResources();
            LoadCollages();
            PopulateCategoryTree(cattree);
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
                        author = item.Author.Username, 
                        uploaddate = item.UploadDate.ToString("dd MMM") 
                    };

                    assetspanel.Children.Add(userControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void LoadCollages()
        {
            try
            {

                collagepanel.Children.Clear();

                foreach (var item in collageset_filtered)
                {
                    var userControl = new MainpageElement()
                    {
                        Margin = new Thickness(15),
                        imgsrc = item.FilePath,
                        title = item.Collage.Title,
                        author = item.Collage.Author.Username,
                        uploaddate = item.CreatedAt.ToString("dd MMM"),
                        itemid = item.CollageId
                    };
                    userControl.MouseLeftButtonUp += OpenCollage;
                    userControl.Tag = item.Collage;

                    collagepanel.Children.Add(userControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void OpenCollage(object sender, MouseButtonEventArgs e)
        {
            if (App.IsLoggedIn)
            {
                if (sender is MainpageElement control)
                {
                    MainWindow current = this;
                    int collageId = (int)control.itemid;
                    if (collageWindow == null || !collageWindow.IsLoaded)
                    {
                        collageWindow = new CollageMakerWindow();
                        collageWindow.currentCollage = db.Collages.Include(с => с.Layers).ThenInclude(l => l.Resource).FirstOrDefault(c => c.Id == collageId);
                        collageWindow.Show();
                    }
                    else
                    {
                        collageWindow.currentCollage = db.Collages.Include(с => с.Layers).ThenInclude(l => l.Resource).FirstOrDefault(c => c.Id == collageId);
                        collageWindow.Activate();
                    }
                    collageWindow.LoadCollageLayers();
                    this.Hide();
                }
            }
            else
            {
                MessageBox.Show($"Для изменения коллажей необходимо войти в аккаунт", "Вы не авторизованы");
                if (loginWindow == null || !loginWindow.IsLoaded)
                {
                    loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Hide();
                }
                else
                {
                    loginWindow.Activate();
                    this.Hide();
                }
            }
        }

        private void Uploader_Window_Click(object sender, RoutedEventArgs e)
        {

            if (App.IsLoggedIn)
            {
                if (uploadWindow == null || !uploadWindow.IsLoaded)
                {
                    uploadWindow = new ResourceUploadWindow();
                    uploadWindow.Show();
                }
                else
                {
                    uploadWindow.Activate();
                }
            }
            else
            {
                MessageBox.Show($"Для загрузки файлов необходимо войти в аккаунт", "Вы не авторизованы");
                if (loginWindow == null || !loginWindow.IsLoaded)
                {
                    loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Hide();
                }
                else
                {
                    loginWindow.Activate();
                    this.Hide();
                }
            }
        }

        private void CollageMaker_Window_Click(object sender, RoutedEventArgs e)
        {
            if (App.IsLoggedIn)
            {
                MainWindow current = this;
                using (var db = new AppDbContext())
                {
                    var currentUser = db.UserSet.FirstOrDefault(u => u.Id == App.CurrentUserId);
                    var collage = new Collages()
                    {
                        AuthorId = App.CurrentUserId,
                        Author = currentUser,
                        Title = "NewCollage",
                        CreatedAt = DateTime.Now,
                        Height = 500,
                        Width = 500,
                        UpdatedAt = DateTime.Now
                    };
                    db.Collages.Add(collage);
                    db.SaveChanges();

                    if (collageWindow == null || !collageWindow.IsLoaded)
                    {
                        collageWindow = new CollageMakerWindow();
                        collageWindow.currentCollage = collage;
                        collageWindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        collageWindow.currentCollage = collage;
                        collageWindow.Activate();
                        this.Hide();
                    }
                }
                collageWindow.mainwindow = this;
                collageWindow.LoadCollageLayers();
            }
            else
            {
                MessageBox.Show($"Для создания новых коллажей необходимо войти в аккаунт", "Вы не авторизованы");
                if (loginWindow == null || !loginWindow.IsLoaded)
                {
                    loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Hide();
                }
                else
                {
                    loginWindow.Activate();
                    this.Hide();
                }
            }
        }

        private void SearchAssets(object sender = null, TextChangedEventArgs e = null)
        {
            resquerytext = restextbox.Text;
            FilterResources();
        }

        private void FilterResources()
        {
            resset = db.ResourcesSet.Include(r => r.Author).Include(r => r.Tags).ThenInclude(t => t.Category);
            resset_filtered = resset;
            if (resquerytext != null && resquerytext.Trim() != null)
            {
                resset_filtered = resset.Where(resource =>

                    resource.Title.Contains(resquerytext, StringComparison.OrdinalIgnoreCase)
                );
            }
            if (categoryId != null && categoryId >1)
            {
                List<int> categoryIds = new List<int> { categoryId };
                BuildDescendantIds(categoryId, lookup, categoryIds);

                resset_filtered = resset_filtered.Where(r => r.Tags.Any(rt => categoryIds.Contains(rt.CategoryId)));
            }
            LoadResources();
        }

        private void SearchCollages(object sender, TextChangedEventArgs e)
        {
            string querytext = collagetb.Text;

            if (querytext.Trim() != null)
            {
                collageset = db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author).GroupBy(g => g.CollageId).Select(g => g.OrderByDescending(x => x.CreatedAt).First());
                collageset_filtered = collageset.Where(c => c.Collage.Title.Contains(querytext, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                collageset = db.ResultGIFs.Include(p => p.Collage).ThenInclude(c => c.Author).GroupBy(g => g.CollageId).Select(g => g.OrderByDescending(x => x.CreatedAt).First());
                collageset_filtered = collageset;
            }
            LoadCollages();
        }

        /*private void SortByName(object sender, RoutedEventArgs e)
        {
            if (resnamesort.IsChecked == true)
            {
                resset_filtered = resset_filtered.OrderBy(r => r.Title);
                LoadResources();
            }
        }

        private void SortByDate(object sender, RoutedEventArgs e)
        {
            if (resnamesort.IsChecked == true)
            {
                resset_filtered = resset_filtered.OrderBy(r => r.Title);
                LoadResources();
            }
        }*/

        public void PopulateCategoryTree(MenuItem cattree)
        {
            categoryTree.Items.Clear();

            var allCategories = db.Categories.ToList();
            lookup = allCategories.ToLookup(c => c.ParentId);

            foreach (var rootCategory in lookup[null])
            {
                var node = new TreeViewItem
                {
                    Header = rootCategory.Name,
                    Tag = rootCategory.Id
                };
                categoryTree.Items.Add(node);
                AddChildNodesRecursive(node, lookup, rootCategory.Id);
            }

            categoryTree.SelectedItemChanged += CategoryTree_SelectedItemChanged;
        }

        private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (categoryTree.SelectedItem is TreeViewItem selectedNode)
            {
                cattree.Header = $"{selectedNode.Header}";
                categoryId = (int)selectedNode.Tag;
                FilterResources();
            }
        }

        private void BuildDescendantIds(int parentId, ILookup<int?, Categories> lookup, List<int> result)
        {
            foreach (var child in lookup[parentId])
            {
                result.Add(child.Id);
                BuildDescendantIds(child.Id, lookup, result);
            }
        }

        private void AddChildNodesRecursive(TreeViewItem parentNode, ILookup<int?, Categories> lookup, int parentId)
        {
            foreach (var child in lookup[parentId])
            {
                TreeViewItem childNode = CreateTreeNode(child);
                parentNode.Items.Add(childNode);
                AddChildNodesRecursive(childNode, lookup, child.Id);
            }
        }
        private TreeViewItem CreateTreeNode(Categories category)
        {
            TreeViewItem node = new TreeViewItem
            {
                FontSize = 14,
                Header = category.Name,
                Tag = category.Id,
            };
            //node.Click += MenuItem_Click;
            return node;
        }

    }
}