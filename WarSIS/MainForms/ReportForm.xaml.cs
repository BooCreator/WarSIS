using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for ReportForm.xaml
    /// </summary>
    public partial class ReportForm : Window
    {
        private TextBlock Text = new TextBlock() { Padding = new Thickness(10, 10, 16, 16) };
        private Dictionary<String, Dictionary<String, String>> Data = null;
        private ComboBox Selector = new ComboBox() { Margin = new Thickness(10, 0, 10, 10) };
        private Dictionary<String, String> SimpleData = null;
        public ReportForm(String Text)
        {
            this.InitializeComponent();
            this.Text.Text = Text;
        }
        public ReportForm(String Text, Dictionary<String, String> Data):this(Text)
        {
            this.SimpleData = Data;
            this.LoadAll();
        }

        public ReportForm(String Text, Dictionary<String, Dictionary<String, String>> Data) : this(Text)
        {
            this.Data = Data;
            foreach(var Item in Data)
                this.Selector.Items.Add(Item.Key);
            if(this.Selector.Items.Count > 0)
                this.Selector.SelectedIndex = 0;
            this.Selector.SelectionChanged += this.LoadAll;
            this.LoadAll();
        }
        
        private void LoadAll(object sender = null, SelectionChangedEventArgs e = null)
        {
            Grid Grid = this.ReportData;
            Grid.Children.Clear();
            Grid.Children.Add(this.Text);
            Grid.SetColumn(this.Text, 0);
            Grid.SetRow(this.Text, 0);
            Grid.SetColumnSpan(this.Text, 2);
            if(this.Data != null && this.Selector.SelectedIndex > -1)
            {
                Grid.Children.Add(this.Selector);
                Grid.SetColumn(this.Selector, 0);
                Grid.SetRow(this.Selector, 1);
                Grid.SetColumnSpan(this.Selector, 2);
                if(this.Data.TryGetValue(this.Selector.SelectedItem.ToString(), out Dictionary<String, String> value))
                    this.LoadData(value);
            } else
                this.LoadData(this.SimpleData);
        }
        private void LoadData(Dictionary<String, String> Data)
        {
            Grid Grid = this.ReportData;
            int row = 2;
            foreach(KeyValuePair<string, string> item in Data)
            {
                Grid.RowDefinitions.Add(new RowDefinition());
                var Item1 = new TextBlock()
                {
                    Text = $"{item.Key}:",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25,
                };
                var Item2 = new TextBlock()
                {
                    Text = item.Value,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25,
                };

                Grid.Children.Add(Item1);
                Grid.Children.Add(Item2);

                Grid.SetColumn(Item1, 0);
                Grid.SetRow(Item1, row);
                Grid.SetColumn(Item2, 1);
                Grid.SetRow(Item2, row);

                row++;
            }
        }
    
    }
}
