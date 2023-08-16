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

namespace FeedReader
{
    /// <summary>
    /// Логика взаимодействия для WindowAddSource.xaml
    /// </summary>
    public partial class WindowAddSource : Window
    {
        private MainWindow parent;
        public WindowAddSource(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void buttonAddClick(object sender, RoutedEventArgs e)
        {
            string source = textboxSource.Text;
            if (source != "")
            {
                parent.AddFeedSource(source);
                Close();
            }
        }
    }
}
