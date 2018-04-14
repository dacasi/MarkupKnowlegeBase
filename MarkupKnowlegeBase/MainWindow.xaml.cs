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

namespace Markdown.UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KBRepository _repository;
        private KBConfig _config;

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
        }

        private KBConfig GetConfig()
        {
            try
            {
                if (File.Exists("Config.json"))
                {
                    return JsonConvert.DeserializeObject<KBConfig>("Config.json");
                }
                return new KBConfig() { BaseDirectory = Environment.CurrentDirectory };
            }
            catch
            {
                return new KBConfig() { BaseDirectory = Environment.CurrentDirectory };
            }
        }
    }
}
