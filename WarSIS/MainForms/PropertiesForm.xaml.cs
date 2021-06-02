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

using WarSISModelsDB.Models;
using WarSISModelsDB.Models.Data;

using PropertiesTable = WarSISModelsDB.Models.DataBase.Properties;
using WarSISModelsDB.Models.DataBase.Property;
using WarSISModelsDB.Models.DataBase;
using WarSISModelsDB.Models.DataBase.Subdivision;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

using WordApp = Microsoft.Office.Interop.Word.Application;
using WordDoc = Microsoft.Office.Interop.Word.Document;

namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class PropertiesForm : Window
    {
        MSSQLEngine DB = null;
        String LastError = "";

        IDataBaseElement ActiveProperty = null;
        IProperty ActiveItem = null;

        WordApp Word = null;

        public PropertiesForm(MSSQLEngine DataBase)
        {
            this.InitializeComponent();
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
            this.Inventory_Value.Text = "";
            this.Subdivision_Value.Text = "";
        }

        // ------------------------------------ load -----------

        // WindowLoaded
        /// <summary>
        /// Загрузка типов имущества
        /// </summary>
        private void LoadPropertyType()
        {
            // создаём экземпляр класса "Подразделения"
            PropertiesTable Table = Ext.Create<PropertiesTable>(this.DB);
            // получаем все элементы и добавляем их в выпадающий список
            foreach(Property Item in Table.Select())
                this.TypeBox.Items.Add(Item.Table);
            // если элементы есть, то выбираем первы элемент
            if(this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = 0;
            else
                // иначе выдаём сообщение
                Ext.MessageBox("Типы имущества не были загружены!");
        }

        // TypeBoxChanged
        /// <summary>
        /// Загрузка списка подразделений
        /// </summary>
        private void LoadProperties()
        {
            // получаем активный тип имущества
            string Text = this.TypeBox.SelectedItem.ToString();
            if(Text.Length > 0)
            {
                // создаём экземпляр класса активного имущества (Автоматы, БМП и т.д.)
                // и запоминаем его
                this.ActiveProperty = Ext.Create<PropertiesTable>(this.DB).GetProperty(Text);
                // если данный экземпляр реализует интерфейс 
                if(this.ActiveProperty is IDataBaseElement<IProperty> Table)
                {
                    // очищаем список подразделений
                    this.ListBox.Items.Clear();
                    // получаем все элементы и заполняем список подраздлелений
                    foreach(IProperty Item in Table.Select())
                        this.ListBox.Items.Add(Item.Title);
                } else
                    // если нет - выводим сообщение об ошибке
                    Ext.MessageBox("Имущества не были загружены!");
                // если элементы были загружены, то выбираем певрый
                if(this.ListBox.Items.Count > 0)
                    this.ListBox.SelectedIndex = 0;
            }
        }

        // ListBoxChanged
        /// <summary>
        /// Загрузка данных выбранного подразделения
        /// </summary>
        private void LoadData()
        {
            // очищаем поля с данными
            this.ClearFields();
            // получаем название выбранного имущества
            string Prop = this.ListBox.SelectedItem?.ToString();
            // если название выбрано и существует класс активного имущества
            if(Prop?.Length > 0 && this.ActiveProperty != null)
            {
                // если класс реализует интерфейсы, необходимые для имущества
                if(this.ActiveProperty is IDataBaseElement<IProperty> Table
                    && this.ActiveProperty is IDataBaseProperties tmp)
                {
                    // получаем данные из таблицы
                    List<IProperty> Item = Table.Select($"{tmp.TitleName} Like '{Prop}'");
                    // если данные были получены
                    if(Item.Count > 0)
                    {
                        // запоминаем полученные данные
                        this.ActiveItem = Item[0];
                        // выводим данные о ИД, Названии и Инвентарном номере в поля формы
                        this.ID_Value.Text = this.ActiveItem.ID.ToString();
                        this.Name_Value.Text = this.ActiveItem.Title.ToString();
                        this.Inventory_Value.Text = this.ActiveItem.Inventary.ToString();

                        // ищем в каком подразделении находится имущество
                        Property Prop22 = Ext.Create<PropertiesTable>(this.DB).SelectFirst($"{PropertiesTable.Table} Like '{this.TypeBox.SelectedItem}'");
                        PropertyInSubdivision Data = Ext.Create<PropertiesInSubdivissions>(this.DB).SelectFirst($"{PropertiesInSubdivissions.Property} = {Prop22.ID} and {PropertiesInSubdivissions.PropertyID} = {this.ActiveItem.ID}");
                        if(Data != null)
                        {
                            // Получаем тип подразделение
                            Subdivisions SubdTable = Ext.Create<Subdivisions>(this.DB);
                            Subdivision Subdivision = SubdTable.SelectFirst($"{Subdivisions.ID} = {Data.Subdivision}");
                            if(Subdivision != null)
                            {
                                IDataBaseElement temp = SubdTable.GetSubdivision(Subdivision.Table);
                                if(temp is IDataBaseSubdivisions SubdivisionTable &&
                                   temp is IDataBaseElement<ISubdivision> subd)
                                {
                                    List<ISubdivision> Items = subd.Select($"{SubdivisionTable.IdName} = {Data.SubdivisionID}");
                                    if(Items.Count > 0)
                                        this.Subdivision_Value.Text = Items[0].Title;
                                }
                                
                            }
                            
                        }
                    }
                } else
                    Ext.MessageBox("Имущество не было загружено!");
            }

        }
        
        private List<String> LoadFields(String PropertyName)
        {
            List<String> Result = new List<String>();
            IDataBaseElement PropertyData = Ext.Create<PropertiesTable>(this.DB).GetProperty(PropertyName);
            if(PropertyData != null)
            {
                if(PropertyData is Artilleries)
                {
                    Result.Add(Artilleries.Caliber);
                    Result.Add(Artilleries.Type);
                } else
                if(PropertyData is Automobils)
                {
                    Result.Add(Automobils.Peoples);
                    Result.Add(Automobils.Tank);
                    Result.Add(Automobils.Fuel);
                } else
                if(PropertyData is BMPs)
                {
                    Result.Add(BMPs.Shassie);
                    Result.Add(BMPs.Peoples);
                    Result.Add(BMPs.Tank);
                    Result.Add(BMPs.Fuel);
                } else
                if(PropertyData is RocketAmmos)
                {
                    Result.Add(RocketAmmos.Cannons);
                } else
                if(PropertyData is Tractors)
                {
                    Result.Add(Tractors.Weight);
                    Result.Add(Tractors.Shassie);
                    Result.Add(Tractors.Tank);
                    Result.Add(Tractors.Fuel);
                }
            }
            return Result;
        }

        private void SetFields(List<String> Items)
        {
            this.Fields.Children.Clear();
            foreach(string Item in Items)
            {
                Grid Grid = new Grid()
                {
                    Height = 30
                };
                Grid.RowDefinitions.Add(new RowDefinition());
                Grid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.ColumnDefinitions.Add(new ColumnDefinition());

                Grid.Children.Add(new TextBlock()
                {
                    Text = $"{Item}:",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25
                });
                var Item2 = new TextBlock()
                {
                    Name = Item.Replace(" ", ""),
                    Text = $"",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10, 0, 10, 0),
                    Height = 25,
                };
                Grid.Children.Add(Item2);
                Grid.SetColumn(Item2, 1);
                this.Fields.Children.Add(Grid);
            }
            this.ParamsBox.Height = 30 * (Items.Count() + 1);
            this.LoadFieldsData();
        }

        private void LoadFieldsData()
        {
            if(this.ActiveItem != null)
            {
                if(this.ActiveItem is Artillery Item1)
                {
                    foreach(Grid Item in this.Fields.Children)
                    {
                        if(Item.Children[1] is TextBlock text)
                        {
                            if(text.Name.CompareTo(Artilleries.Caliber.Replace(" ", "")) == 0)
                                text.Text = Item1.Caliber.ToString();
                            else
                            if(text.Name.CompareTo(Artilleries.Type.Replace(" ", "")) == 0)
                                text.Text = Item1.Type;
                        }
                    }

                } else
                if(this.ActiveItem is Automobil Item2)
                {
                    foreach(Grid Item in this.Fields.Children)
                    {
                        if(Item.Children[1] is TextBlock text)
                        {
                            if(text.Name.CompareTo(Automobils.Peoples.Replace(" ", "")) == 0)
                                text.Text = Item2.Peoples.ToString();
                            else
                            if(text.Name.CompareTo(Automobils.Tank.Replace(" ", "")) == 0)
                                text.Text = Item2.Tank.ToString();
                            else
                            if(text.Name.CompareTo(Automobils.Fuel.Replace(" ", "")) == 0)
                                text.Text = Item2.Fuel;
                        }
                    }
                } else
                if(this.ActiveItem is BMP Item3)
                {
                    foreach(Grid Item in this.Fields.Children)
                    {
                        if(Item.Children[1] is TextBlock text)
                        {
                            if(text.Name.CompareTo(BMPs.Shassie.Replace(" ", "")) == 0)
                                text.Text = Item3.Shassie;
                            else
                            if(text.Name.CompareTo(BMPs.Peoples.Replace(" ", "")) == 0)
                                text.Text = Item3.Peoples.ToString();
                            else
                            if(text.Name.CompareTo(BMPs.Tank.Replace(" ", "")) == 0)
                                text.Text = Item3.Tank.ToString();
                            else
                            if(text.Name.CompareTo(BMPs.Fuel.Replace(" ", "")) == 0)
                                text.Text = Item3.Fuel;
                        }
                    }
                } else
                if(this.ActiveItem is RocketAmmo Item4)
                {
                    foreach(Grid Item in this.Fields.Children)
                    {
                        if(Item.Children[1] is TextBlock text)
                        {
                            if(text.Name.CompareTo(RocketAmmos.Cannons.Replace(" ", "")) == 0)
                                text.Text = Item4.Cannons.ToString();
                        }
                    }
                } else
                if(this.ActiveItem is Tractor Item5)
                {
                    foreach(Grid Item in this.Fields.Children)
                    {
                        if(Item.Children[1] is TextBlock text)
                        {
                            if(text.Name.CompareTo(Tractors.Weight.Replace(" ", "")) == 0)
                                text.Text = Item5.Weight.ToString();
                            else
                            if(text.Name.CompareTo(Tractors.Shassie.Replace(" ", "")) == 0)
                                text.Text = Item5.Shassie;
                            else
                            if(text.Name.CompareTo(Tractors.Tank.Replace(" ", "")) == 0)
                                text.Text = Item5.Tank.ToString();
                            else
                            if(text.Name.CompareTo(Tractors.Fuel.Replace(" ", "")) == 0)
                                text.Text = Item5.Fuel;
                        }
                    }
                }
            }
        }

        // ------------------------------------ add ------------

        private void AddPropertyFieldValues(String TypeName, ref Dictionary<string, object> Fields, List<String> FieldValues)
        {
            IDataBaseElement PropertyData = Ext.Create<PropertiesTable>(this.DB).GetProperty(TypeName);
            if(PropertyData != null)
            {
                if(PropertyData is Artilleries)
                {
                    Fields.Add(Artilleries.Caliber, FieldValues[0]);
                    Fields.Add(Artilleries.Type, FieldValues[1]);
                } else
                if(PropertyData is Automobils)
                {
                    Fields.Add(Automobils.Peoples, FieldValues[0]);
                    Fields.Add(Automobils.Tank, FieldValues[1]);
                    Fields.Add(Automobils.Fuel, FieldValues[2]);
                } else
                if(PropertyData is BMPs)
                {
                    Fields.Add(BMPs.Shassie, FieldValues[0]);
                    Fields.Add(BMPs.Peoples, FieldValues[1]);
                    Fields.Add(BMPs.Tank, FieldValues[2]);
                    Fields.Add(BMPs.Fuel, FieldValues[3]);
                } else
                if(PropertyData is RocketAmmos)
                {
                    Fields.Add(RocketAmmos.Cannons, FieldValues[0]);
                } else
                if(PropertyData is Tractors)
                {
                    Fields.Add(Tractors.Weight, FieldValues[0]);
                    Fields.Add(Tractors.Shassie, FieldValues[1]);
                    Fields.Add(Tractors.Tank, FieldValues[2]);
                    Fields.Add(Tractors.Fuel, FieldValues[3]);
                }
            }
        }

        /// <summary>
        /// Добавление нового имущества
        /// </summary>
        /// <param name="TypeName">Тип имущества</param>
        /// <param name="NewTitle">название имущества</param>
        /// <param name="InventoryValue">Инвентарный номер имущства</param>
        /// <param name="FieldValues">Данные дополнительных полей</param>
        private void AddProperty(String TypeName, String NewTitle, Int32 InventoryValue, List<String> FieldValues)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(NewTitle.Length > 0)
            {
                // создаём экземпляр класса "Имущество"
                PropertiesTable Properties = Ext.Create<PropertiesTable>(this.DB);
                // создаём экземпляр класса имущества по названию типа
                IDataBaseElement Property = Properties.GetProperty(TypeName);
                // если класс подразделения реализует интерфейс имущества
                if(Property is IDataBaseProperties subd)
                {
                    // создаём таблицу для добавления
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        {subd.IdName, Property.Max(subd.IdName) + 1},
                        {subd.TitleName, NewTitle},
                        {subd.InventaryName, InventoryValue}
                    };
                    this.AddPropertyFieldValues(TypeName, ref Fields, FieldValues);
                    // добавляем данные в таблицу
                    if(Property.Insert(Fields))
                    {
                        // если было добавлено - обновляем данные формы
                        this.LoadProperties();
                        isAccept = true;
                    } else
                        // если нет, то запоминаем текст ошибки БД
                        this.LastError = Property.GetLastError();
                } else
                    this.LastError = "Ошибка метода GetProperty. Получен неверный класс имущества!";
            } else
                this.LastError = "Имя имущества не было введено!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void AddToSubdivision(String SubdivisionType, String SudivisionName)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";

            // создаём экземпляр класса "Подразделения"
            Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
            Subdivision Subditem = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} like '{SubdivisionType}'");
            // создаём экземпляр класса подразделения по названию типа
            IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(SubdivisionType);
            // если класс подразделения реализует интерфейс подраздления
            if(Subdivision is IDataBaseElement<ISubdivision> subd &&
                Subdivision is IDataBaseSubdivisions Table)
            {
                List<ISubdivision> Items = subd.Select($"{Table.TitleName} Like '{SudivisionName}'");
                if(Items.Count > 0)
                {
                    Property Prop22 = Ext.Create<PropertiesTable>(this.DB).SelectFirst($"{PropertiesTable.Table} Like '{this.TypeBox.SelectedItem}'");
                    PropertiesInSubdivissions PropertInSubd = Ext.Create<PropertiesInSubdivissions>(this.DB);
                    PropertInSubd.Delete($"{PropertiesInSubdivissions.Property} = {Prop22.ID} and {PropertiesInSubdivissions.PropertyID} = {this.ActiveItem.ID}");

                    // создаём таблицу для добавления
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        {PropertiesInSubdivissions.Property, Prop22.ID},
                        {PropertiesInSubdivissions.PropertyID, this.ActiveItem.ID},
                        {PropertiesInSubdivissions.Subdivision, Subditem.ID},
                        {PropertiesInSubdivissions.SubdivisionID, Items[0].ID}
                    };

                    if(PropertInSubd.Insert(Fields))
                    {
                        // если было добавлено - обновляем данные формы
                        this.LoadProperties();
                        isAccept = true;
                    } else
                        // если нет, то запоминаем текст ошибки БД
                        this.LastError = PropertInSubd.GetLastError();
                } else
                    this.LastError = "Ошибка метода GetProperty. Получен неверный класс имущества!";
            }

            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }
        
        // ------------------------------------ get ------------

        private Dictionary<String, List<String>> GetAdditionFields()
        {
            Dictionary<String, List<String>> Result = new Dictionary<string, List<string>>();

            List<Property> Properties = Ext.Create<PropertiesTable>(this.DB).Select();
            foreach(Property Property in Properties)
                Result.Add(Property.Title, this.LoadFields(Property.Table));

            return Result;
        }

        /// <summary>
        /// Генерация списка подходящих для таблицы "Передать в подраздленение"
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, List<String>> GetSubdivisions()
        {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            // создаём экземпляр класса "Подразделения"
            Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
            // получаем данные о типе подразделения
            List<Subdivision> Items = SubdivisionsTable.Select();
            // для каждого элемента
            foreach(Subdivision Item in Items)
            {
                List<string> values = new List<string>();
                // получаем данные о типе подразделения по индексу
                Subdivision subd = SubdivisionsTable.SelectFirst($"{Subdivisions.ID} = {Item.ID}");
                // получаем класс подразделения
                IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(subd.Table);
                // если класс реализует интерфейс подрезделений
                if(Subdivision is IDataBaseElement<ISubdivision> Table)
                {
                    // заполняем список подразделений
                    foreach(ISubdivision Subd in Table.Select())
                        values.Add(Subd.Title);
                }
                Result.Add(subd.Table, values);
            }
            return Result;
        }

        // ------------------------------------ print ------------

        private void PrintToBuy(String PropertyName)
        {
            if(PropertyName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Приобретение.docx"))
                {
                    SharedForms.BuyTemplate form = new SharedForms.BuyTemplate();
                    if(form.ShowDialog() == true)
                    {
                        int count = form.Count;
                        if(this.Word == null)
                            this.Word = new WordApp();
                        WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Приобретение.docx");
                        if(oDoc.Bookmarks.Exists("property"))
                            oDoc.Bookmarks["property"].Range.Text = PropertyName;
                        if(oDoc.Bookmarks.Exists("count"))
                            oDoc.Bookmarks["count"].Range.Text = count.ToString();
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
                            oDoc.SaveAs("Приобретение.docx");
                        }
                        oDoc.Close();
                    }
                } else
                    Ext.MessageBox("Шаблон Приобретение.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите подразделение!");
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadPropertyType();
        }
        
        // Добавить имущество
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Form = new PropertyForms.AddProperty(this.TypeBox.Items.ToStringList(), this.GetAdditionFields());
            if(Form.ShowDialog() == true)
                this.AddProperty(Form.TypeName, Form.NewTitle, Form.Inventory, Form.FieldValues);
        }
        // Передать имущество подразделению
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var Form = new PropertyForms.ToSubdivision(this.Name_Value.Text, this.GetSubdivisions());
            if(Form.ShowDialog() == true)
                this.AddToSubdivision(Form.Type, Form.Subdivision);
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadProperties();
            this.SetFields(this.LoadFields(this.TypeBox.SelectedItem?.ToString()));
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadData();
            this.LoadFieldsData();
        }

        private void Inventory_Value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!Char.IsDigit(e.Text, 0))
                e.Handled = true;
        }

        // сохранение изменений
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveProperty is IDataBaseProperties prop && this.ActiveItem != null)
            {
                // создаём список на изменение
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    {prop.TitleName, this.Name_Value.Text},
                    {prop.InventaryName, this.Inventory_Value.Text},
                };
                // изменяем
                if(this.ActiveProperty.Update(Fields, $"{prop.IdName} = {this.ActiveItem.ID}"))
                {
                    // если всё хорошо - обновляем данные формы
                    isAccept = true;
                    this.LoadProperties();
                } else
                    // если нет, то запоминаем последнюю ошибку БД
                    this.LastError = this.ActiveProperty.GetLastError();
            } else
                this.LastError = "Не выбрано имущество!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }
        // удалить имущество
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool? isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveProperty is IDataBaseProperties subd && this.ActiveItem != null)
            {
                // выводим сообщение, действительно ли пользователь хочет удалить подразделение
                if(Ext.MessageBox("Вы действительно хотите удалить имущество", "Внимание!", MessageBoxButton.YesNo) == true)
                {
                    // удаляем подразделение
                    if(this.ActiveProperty.Delete($"{subd.IdName} = {this.ActiveItem.ID}"))
                    {
                        Property PropTableID = Ext.Create<PropertiesTable>(this.DB).SelectFirst($"{PropertiesTable.TableName} Like '{this.ActiveProperty.GetTableName()}'");
                        if(PropTableID != null)
                        {
                            if(!Ext.Create<PropertiesInSubdivissions>(this.DB).Delete($"{PropertiesInSubdivissions.Property} = {PropTableID.ID} and {PropertiesInSubdivissions.PropertyID} = {this.ActiveItem.ID}"))
                                this.LastError = PropertiesInSubdivissions.LastError;
                        }
                        // если всё хорошо - обновляем данные формы
                        isAccept = true;
                        this.LoadProperties();
                    }
                } else
                    isAccept = null;
            } else
                this.LastError = "Не выбрано имущество!";
            // выводим результат работы операции на пользователю
            if(isAccept != null)
                this.ViewResult(isAccept.Value);
        }

        private void InfoColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // если ошибка существует - вывести на экран
            if(this.LastError?.Length > 0)
                new SharedForms.MessageBox(this.LastError, "Текст последней ошибки БД").ShowDialog();
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem owner = (MenuItem)sender;
            Popup child = (Popup)owner.Template.FindName("PART_Popup", owner);
            child.Placement = PlacementMode.Left;
            child.HorizontalOffset = -owner.ActualWidth;
            child.VerticalOffset = owner.ActualHeight;
        }

        // Количество техники
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();

            int Count = Ext.Create<Automobils>(this.DB).Count();
            Data.Add("Автомобили", Count.ToString());
            Count = Ext.Create<Tractors>(this.DB).Count();
            Data.Add("Тягачи", Count.ToString());
            Count = Ext.Create<BMPs>(this.DB).Count();
            Data.Add("БМП", Count.ToString());
            Count = Ext.Create<Artilleries>(this.DB).Count();
            Data.Add("Артиллерия", Count.ToString());

            new ReportForm("Количество техники по типам", Data).ShowDialog();
        }
        // Количество вооружения
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();

            int Count = Ext.Create<Authomats>(this.DB).Count();
            Data.Add("Автоматы", Count.ToString());
            Count = Ext.Create<Carabins>(this.DB).Count();
            Data.Add("Карабины", Count.ToString());
            Count = Ext.Create<RocketAmmos>(this.DB).Count();
            Data.Add("Ракетное оружие", Count.ToString());

            new ReportForm("Количество вооружения по типам", Data).ShowDialog();
        }
        
        // шаблон заявки на приобретение
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
                this.PrintToBuy(this.ActiveItem.Title);
            else
                Ext.MessageBox("Выберите имущество!");
        }
    }
}
