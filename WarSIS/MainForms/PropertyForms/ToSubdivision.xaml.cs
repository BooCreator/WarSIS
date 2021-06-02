using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.PropertyForms
{
    /// <summary>
    /// Interaction logic for ToSubdivision.xaml
    /// </summary>
    public partial class ToSubdivision : Window
    {
        /// <summary>
        /// Тип подразделения (Рота, Отделение, Баттальон и т.д.)
        /// </summary>
        public String Type => this.SubdivisionType.Text;
        /// <summary>
        /// Название выбранного подразделения
        /// </summary>
        public String Subdivision => this.SubdivisionBox.Text;
        /// <summary>
        /// Список с ключами - типами подразделений и значениями - списком подразделений типа
        /// </summary>
        private Dictionary<String, List<String>> Items = null;

        /// <param name="Property">Название активного имущества</param>
        /// <param name="Items">
        /// Список ключей-значений, где ключ - тип подразделения, 
        /// значение - список подразделений данного типа
        /// </param>
        public ToSubdivision(String Property, Dictionary<String, List<String>> Items)
        {
            this.InitializeComponent();
            this.PropertyName.Text = Property;
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
                if(this.SubdivisionBox.SelectedIndex > -1)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Выберите подразделение!");
            } else
                Ext.MessageBox("Выберите тип подразделения!");
        }

        private void SubdivisionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // очищаем выпадающий список подразделений 
            this.SubdivisionBox.Items.Clear();
            // если удалось из списка типов и значений получить список подразделений
            if(this.Items.TryGetValue(this.SubdivisionType.SelectedItem.ToString(), out List<string> val))
            {
                // заполняем выпадающий список подразлделений
                foreach(var Item in val)
                    this.SubdivisionBox.Items.Add(Item);
                if(this.SubdivisionBox.Items.Count > 0)
                    this.SubdivisionBox.SelectedIndex = 0;
            }
        }
    
    }
}
