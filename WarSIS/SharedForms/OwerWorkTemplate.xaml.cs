using System;
using System.Windows;

namespace WarSIS.SharedForms
{
    /// <summary>
    /// Interaction logic for OwerWorkTemplate.xaml
    /// </summary>
    public partial class OwerWorkTemplate : Window
    {
        public DateTime? StartDate => this.StartDateBox.SelectedDate;
        public DateTime? EndDate => this.EndDateBox.SelectedDate;
        public OwerWorkTemplate()
        {
            this.InitializeComponent();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
