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

namespace WarSIS.SharedForms
{
    /// <summary>
    /// Interaction logic for BuyTemplate.xaml
    /// </summary>
    public partial class BuyTemplate : Window
    {
        public Int32 Count => Int32.Parse(this.CountBox.Text);
        public BuyTemplate()
        {
            this.InitializeComponent();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
    }
}
