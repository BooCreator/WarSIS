using System;
using System.Collections.Generic;
using System.Windows;

namespace WarSIS.MainForms.SubdivisionForms
{
    /// <summary>
    /// Interaction logic for AddSubdivision.xaml
    /// </summary>
    public partial class AddSubdivision : Window
    {
        /// <summary>
        /// Название нового подразделения
        /// </summary>
        public String NewTitle { get => this.TextBox.Text; set => this.TextBox.Text = value; }
        /// <summary>
        /// Тип нового подразделения (Рота, Отделение, Баттальон и т.д.)
        /// </summary>
        public String Type => this.TypeBox.Text;

        /// <param name="Items">Список типов подразделения (Рота, Отделение, Баттальон и т.д.)</param>
        /// <param name="SelectedIndex">
        /// Индекс активного типа. Нужен для того, чтобы сразу выбирался тот же
        /// что и на главной форме
        /// </param>
        public AddSubdivision(List<String> Items, Int32 SelectedIndex = 0)
        {
            this.InitializeComponent();
            // заполняем список типов подраздлелений
            foreach(var Item in Items)
                this.TypeBox.Items.Add(Item);
            // если элементы есть, то выбираем первый
            if(this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = SelectedIndex;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбран ли тип
            if (this.TypeBox.SelectedIndex > -1)
            {
                // проверяем введено ли название
                if(this.TextBox.Text.Length > 0)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Введите имя!");
            }
            else
                Ext.MessageBox("Выберите тип подразделения!");
        }
    }
}
