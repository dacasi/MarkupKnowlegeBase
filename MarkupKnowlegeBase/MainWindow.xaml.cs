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

        private KBRepository _repository;
        private KBConfig _config;
        private readonly TreeViewItemFactory _treeViewFactory = new TreeViewItemFactory();

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
                var settings = new MarkdownHtmlWriterSettings()
                {
                    Title = info.Name
                };
                using (var writer = new MarkdownHtmlWriter(settings))
                {
                    writer.AppendMarkdown(File.ReadAllText(info.FullName, Encoding.UTF8));
                    var tempFile = CreateTempFile(writer.GetHtml());
                    webBrowser.Navigate(tempFile);
                }
            }
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
                var node = _treeViewFactory.CreateFolder(dirInfo.Name, dirInfo, tvwEntries_Expand);
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

                return new KBConfig() { BaseDirectory = @"D:\temp\markdown" };
                }
            catch
            {
                return new KBConfig() { BaseDirectory = Environment.CurrentDirectory };
            }
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

    }
}
