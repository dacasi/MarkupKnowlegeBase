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

namespace Markdown.UI
{
    /// <summary>
    /// Interaktionslogik für FrmName.xaml
    /// </summary>
    public partial class FrmName : Window
    {
        private Func<string, bool> _action;

        public FrmName(Func<string, bool> a_action)
        {
            InitializeComponent();
            _action = a_action;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_action(txtName.Text))
                this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
