using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WarSISDataBase.DataBase;

using WordApp = Microsoft.Office.Interop.Word.Application;
using WordDoc = Microsoft.Office.Interop.Word.Document;


namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for RaspisForm.xaml
    /// </summary>
    public partial class RaspisForm : Window
    {
        MSSQLEngine DB = null;

        List<Grid> Grids = new List<Grid>();

        WordApp Word = null;

        public RaspisForm(MSSQLEngine DB)
        {
            this.InitializeComponent();
            this.DB = DB;
            this.Grids = new List<Grid>()
            {
                this.Monday,
                this.Tuesday,
                this.Wednesday,
                this.Thursday,
                this.Friday,
                this.Saturday,
                this.Sunday
            };
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem owner = (MenuItem)sender;
            Popup child = (Popup)owner.Template.FindName("PART_Popup", owner);
            child.Placement = PlacementMode.Left;
            child.HorizontalOffset = -owner.ActualWidth;
            child.VerticalOffset = owner.ActualHeight;
        }

        #region My methods

        private void AddLine(Grid Grid, String LineText = "", String DateText = "")
        {
            TextBox Time = new TextBox()
            {
                Text = LineText,
                Name = Grid.Name + "_Line_" + Grid.Children.Count.ToString(),
                MaxLength = 4,
                MaxLines = 1,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(4, 0, 4, 0)
            };
            Time.PreviewTextInput += this.TextBox_PreviewTextInput;
            Time.LostFocus += this.TextBox_LostFocus;

            TextBox Data = new TextBox()
            {
                Text = DateText,
                Name = Grid.Name + "_Data_" + Grid.Children.Count.ToString(),
                MaxLines = 1,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(4, 0, 4, 0)
            };
            var NewRow = new RowDefinition() { Height = new GridLength(30, GridUnitType.Pixel)};

            Grid.RowDefinitions.Add(NewRow);
            Grid.Children.Add(Time);
            Grid.Children.Add(Data);

            Grid.SetRow(Time, Grid.RowDefinitions.Count - 1);
            Grid.SetColumn(Time, 0);
            Grid.SetRow(Data, Grid.RowDefinitions.Count - 1);
            Grid.SetColumn(Data, 1);
        }

        private void RemLine(Grid Grid)
        {
            for(int i = 0; i < Grid.RowDefinitions.Count; i++)
            {
                int id = i * 2 + 1;
                if (Grid.Children[id] is TextBox TextBox)
                {
                    if(TextBox.Text.Length == 0)
                    {
                       
                    }
                }
            }
        }

        private void New()
        {
            this.Monday.Children.Clear();
            this.Tuesday.Children.Clear();
            this.Wednesday.Children.Clear();
            this.Thursday.Children.Clear();
            this.Friday.Children.Clear();
            this.Saturday.Children.Clear();
            this.Sunday.Children.Clear();
        }
        private void SaveData()
        {
            Dictionary<String, Dictionary<String, String>> Data = new Dictionary<string, Dictionary<string, string>>();
            var dialog = new SaveFileDialog
            {
                Filter = "Данные о расписании(*.xml)|*.xml"
            };
            if(dialog.ShowDialog() == true)
            {
                foreach(Grid Item in Grids)
                {
                    Data.Add(Item.Name, new Dictionary<string, string>());
                    for(int i = 0; i < Item.RowDefinitions.Count; i++)
                    {
                        int id = i * 2 + 1;
                        if(Item.Children[id] is TextBox DataBox)
                        {
                            if(DataBox.Text.Length != 0)
                            {
                                Data[Item.Name].Add(DataBox.Name, DataBox.Text);
                                if(Item.Children[id - 1] is TextBox TimeBox)
                                    Data[Item.Name].Add(TimeBox.Name, TimeBox.Text);
                            }
                        }

                    }
                    if(!XMLSaver.CreateNew(Data, dialog.FileName, out string Error))
                        Ext.MessageBox("Произошла ошибка при сохранении!", "Внимание!");
                    else
                        Ext.MessageBox("Успешно сохранено!", "Внимание!");
                }
            }
        }
        private void LoadData()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Данные о расписании(*.xml)|*.xml"
            };
            if(dialog.ShowDialog() == true)
            {
                this.Monday.Children.Clear();
                this.Tuesday.Children.Clear();
                this.Wednesday.Children.Clear();
                this.Thursday.Children.Clear();
                this.Friday.Children.Clear();
                this.Saturday.Children.Clear();
                this.Sunday.Children.Clear();
                foreach(Grid Item in this.Grids)
                {
                    if(XMLSaver.Load(out Dictionary<string, string> Data, dialog.FileName, Item.Name, out string Error))
                    {
                        for(int i = 0; i < Data.Count; i += 2)
                        {
                            string line_key = Data.Keys.ToArray()[i + 1];
                            string data_key = Data.Keys.ToArray()[i];
                            this.AddLine(Item, Data[line_key], Data[data_key]);
                        }
                    }
                }
            }
        }

        private void PrintData()
        {
            if(System.IO.File.Exists("templates\\Расписание.docx"))
            {
                if(this.Word == null)
                    this.Word = new WordApp();
                WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Расписание.docx");
                foreach(Grid Item in Grids)
                {
                    StringBuilder Data = new StringBuilder();
                    String Name = Item.Name.ToLower();
                    for(int i = 0; i < Item.RowDefinitions.Count; i++)
                    {
                        int id = i * 2 + 1;
                        if(Item.Children[id] is TextBox DataBox)
                        {
                            if(DataBox.Text.Length != 0)
                            {
                                if(Item.Children[id - 1] is TextBox TimeBox)
                                    Data.Append(TimeBox.Text + " - " + DataBox.Text + "\r\n");
                            }
                        }

                    }
                    if(oDoc.Bookmarks.Exists(Name))
                        oDoc.Bookmarks[Name].Range.Text = Data.ToString();
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Файлы MS Word(*.docx)|*.docx"
                };
                if(dialog.ShowDialog() == true)
                {
                    oDoc.SaveAs(FileName: dialog.FileName);
                    System.Diagnostics.Process.Start(dialog.FileName);
                } else
                {
                    oDoc.SaveAs("Расписание.docx");
                }
                oDoc.Close();
            } else
                Ext.MessageBox("Шаблон Расписание.dotx не найден в папке templates!");
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }
        // ввод времени
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(sender is TextBox TextBox)
            {
                int sel = TextBox.SelectionStart;
                if(TextBox.Text.IndexOf(":") > -1 && sel > TextBox.Text.IndexOf(":"))
                    sel--;
                TextBox.Text = TextBox.Text.Replace(":", "");
                TextBox.SelectionStart = sel;
                TextBox.SelectionLength = 1;
            }
            if(!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox TextBox)
            {
                StringBuilder Result = new StringBuilder();
                Result.Insert(0, TextBox.Text.Replace(":", "") + "0000");
                Result.Insert(2, ":");
                TextBox.Text = Result.ToString().Substring(0, 5);
            }
        }
       
        // клик добавить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Monday);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Tuesday);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Wednesday);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Thursday);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Friday);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Saturday);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            this.AddLine(this.Sunday);
        }
        // новый
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(Ext.MessageBox("Вы действиельно хотите очистить все данные?", "Внимание!", MessageBoxButton.YesNo) == true)
                this.New();
        }
        // открыть
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.LoadData();
        }
        // сохранить
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            this.SaveData();
        }
        // печать
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            this.PrintData();
        }
    }
}
