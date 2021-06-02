using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.PeopleForms
{
    /// <summary>
    /// Interaction logic for AddSubdivision.xaml
    /// </summary>
    public partial class AddSubdivision : Window
    {
        /// <summary>
        /// Тип подразделения (Рота, Отделение, Баттальон и т.д.)
        /// </summary>
        public String SType => this.SubdivisionType.Text;
        /// <summary>
        /// Название подразделения
        /// </summary>
        public String SName => this.SubdivisionName.Text;
        /// <summary>
        /// Назначить командиром
        /// </summary>
        public Boolean IsComander => this.ComanderBox.IsChecked == true;

        /// <summary>
        /// Список с ключами - типами подразделений и значениями - списком подразделений типа
        /// </summary>
        private Dictionary<String, List<String>> Items = null;

        /// <param name="Name">Имя человека</param>
        /// <param name="Items">
        /// Список ключей-значений, где ключ - тип подразделения, 
        /// значение - список подразделений данного типа
        /// </param>
        public AddSubdivision(String Name, Dictionary<String, List<String>> Items)
        {
            this.InitializeComponent();
            this.NameBox.Text = Name;
            this.Items = Items;
            // заполняем список типов подраздлелений
            foreach(var Item in Items)
                this.SubdivisionType.Items.Add(Item.Key);
            // если элементы есть, то выбираем первый
            if(this.SubdivisionType.Items.Count > 0)
                this.SubdivisionType.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбран ли тип
            if(this.SubdivisionType.SelectedIndex > -1)
            {
                // проверяем выбрано ли подразделение
                if(this.SubdivisionName.SelectedIndex > -1)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Выберите подраздленение!");
            } else
                Ext.MessageBox("Выберите тип подразделения!");
        }

        private void SubdivisionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // очищаем выпадающий список подразделений 
            this.SubdivisionName.Items.Clear();
            // если удалось из списка типов и значений получить список подразделений
            if(this.Items.TryGetValue(this.SubdivisionType.SelectedItem.ToString(), out List<string> val))
            {
                // заполняем выпадающий список подразлделений
                foreach(var Item in val)
                    this.SubdivisionName.Items.Add(Item);
                if(this.SubdivisionName.Items.Count > 0)
                    this.SubdivisionName.SelectedIndex = 0;
            }
        }
    
    }
}
