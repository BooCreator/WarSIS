using System;
using System.Collections.Generic;
using System.Windows;

namespace WarSIS.MainForms.SubdivisionForms
{
    /// <summary>
    /// Interaction logic for SetUpper.xaml
    /// </summary>
    public partial class SetUpper : Window
    {
        /// <summary>
        /// Тип нового подразделения (Рота, Отделение, Баттальон и т.д.)
        /// </summary>
        public String Type => this.TypeBox.Text;
        /// <summary>
        /// Название вышестоящего подразделения
        /// </summary>
        public String Upper => this.UpperBox.Text;
        /// <summary>
        /// Список с ключами - типами подразделений и значениями - списком подразделений типа
        /// </summary>
        private Dictionary<String, List<String>> Items = null;

        /// <param name="Subdivision">Название активного подразделения</param>
        /// <param name="Items">
        /// Список ключей-значений, где ключ - тип подразделения, 
        /// значение - список подразделений данного типа
        /// </param>
        public SetUpper(String Subdivision, Dictionary<String, List<String>> Items)
        {
            this.InitializeComponent();
            this.Subdivision_name.Text = Subdivision;
            this.Items = Items;
            // заполняем список типов подраздлелений
            foreach (var Item in Items)
                this.TypeBox.Items.Add(Item.Key);
            // если элементы есть, то выбираем первый
            if (this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // првоеряем выбран ли тип
            if (this.TypeBox.SelectedIndex > -1)
            {
                // проверяем выбрано ли подразделение
                if (this.UpperBox.SelectedIndex > -1)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Выберите вышестоящую!");
            }
            else
                Ext.MessageBox("Выберите тип подразделения!");
        }

        private void TypeBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // очищаем выпадающий список подразделений 
            this.UpperBox.Items.Clear();
            // если удалось из списка типов и значений получить список подразделений
            if(this.Items.TryGetValue(this.TypeBox.SelectedItem.ToString(), out List<string> val))
            {
                // заполняем выпадающий список подразлделений
                foreach (var Item in val)
                    this.UpperBox.Items.Add(Item);
                if (this.UpperBox.Items.Count > 0)
                    this.UpperBox.SelectedIndex = 0;
            }
        }
    }
}
