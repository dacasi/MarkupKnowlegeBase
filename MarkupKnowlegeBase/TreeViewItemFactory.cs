using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Markdown.UI
{
    public class TreeViewItemFactory
    {
        private const string _folderImage = @"Resources/folder_48x48.png";
        private const string _markdownImage = @"Resources/filenew.png";

        public TreeViewItem CreateFolder(string a_text, object a_tag, Action<TreeViewItem> a_expandAction)
        {
            return CreateMarkdownFile(a_text, _folderImage, a_tag, a_expandAction, true);
        }

        public TreeViewItem CreateMarkdownFile(string a_text, object a_tag, Action<TreeViewItem> a_expandAction)
        {
            return CreateMarkdownFile(a_text, _markdownImage, a_tag, a_expandAction, false);
        }

        private TreeViewItem CreateMarkdownFile(string a_text, string a_image, object a_tag, Action<TreeViewItem> a_expandAction, bool a_canExpand)
        {
            var stack = new StackPanel() { Orientation = Orientation.Horizontal };
            var image = new Image();
            image.Source = new BitmapImage(new Uri(a_image, UriKind.Relative));
            image.Width = 16;
            image.Height = 16;
            var label = new Label() { Content = a_text };

            stack.Children.Add(image);
            stack.Children.Add(label);

            var item = new TreeViewItem()
            {
                IsExpanded = false,
                Header = stack,
                Tag = a_tag
            };
            item.Expanded += (s, e) => { a_expandAction(item); };
            if(a_canExpand)
                item.Items.Add(new TreeViewItem());

            return item;
        }
    }
}
