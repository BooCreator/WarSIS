using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.SubdivisionForms
{
    /// <summary>
    /// Interaction logic for SetComander.xaml
    /// </summary>
    public partial class SetComander : Window
    {

        public String RankName => this.RankBox.Text;
        public String PeopleName => this.PeopleBox.Text;
        /// <summary>
        /// Массив с названиями дополнительных полей
        /// </summary>
        private Dictionary<String, List<String>> Items = null;

        /// <param name="Ranks">Список названий званий (Генерал, Рядовой и т.д.)</param>
        /// <param name="Items">
        /// Список названий людей где
        /// ключ - название звания, значение - список людей
        /// </param>
        public SetComander(String Subdivision, Dictionary<String, List<String>> Items)
        {
            InitializeComponent();
            this.SundivisionName.Text = Subdivision;
            this.Items = Items;
            // заполняем список званий
            foreach(KeyValuePair<string, List<string>> Item in Items)
                this.RankBox.Items.Add(Item.Key);
            // если элементы есть, то выбираем первый
            if(this.RankBox.Items.Count > 0)
                this.RankBox.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбран ли тип
            if(this.RankBox.SelectedIndex > -1)
            {
                if(this.PeopleBox.SelectedIndex > -1)
                {
                    this.DialogResult = true;
                } else
                    Ext.MessageBox("Выберите человека!");
            } else
                Ext.MessageBox("Выберите звание!");
        }

        private void RankBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PeopleBox.Items.Clear();
            // если удалось получить список полей
            if(this.Items.TryGetValue(this.RankBox.SelectedItem.ToString(), out List<string> Items))
            {
                // для каждого поля
                foreach(string Item in Items)
                    this.PeopleBox.Items.Add(Item);
                // если элементы есть, то выбираем первый
                if(this.PeopleBox.Items.Count > 0)
                    this.PeopleBox.SelectedIndex = 0;
            }
        }

    }
}
