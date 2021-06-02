using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.SubdivisionForms
{
    /// <summary>
    /// Interaction logic for PropertyWorks.xaml
    /// </summary>
    public partial class PropertyWorks : Window
    {

        public Dictionary<String, List<String>> LastProperties => this.SubdivisionProperties;

        Dictionary<String, List<String>> Properties = null;
        Dictionary<String, List<String>> SubdivisionProperties = null;

        public PropertyWorks(String Title, Dictionary<String, List<String>> Properties, Dictionary<String, List<String>> SubdivisionProperties)
        {
            InitializeComponent();
            this.Properties = Properties;
            this.SubdivisionProperties = SubdivisionProperties;
            this.NameBox.Text = Title;
            foreach(var Item in this.Properties)
            {
                this.TypeBox.Items.Add(Item.Key);
                this.DubTypeBox.Items.Add(Item.Key);
            }
            if(this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = 0;
        }

        private void LoadData()
        {
            this.PropertiesList.Items.Clear();
            this.PropertiesInSundivisionList.Items.Clear();
            if(this.Properties.TryGetValue(this.TypeBox.SelectedItem.ToString(), out List<string> Items))
                foreach(string Item in Items)
                    this.PropertiesList.Items.Add(Item);
            if(this.SubdivisionProperties.TryGetValue(this.TypeBox.SelectedItem.ToString(), out Items))
                foreach(string Item in Items)
                    this.PropertiesInSundivisionList.Items.Add(Item);
            if(this.PropertiesList.Items.Count > 0)
                this.PropertiesList.SelectedIndex = 0;
            if(this.PropertiesInSundivisionList.Items.Count > 0)
                this.PropertiesInSundivisionList.SelectedIndex = 0;
        }
        
        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        // добавить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Value = this.PropertiesList.SelectedItem?.ToString();
            if(Value != null)
            {
                var Item = this.PropertiesInSundivisionList.Items.Find(Value);
                if(Item == null)
                {
                    this.SubdivisionProperties[this.TypeBox.Text].Add(Value);
                    this.PropertiesInSundivisionList.Items.Add(Value);
                }
            }
        }
        // удалить
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int SelID = this.PropertiesInSundivisionList.SelectedIndex;
            if(SelID > -1)
            {
                this.SubdivisionProperties[this.TypeBox.Text].Remove(this.PropertiesInSundivisionList.SelectedItem.ToString());
                this.PropertiesInSundivisionList.Items.RemoveAt(SelID);
                this.PropertiesInSundivisionList.SelectedIndex =
                    (SelID < this.PropertiesInSundivisionList.Items.Count) ? SelID : SelID - 1;
            }
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DubTypeBox.SelectedIndex = this.TypeBox.SelectedIndex;
            this.LoadData();
        }
    }
}
