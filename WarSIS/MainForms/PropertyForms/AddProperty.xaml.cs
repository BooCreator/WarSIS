using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.PropertyForms
{
    /// <summary>
    /// Interaction logic for AddProperty.xaml
    /// </summary>
    public partial class AddProperty : Window
    {
        /// <summary>
        /// Тип нового имущества
        /// </summary>
        public String TypeName => this.TypeBox.Text;
        /// <summary>
        /// Навзвание нового имущества
        /// </summary>
        public String NewTitle => this.TitleBox.Text;
        public Int32 Inventory => Int32.Parse(this.InventoryBox.Text);
        /// <summary>
        /// Значение дополнительных полей
        /// </summary>
        public List<String> FieldValues => this.GetValues();
        /// <summary>
        /// Массив с названиями дополнительных полей
        /// </summary>
        private Dictionary<String, List<String>> Fields = null;

        /// <param name="Types">Список названий типов имущества (Автоматы, БМП и т.д.)</param>
        /// <param name="Fields">
        /// Список названий дополнительных полей где
        /// ключ - название типа, значение - список названий полей
        /// </param>
        public AddProperty(List<String> Types, Dictionary<String, List<String>> Fields)
        {
            this.InitializeComponent();
            this.Fields = Fields;
            // заполняем список типов имущества
            foreach(string Item in Types)
                this.TypeBox.Items.Add(Item);
            // если элементы есть, то выбираем первый
            if(this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбран ли тип
            if(this.TypeBox.SelectedIndex > -1)
            {
                // проверяем введено ли название
                if(this.TitleBox.Text.Length > 0)
                    this.DialogResult = true;
                else
                    Ext.MessageBox("Введите название!");
            } else
                Ext.MessageBox("Выберите тип имущества!");
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // очищаем список дополнительных полей
            this.Params.Children.Clear();
            // если удалось получить список полей
            if(this.Fields.TryGetValue(this.TypeBox.SelectedItem.ToString(), out List<string> Items))
            {
                this.SetFields(Items);
            }
        }
    
        private List<String> GetValues()
        {
            List<string> Result = new List<string>();
            // для каждого элемента в списке полей на форме
            foreach(Grid Item in this.Params.Children)
                // для каждого текстового поля
                foreach(Object Child in Item.Children)
                    if(Child is TextBox textBox)
                    // добавляем текст в результат
                    Result.Add(textBox.Text);
            return Result;
        }

        private void TitleBox_Copy_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if(!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }

        private void SetFields(List<String> Items)
        {
            this.Params.Children.Clear();
            foreach(string Item in Items)
            {
                Grid Grid = new Grid()
                {
                    Height = 30
                };
                Grid.RowDefinitions.Add(new RowDefinition());
                Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = this.MainGrid.ColumnDefinitions[0].Width });
                Grid.ColumnDefinitions.Add(new ColumnDefinition());

                Grid.Children.Add(new TextBlock()
                {
                    Text = $"{Item}:",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25
                });
                var Item2 = new TextBox()
                {
                    Name = Item.Replace(" ", ""),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25,
                };
                Grid.Children.Add(Item2);
                Grid.SetColumn(Item2, 1);
                this.Params.Children.Add(Grid);
            }
        }

    }
}
