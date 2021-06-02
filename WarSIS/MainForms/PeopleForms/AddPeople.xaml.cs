using System;
using System.Windows;

namespace WarSIS.MainForms.PeopleForms
{
    /// <summary>
    /// Interaction logic for AddPeople.xaml
    /// </summary>
    public partial class AddPeople : Window
    {
        public String PeopleName { 
            get => this.TextBox.Text;
            set => this.TextBox.Text = value; 
        }

        public AddPeople()
        {
            this.InitializeComponent();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            if(this.TextBox.Text.Length > 0)
                this.DialogResult = true;
            else
                Ext.MessageBox("Введите имя!");
        }

    }
}
