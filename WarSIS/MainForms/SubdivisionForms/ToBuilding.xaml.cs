using System;
using System.Collections.Generic;
using System.Windows;

namespace WarSIS.MainForms.SubdivisionForms
{
    /// <summary>
    /// Interaction logic for ToBuilding.xaml
    /// </summary>
    public partial class ToBuilding : Window
    {
        public String Building 
            => this.BuildingsBox.SelectedItem?.ToString();
        public ToBuilding(String Title, List<String> Buildings)
        {
            this.InitializeComponent();
            this.TitleBox.Text = Title;
            foreach(var Building in Buildings)
                this.BuildingsBox.Items.Add(Building);
            if(this.BuildingsBox.Items.Count > 0)
                this.BuildingsBox.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            if(this.BuildingsBox.SelectedIndex > -1)
            {
                this.DialogResult = true;
            } else
                Ext.MessageBox("Выберите здание!");
        }
    }
}
