using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms.PeopleForms
{
    /// <summary>
    /// Interaction logic for AddRank.xaml
    /// </summary>
    public partial class AddRank : Window
    {
        public String RankName => this.RankType.Text;
        public List<String> FieldValues => this.GetValues();
        /// <summary>
        /// Массив с названиями дополнительных полей
        /// </summary>
        private Dictionary<String, List<String>> Fields = null;

        /// <param name="Ranks">Список названий званий (Генерал, Рядовой и т.д.)</param>
        /// <param name="Fields">
        /// Список названий дополнительных полей где
        /// ключ - название типа, значение - список названий полей
        /// </param>
        public AddRank(String Name, List<String> Ranks, Dictionary<String, List<String>> Fields)
        {
            this.InitializeComponent();
            this.Fields = Fields;
            this.NameBox.Text = Name;
            // заполняем список званий
            foreach(String Item in Ranks)
                this.RankType.Items.Add(Item);
            this.RankType.Items.Remove("Все");
            // если элементы есть, то выбираем первый
            if(this.RankType.Items.Count > 0)
                this.RankType.SelectedIndex = 0;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбран ли тип
            if(this.RankType.SelectedIndex > -1)
            {
                this.DialogResult = true;
            } else
                Ext.MessageBox("Выберите звание!");
        }

        private void RankType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // очищаем список дополнительных полей
            this.Params.Children.Clear();
            // если удалось получить список полей
            if(this.Fields.TryGetValue(this.RankType.SelectedItem.ToString(), out List<string> Items))
            {
                // для каждого поля
                foreach(string Item in Items)
                {
                    // создаём элемент с двумя столбцами и однйо строкой
                    Grid NewItem = new Grid()
                    {
                        Height = 30
                    };
                    NewItem.RowDefinitions.Add(new RowDefinition());
                    NewItem.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(this.MainGrid.ColumnDefinitions[0].Width.Value, GridUnitType.Pixel) });
                    NewItem.ColumnDefinitions.Add(new ColumnDefinition());

                    // создаём текстовый блок с названием поля
                    NewItem.Children.Add(new TextBlock()
                    {
                        Text = Item,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 10, 0)
                    });
                    // создаём поле для ввода
                    NewItem.Children.Add(new TextBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 10, 0)
                    });
                    // добавляем всё в список
                    this.Params.Children.Add(NewItem);
                }
            }
        }

        private List<String> GetValues()
        {
            List<string> Result = new List<string>();
            // для каждого элемента в списке полей на форме
            foreach(Grid Item in this.Params.Children)
                // для каждого текстового поля
                foreach(TextBox TextBox in Item.Children)
                    // добавляем текст в результат
                    Result.Add(TextBox.Text);
            return Result;
        }

    }
}
