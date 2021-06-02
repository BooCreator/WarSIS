using System;
using System.Windows;

namespace WarSIS.MainForms.BuildingForms
{
    /// <summary>
    /// Interaction logic for AddBuilding.xaml
    /// </summary>
    public partial class AddBuilding : Window
    {
        public String NewTitle => this.TitleBox.Text;
        public String Address => this.AddressBox.Text;

        public AddBuilding()
        {
            this.InitializeComponent();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            if(this.TitleBox.Text.Length > 0)
            {
                if(this.AddressBox.Text.Length > 0)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Введите адрес!");
            } else
                Ext.MessageBox("Введите название!");
        }
    }
}
