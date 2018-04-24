using KnowlegeBase.Core;
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
using Newtonsoft.Json;
using System.IO;
using Markdown.Core;

namespace Markdown.UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string _configFile = "Config.json";
        private static readonly string _welcomePage = @"Content\Welcome.markdown";

        private KBRepository _repository;
        private KBConfig _config;
        private readonly TreeViewItemFactory _treeViewFactory = new TreeViewItemFactory();
        private bool _isEditorVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => {
                _config = GetConfig();
                _repository = new KBRepository(_config.BaseDirectory);
            });
            ReloadTree();
            LoadWelcomePage();
        }

        private void LoadWelcomePage()
        {
            if (File.Exists(_welcomePage))
                LoadMarkdown(new FileInfo(_welcomePage));
        }

        private void ReloadTree()
        {
            tvwEntries.Items.Clear();
            var parentInfo = new DirectoryInfo(_config.BaseDirectory);
            foreach (var node in GetNodes(parentInfo)){
                tvwEntries.Items.Add(node);
            }
        }

        private void tvwEntries_Expand(TreeViewItem a_parentItem)
        {
            var parentInfo = (DirectoryInfo)a_parentItem.Tag;
            a_parentItem.Items.Clear();
            foreach (var node in GetNodes(parentInfo))
            {
                a_parentItem.Items.Add(node);
            }
        }

        private void tvwEntries_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)e.NewValue;
            if(item.Tag is FileInfo)
            {
                var info = (FileInfo)item.Tag;
                LoadMarkdown(info);
            }
        }

        private void LoadMarkdown(FileInfo  a_info)
        {
            var settings = new MarkdownHtmlWriterSettings()
            {
                Title = a_info.Name
            };
            using (var writer = new MarkdownHtmlWriter(settings))
            {
                var text = File.ReadAllText(a_info.FullName, Encoding.UTF8);
                txtEditor.Text = text;
                writer.AppendMarkdown(text);
                var tempFile = CreateTempFile(writer.GetHtml());
                webBrowser.Navigate(tempFile);
            }
        }

        private FileInfo GetSelectedFileInfo()
        {
            var obj = tvwEntries.SelectedItem;
            if (!(obj is TreeViewItem)) return null;
            var item = (TreeViewItem)obj;
            if (item.Tag == null || !(item.Tag is FileInfo)) return null;
            return (FileInfo)item.Tag;
        }

        private string CreateTempFile(string a_html)
        {
            var p = System.IO.Path.Combine(Environment.CurrentDirectory, "temp.html");
            File.WriteAllText(p, a_html, Encoding.UTF8);
            return p;
        }

        private List<TreeViewItem> GetNodes(DirectoryInfo a_parent)
        {
            if (!Directory.Exists(a_parent.FullName)) return  new List<TreeViewItem>();

            var nodes = new List<TreeViewItem>();
            foreach (var dir in Directory.GetDirectories(a_parent.FullName))
            {
                var dirInfo = new DirectoryInfo(dir);
                var node = _treeViewFactory.CreateFolder(dirInfo.Name, dirInfo, tvwEntries_Expand, dirInfo.GetDirectories().Any());
                nodes.Add(node);
            }
            foreach (var file in Directory.GetFiles(a_parent.FullName, "*.markdown"))
            {
                var fileInfo = new FileInfo(file);
                var node = _treeViewFactory.CreateMarkdownFile(fileInfo.Name, fileInfo, tvwEntries_Expand);
                nodes.Add(node);
            }
            return nodes;
        }

        private KBConfig GetConfig()
        {
            try
            {
                if (File.Exists(_configFile))
                    return JsonHelper.DeserializeFromFile<KBConfig>(_configFile);

                return new KBConfig() { BaseDirectory = GetDefaultDirectory() };
                }
            catch
            {
                return new KBConfig() { BaseDirectory = GetDefaultDirectory() };
            }
        }

        private string GetDefaultDirectory()
        {
            var dir = System.IO.Path.Combine(Environment.CurrentDirectory, "Content");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_config != null)
                    JsonHelper.SerializeToFile(_configFile, _config);
            }
            catch { }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ToogleEditor();
        }

        private void ToogleEditor()
        {
            _isEditorVisible = !_isEditorVisible;
            if (_isEditorVisible)
                grdBrowserEditor.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            else
                grdBrowserEditor.RowDefinitions[0].Height = new GridLength(0);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = GetSelectedFileInfo();
                if (info == null) return;
                File.WriteAllText(info.FullName, txtEditor.Text, Encoding.UTF8);
            }
            catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var info = GetSelectedFileInfo();
            if (info != null)
                LoadMarkdown(info);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            var newName = txtNew.Text + ".markdown";
            var directory = GetCurrentDirectory();
            if (Directory.GetFiles(directory).Select(f => new FileInfo(f)).Any(f => f.Name.Equals(newName, StringComparison.InvariantCultureIgnoreCase)))
            {
                ShowError(new ArgumentException("File already exists."));
                return;
            }
            var newFile = System.IO.Path.Combine(directory, newName);
            File.WriteAllText(newFile, "# New Markup File", Encoding.UTF8);

            var directoryNode = GetCurrentDirectoryNode();
            var node = _treeViewFactory.CreateMarkdownFile(txtNew.Text, new FileInfo(newFile), tvwEntries_Expand);
            if (directoryNode != null)
                directoryNode.Items.Add(node);
            else
                tvwEntries.Items.Add(node);
        }

        private TreeViewItem GetCurrentDirectoryNode()
        {
            if (tvwEntries.SelectedItem == null) return null;

            var node = tvwEntries.SelectedItem as TreeViewItem;
            if (node.Tag == null)
                return null;
            else if (node.Tag is FileInfo)
                return node.Parent as TreeViewItem;
            else if (node.Tag is DirectoryInfo)
                return node;
            return null;
        }

        private string GetCurrentDirectory()
        {
            //Get current directory from tree
            var currentDirectory = string.Empty;
            var directoryNode = GetCurrentDirectoryNode();
            if (directoryNode != null)
                currentDirectory = ((DirectoryInfo)directoryNode.Tag).FullName;

            return string.IsNullOrWhiteSpace(currentDirectory) ? _config.BaseDirectory : currentDirectory;
        }
    }
}
