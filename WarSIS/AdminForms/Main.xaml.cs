using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WarSISDataBase.DataBase;

using WarSISModelsDB.Models;
using WarSISModelsDB.Models.Data;
using WarSISDataBase.Args;
using WarSISModelsDB.Models.DataBase;

using PropertiesTable = WarSISModelsDB.Models.DataBase.Properties;

namespace WarSIS.AdminForms
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        MSSQLEngine DB = null;
        Dictionary<String, Object> Tables = MainExt.Tables;

        Type DataBaseType = null;
        IDataBaseElement Table = null;
        String TableIDName = "";
        DataTable Data = null;

        public Main(MSSQLEngine DataBase)
        {
            InitializeComponent();
            this.DB = DataBase;
        }

        #region -- addition methods

        private void LoadData()
        {
            int i;
            DataTable Table = this.Table.SelectData();
            for (i = 0; i < Table.Columns.Count; i++)
            {
                var Column = this.FindName($"Column_{(i + 1)}");
                if (Column != null)
                    (Column as TextBlock).Text = Table.Columns[i].ColumnName;
                var TextBox = this.FindName($"ValueBox_{(i + 1)}");
                if (TextBox != null)
                {
                    (TextBox as TextBox).Visibility = Visibility.Visible;
                    (TextBox as TextBox).Text = "";
                }
            }
            for (; i < 9; i++)
            {
                var Column = this.FindName($"Column_{(i + 1)}");
                if (Column != null)
                    (Column as TextBlock).Text = "";
                var TextBox = this.FindName($"ValueBox_{(i + 1)}");
                if (TextBox != null)
                {
                    (TextBox as TextBox).Visibility = Visibility.Hidden;
                    (TextBox as TextBox).Text = "";
                }
            }

            this.ListBox.Items.Clear();
            for (i = 0; i < Table.Rows.Count; i++)
            {
                this.ListBox.Items.Add($"{Table.Rows[i].ItemArray[0]}. {Table.Rows[i].ItemArray[1]}");
            }

            this.Data = Table;
 
        }

        private void LoadValues()
        {
            if (this.ListBox.SelectedIndex > -1)
            {
                var TempData = this.Table.SelectData($"{this.TableIDName} = {Data.Rows[this.ListBox.SelectedIndex].ItemArray[0]}", new List<ISelectArgs>() { new TOP(1) });
                MainExt.Load(this, TempData);
            }
            this.InfoBlock.Text = "";
            this.InfoColor.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void SaveData(SaveDataType Type = SaveDataType.Chesk)
        {
            int tmp = -1;
            bool isAccept = false;

            if (Type == SaveDataType.Chesk)
            {
                Type = (this.Table.Count($"{TableIDName} = {Data.Rows[this.ListBox.SelectedIndex].ItemArray[0]}") > 0)
                    ? SaveDataType.Update
                    : SaveDataType.Insert;
            }

            var Fields = MainExt.GenerateValues(this, this.Table, Type);
            if (Type == SaveDataType.Update)
            {
                if (this.ListBox.SelectedIndex > -1 && Fields.Count > 0
                    && this.Table.Update(Fields, $"{this.TableIDName} = {Data.Rows[this.ListBox.SelectedIndex].ItemArray[0]}"))
                {
                    tmp = this.ListBox.SelectedIndex;
                    isAccept = true;
                }
            }
            else
            {
                if (Fields.Count > 0 && this.Table.Insert(Fields))
                {
                    tmp = this.ListBox.Items.Count;
                    isAccept = true;
                }
            }
            if (isAccept) { 
                this.LoadData();
                if (tmp > -1 && this.ListBox.Items.Count > tmp)
                    this.ListBox.SelectedIndex = tmp; 
            }

            this.ViewResult(isAccept);
        }

        private void DeleteData()
        {
            int tmp = 0;
            bool isAccept = false;
            if (this.ListBox.SelectedIndex > -1 
                && this.Table.Delete($"{this.TableIDName} = {Data.Rows[this.ListBox.SelectedIndex].ItemArray[0]}"))
            {
                tmp = (this.ListBox.SelectedIndex < this.ListBox.Items.Count - 1) 
                    ? this.ListBox.SelectedIndex 
                    : this.ListBox.Items.Count - 2;
                isAccept = true;
            }

            if (isAccept)
            {
                this.LoadData();
                if (tmp > -1 && this.ListBox.Items.Count > tmp)
                    this.ListBox.SelectedIndex = tmp;
            }
            this.ViewResult(isAccept);
        }

        private void ViewResult(Boolean isAccept)
        {
            if (isAccept)
            {
                this.InfoBlock.Text = "Успешно!";
                this.InfoColor.Fill = new SolidColorBrush(Colors.GreenYellow);
            }
            else
            {
                this.InfoBlock.Text = "Ошибка!";
                this.InfoColor.Fill = new SolidColorBrush(Colors.OrangeRed);
            }
        }

        private void FocusNextField(String Name)
        {
            var TextBox = this.FindName($"{Name}") as TextBox;
            if (TextBox != null)
            {
                if (TextBox.Visibility == Visibility.Visible)
                {
                    TextBox.Focus();
                    return;
                }
            }
            this.SaveData(SaveDataType.Chesk);
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TablesBox.Items.Clear();
            foreach (var Item in this.Tables)
                this.TablesBox.Items.Add(Item.Key);
            if(this.TablesBox.Items.Count > 0)
                this.TablesBox.SelectedIndex = 0;
            if (this.ListBox.Items.Count > 0)
                this.ListBox.SelectedIndex = 0;
        }

        private void TablesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TablesBox.SelectedIndex > -1)
            {
                if (this.Tables.TryGetValue(this.TablesBox.SelectedItem.ToString(), out Object Item))
                {
                    this.DataBaseType = MainExt.GetTypeItem(Item);
                    this.Table = MainExt.GetTableItem(Item, this.DB, out this.TableIDName);
                    this.LoadData();
                }
                else
                    new SharedForms.MessageBox("Произошла ошибка при получении данных таблицы!").ShowDialog();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.ListBox.SelectedIndex > -1)
                this.LoadValues();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TablesBox.SelectedIndex > -1)
            {
                int tmp = this.ListBox.SelectedIndex;
                this.LoadData();
                if (tmp > -1)
                    this.ListBox.SelectedIndex = tmp;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.SaveData(SaveDataType.Insert);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.SaveData(SaveDataType.Update);
        }

        private void ValueBox_1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }
        
        private void ValueBox_1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                FocusNextField($"ValueBox_2");
        }

        private void ValueBox_2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_3");
        }

        private void ValueBox_3_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_4");
        }

        private void ValueBox_4_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_5");
        }

        private void ValueBox_5_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_6");
        }

        private void ValueBox_6_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_7");
        }

        private void ValueBox_7_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_8");
        }

        private void ValueBox_8_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FocusNextField($"ValueBox_9");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DeleteData();
        }

        private void InfoColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string LastError = this.Table.GetLastError();
            if (LastError?.Length > 0)
                new SharedForms.MessageBox(LastError, "Текст последней ошибки БД").ShowDialog();
        }
    }


    public static class MainExt {

        public static Dictionary<String, Object> Tables { get; } = new Dictionary<String, Object>()
        {
            { "Звания", new Ranks() },
            { "Подразделения", new Subdivisions() },
            { "Имущество", new PropertiesTable() },
            { "Специальности", new Specialities() },
        };

        public static Type GetTypeItem(Object Item)
        {
            if (Item is Ranks)
            {
                return typeof(Ranks);
            }
            else if (Item is Subdivisions)
            {
                return typeof(Subdivisions);
            }
            else if (Item is PropertiesTable)
            {
                return typeof(PropertiesTable);
            }
            else if (Item is Specialities)
            {
                return typeof(Specialities);
            }
            return null;
        }
        public static IDataBaseElement GetTableItem(Object Item, IDataBaseEditor DataBase, out String TableIDName)
        {
            if (Item is Ranks)
            {
                Ranks Res = Item as Ranks;
                Res.Editor = DataBase;
                TableIDName = Ranks.ID;
                return Res;
            }
            else if (Item is Subdivisions)
            {
                Subdivisions Res = Item as Subdivisions;
                Res.Editor = DataBase;
                TableIDName = Subdivisions.ID;
                return Res;
            }
            else if (Item is PropertiesTable)
            {
                PropertiesTable Res = Item as PropertiesTable;
                Res.Editor = DataBase;
                TableIDName = PropertiesTable.ID;
                return Res;
            }
            else if (Item is Specialities)
            {
                Specialities Res = Item as Specialities;
                Res.Editor = DataBase;
                TableIDName = Specialities.ID;
                return Res;
            }
            TableIDName = "";
            return null;
        }
    
        public static void Load(Main Form, DataTable Data)
        {
            if(Data.Rows.Count > 0)
                for(int i = 0; i < Data.Rows[0].ItemArray.Length; i++)
                {
                    var TextBox = Form.FindName($"ValueBox_{(i + 1)}");
                    if (TextBox != null)
                        (TextBox as TextBox).Text = Data.Rows[0].ItemArray[i].ToString();
                }
        }

        public static Dictionary<String, Object> GenerateValues(Main Form, IDataBaseElement DataBase, SaveDataType Type)
        {
            Dictionary<String, Object> Result = new Dictionary<string, object>();

            if (DataBase is Ranks)
            {
                var ID = Form.FindName($"ValueBox_1") as TextBox;
                var Title = Form.FindName($"ValueBox_2") as TextBox;
                var Table = Form.FindName($"ValueBox_3") as TextBox;
                var Upper = Form.FindName($"ValueBox_4") as TextBox;

                if (ID == null || Title == null || Table == null || Upper == null)
                    return Result;
                if(!Int32.TryParse(ID.Text, out int id) || Title.Text.Length == 0 || Table.Text.Length == 0)
                    return Result;
                int upper;
                if(!Int32.TryParse(Upper.Text, out upper)){
                    upper = -1;
                }
                if(Type == SaveDataType.Update)
                    Result.Add(Ranks.ID, id);
                else
                    Result.Add(Ranks.ID, (DataBase.Max(Ranks.ID) + 1));
                Result.Add(Ranks.Title, Title.Text);
                Result.Add(Ranks.Table, Table.Text);
                Result.Add(Ranks.Upper, upper);
            }
            else if (DataBase is Subdivisions)
            {
                var ID = Form.FindName($"ValueBox_1") as TextBox;
                var Title = Form.FindName($"ValueBox_2") as TextBox;
                var Table = Form.FindName($"ValueBox_3") as TextBox;
                var Upper = Form.FindName($"ValueBox_4") as TextBox;

                if (ID == null || Title == null || Table == null || Upper == null)
                    return Result;
                if (!Int32.TryParse(ID.Text, out int id) || Title.Text.Length == 0 || Table.Text.Length == 0)
                    return Result;

                if (Type == SaveDataType.Update)
                    Result.Add(Subdivisions.ID, id);
                else
                    Result.Add(Subdivisions.ID, (DataBase.Max(Subdivisions.ID) + 1));
                Result.Add(Subdivisions.Title, Title.Text);
                Result.Add(Subdivisions.Table, Table.Text);
                Result.Add(Subdivisions.Upper, Upper.Text);
            }
            else if (DataBase is PropertiesTable)
            {
                var ID = Form.FindName($"ValueBox_1") as TextBox;
                var Title = Form.FindName($"ValueBox_2") as TextBox;
                var Table = Form.FindName($"ValueBox_3") as TextBox;

                if (ID == null || Title == null || Table == null)
                    return Result;
                if (!Int32.TryParse(ID.Text, out int id) || Title.Text.Length == 0 || Table.Text.Length == 0)
                    return Result;

                if (Type == SaveDataType.Update)
                    Result.Add(PropertiesTable.ID, id);
                else
                    Result.Add(PropertiesTable.ID, (DataBase.Max(PropertiesTable.ID) + 1));
                Result.Add(PropertiesTable.Title, Title.Text);
                Result.Add(PropertiesTable.Table, Table.Text);
            }
            else if (DataBase is Specialities)
            {
                var ID = Form.FindName($"ValueBox_1") as TextBox;
                var Title = Form.FindName($"ValueBox_2") as TextBox;

                if (ID == null || Title == null)
                    return Result;
                if (!Int32.TryParse(ID.Text, out int id) || Title.Text.Length == 0)
                    return Result;

                if (Type == SaveDataType.Update)
                    Result.Add(Specialities.ID, id);
                else
                    Result.Add(Specialities.ID, (DataBase.Max(Specialities.ID) + 1));
                Result.Add(Specialities.Title, Title.Text);
            }

            return Result;
        }

    }

    public enum SaveDataType {
        Chesk,
        Insert,
        Update
    }

}
