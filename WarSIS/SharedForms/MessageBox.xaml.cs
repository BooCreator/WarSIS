using System;
using System.Windows;

namespace WarSIS.SharedForms
{

    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox(String Message, String Title = "", MessageBoxButton Buttons = MessageBoxButton.OK)
        {
            InitializeComponent();
            this.Title = Title;
            this.TextBlock1.Text = Message;
            switch (Buttons)
            {
                case MessageBoxButton.OK:
                    this.ButtonNo.IsEnabled = false;
                    this.ButtonNo.Visibility = Visibility.Hidden;
                    this.ButtonsGrid.ColumnDefinitions.RemoveAt(1);
                    this.ButtonYes.Width = 150;
                    break;
                case MessageBoxButton.YesNo:
                    this.ButtonYes.Content = "Да";
                    this.ButtonNo.Content = "Нет";
                    break;
            }
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.ButtonYes.Focus();
        }
    }
}
