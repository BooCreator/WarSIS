using System;
using System.Collections.Generic;
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

using WarSISModelsDB;
using WarSISDataBase.DataBase;
using WarSISModelsDB.Models.Data;
using WarSISModelsDB.Models;
using WarSISModelsDB.Models.DataBase;
using System.Windows.Controls.Primitives;
using WarSISModelsDB.Models.DataBase.Subdivision;

namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for BuildingsForm.xaml
    /// </summary>
    public partial class BuildingsForm : Window
    {
        MSSQLEngine DB = null;
        String LastError = "";

        Building ActiveItem = null;

        public BuildingsForm(MSSQLEngine DataBase)
        {
            InitializeComponent();
            this.DB = DataBase;
        }

        #region my_methods

        /// <summary>
        ///  вывод результата о успешности операции
        /// </summary>
        /// <param name="isAccept">Логическое выражение указывающее результат операции</param>
        private void ViewResult(Boolean isAccept)
        {
            if(isAccept)
            {
                this.InfoBlock.Text = "Успешно!";
                this.InfoColor.Fill = new SolidColorBrush(Colors.GreenYellow);
            } else
            {
                this.InfoBlock.Text = "Ошибка!";
                this.InfoColor.Fill = new SolidColorBrush(Colors.OrangeRed);
            }
        }

        /// <summary>
        /// Очистка всех полей
        /// </summary>
        private void ClearFields()
        {
            this.ID_Value.Text = "";
            this.Name_Value.Text = "";
            this.Address_Value.Text = "";
        }

        // ------------------------------------ load -----------

        // WindowLoaded
        /// <summary>
        /// Загрузка типов подразделений
        /// </summary>
        private void LoadBuildings()
        {
            this.ListBox.Items.Clear();
            List<Building> Items = Ext.Create<Buildings>(this.DB).Select();
            foreach(Building Item in Items)
                this.ListBox.Items.Add(Item.Title);
            // если элементы были загружены, то выбираем певрый
            if(this.ListBox.Items.Count > 0)
                this.ListBox.SelectedIndex = 0;
        }

        // ListBoxChanged
        /// <summary>
        /// Загрузка данных выбранного подразделения
        /// </summary>
        private void LoadData()
        {
            // очищаем поля с данными
            this.ClearFields();
            // получаем название здания
            string Build = this.ListBox.SelectedItem?.ToString();
            // если название выбрано и существует класс активного человека
            if(Build?.Length > 0)
            {
                Building Building = Ext.Create<Buildings>(this.DB).SelectFirst($"{Buildings.Title} Like '{Build}'");
                if(Building != null)
                {
                    this.ActiveItem = Building;
                    this.ID_Value.Text = this.ActiveItem.ID.ToString();
                    this.Name_Value.Text = this.ActiveItem.Title;
                    this.Address_Value.Text = this.ActiveItem.Address;
                } else
                    Ext.MessageBox("Здание не было загружено!");
            }
        }

        // ------------------------------------- add --------------

        private void AddBuilding(String Title, String Address)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если данные был ивведены
            if(Title.Length > 0 && Address.Length > 0)
            {
                var BuildingsTable = Ext.Create<Buildings>(this.DB);
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    { Buildings.ID, BuildingsTable.Max($"{Buildings.ID}") + 1 },
                    { Buildings.Title, Title },
                    { Buildings.Address, Address },
                };
                if(BuildingsTable.Insert(Fields))
                    isAccept = true;
                else
                    this.LastError = BuildingsTable.GetLastError();
            }
            if(isAccept)
                // если всё хорошо, то обновляем данные формы
                this.LoadBuildings();
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        // ------------------------------------ get --------------

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }

        private void InfoColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // если ошибка существует - вывести на экран
            if(this.LastError?.Length > 0)
                new SharedForms.MessageBox(this.LastError, "Текст последней ошибки БД").ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadBuildings();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadData();
        }

        // новое здание
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Form = new BuildingForms.AddBuilding();
            if(Form.ShowDialog() == true)
                this.AddBuilding(Form.NewTitle, Form.Address);
        }

        // сохранить
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если класс активного здания реализует интерфейс зданий
            // и данные здания существуют (выбрано конкретное здание)
            if(this.ActiveItem != null)
            {
                // создаём список на изменение
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    {Buildings.Title, this.Name_Value.Text},
                    {Buildings.Address, this.Address_Value.Text},
                };
                // изменяем
                if(Ext.Create<Buildings>(this.DB).Update(Fields, $"{Buildings.ID} = {this.ActiveItem.ID}"))
                {
                    // если всё хорошо - обновляем данные формы
                    isAccept = true;
                    this.LoadBuildings();
                } else
                    // если нет, то запоминаем последнюю ошибку БД
                    this.LastError = Buildings.LastError;
            } else
                this.LastError = "Не выбрано здание!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }
        // удалить
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool? isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс зданий
            // и данные здания существуют (выбрано конкретное здание)
            if(this.ActiveItem != null)
            {
                // выводим сообщение, действительно ли пользователь хочет удалить здание
                if(Ext.MessageBox("Вы действительно хотите удалить здание", "Внимание!", MessageBoxButton.YesNo) == true)
                {
                    // удаляем здание
                    if(Ext.Create<Buildings>(this.DB).Delete($"{Buildings.ID} = {this.ActiveItem.ID}"))
                    {
                        // если всё хорошо - обновляем данные формы
                        isAccept = true;
                        this.LoadBuildings();
                    } else
                        // если нет, то запоминаем последнюю ошибку БД
                        this.LastError = Buildings.LastError;
                } else
                    isAccept = null;
            } else
                this.LastError = "Не выбрано здание!";
            // выводим результат работы операции на пользователю
            if(isAccept != null)
                this.ViewResult(isAccept.Value);
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem owner = (MenuItem)sender;
            Popup child = (Popup)owner.Template.FindName("PART_Popup", owner);
            child.Placement = PlacementMode.Left;
            child.HorizontalOffset = -owner.ActualWidth;
            child.VerticalOffset = owner.ActualHeight;
        }

        // Пустые сооружения
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();
            var buildings = this.DB.Select($"select {Buildings.Title},{Buildings.Address} from {Buildings.TableName} where {Buildings.ID} NOT IN (select {Branches.Building} from {Branches.TableName} GROUP BY {Branches.Building})");
            for(int i = 0; i < buildings.Rows.Count; i++)
                Data.Add(buildings.Rows[i].ItemArray[0].ToString(), buildings.Rows[i].ItemArray[1].ToString());
            new ReportForm("Здания без подразделений", Data).ShowDialog();
        }
        // Сооружения с несколькими подразделениями
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();
            var BuildingTable = Ext.Create<Buildings>(this.DB);
            var items = this.DB.Select($"select {Branches.Building}, COUNT({Branches.Building}) as c from {Branches.TableName} GROUP BY {Branches.Building}");
            for(int i = 0; i < items.Rows.Count; i++)
            {
                if(items.Rows[i].ItemArray[1].ToInt32() > 1)
                {
                    var Build = BuildingTable.SelectFirst($"{Buildings.ID} = {items.Rows[i].ItemArray[0]}");
                    if(Build != null)
                        Data.Add(Build.Title, Build.Address);
                }
            }
            new ReportForm("Здания c несколькими подразделениями", Data).ShowDialog();
        }
        // Занятые здания
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();
            var buildings = this.DB.Select($"select {Buildings.TableName}.{Buildings.Title},{Buildings.TableName}.{Buildings.Address} from {Buildings.TableName} inner join {Branches.TableName} on {Buildings.TableName}.{Buildings.ID} = {Branches.Building}");
            for(int i = 0; i < buildings.Rows.Count; i++)
                Data.Add(buildings.Rows[i].ItemArray[0].ToString(), buildings.Rows[i].ItemArray[1].ToString());
            new ReportForm("Здания c несколькими подразделениями", Data).ShowDialog();
        }
    }
}
