using System;
using System.Collections.Generic;
using System.Windows;

namespace WarSIS.MainForms.PeopleForms
{
    /// <summary>
    /// Interaction logic for AddSpecialties.xaml
    /// </summary>
    public partial class AddSpecialties : Window
    {
        public List<String> LastSpecialties 
            => this.PeopleSpecialtiesList.Items.ToStringList();

        public AddSpecialties(String Name, List<String> Specialties, List<String> PeopleSpecialties)
        {
            InitializeComponent();
            this.NameBox.Text = Name;
            foreach(var Item in Specialties)
                this.SpecialtiesList.Items.Add(Item);
            foreach(var Item in PeopleSpecialties)
                this.PeopleSpecialtiesList.Items.Add(Item);
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        // добавить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Value = this.SpecialtiesList.SelectedItem?.ToString();
            if(Value != null)
            {
                var Item = this.PeopleSpecialtiesList.Items.Find(Value);
                if(Item == null)
                    this.PeopleSpecialtiesList.Items.Add(Value);
            }
        }
        // удалить
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int SelID = this.PeopleSpecialtiesList.SelectedIndex;
            if(SelID > -1)
            {
                this.PeopleSpecialtiesList.Items.RemoveAt(SelID);
                this.PeopleSpecialtiesList.SelectedIndex = 
                    (SelID < this.PeopleSpecialtiesList.Items.Count) ? SelID : SelID - 1;
            }
        }
    }
}
