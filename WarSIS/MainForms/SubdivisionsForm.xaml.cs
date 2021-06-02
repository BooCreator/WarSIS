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
using WarSISModelsDB.Models.DataBase;
using WarSISModelsDB.Models.DataBase.Subdivision;
using WarSISModelsDB.Models.Data;
using WarSISModelsDB.Models;
using WarSISModelsDB.Models.DataBase.Property;

using PropertiesTable = WarSISModelsDB.Models.DataBase.Properties;
using System.Windows.Controls.Primitives;

using WordApp = Microsoft.Office.Interop.Word.Application;
using WordDoc = Microsoft.Office.Interop.Word.Document;
using Microsoft.Win32;

namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for Subdivisions.xaml
    /// </summary>
    public partial class SubdivisionsForm : Window
    {
        MSSQLEngine DB = null;
        String LastError = "";

        public IDataBaseElement ActiveSubdivision = null;
        public ISubdivision ActiveItem = null;

        WordApp Word = null;

        public SubdivisionsForm(MSSQLEngine DataBase)
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
            this.Upper_value.Text = "";
            this.Comander_name.Text = "Нет командира";
            this.Comander_Rank.Text = "";
            this.Peoples_Value.Text = "0";
        }

        // ------------------------------------ load -----------

        // WindowLoaded
        /// <summary>
        /// Загрузка типов подразделений
        /// </summary>
        private void LoadSubdivisionsType()
        {
            // создаём экземпляр класса "Подразделения"
            Subdivisions Table = Ext.Create<Subdivisions>(this.DB);
            // получаем все элементы и добавляем их в выпадающий список
            foreach(Subdivision Item in Table.Select())
                this.TypeBox.Items.Add(Item.Table);
            // если элементы есть, то выбираем первы элемент
            if(this.TypeBox.Items.Count > 0)
                this.TypeBox.SelectedIndex = 0;
            else
                // иначе выдаём сообщение
                Ext.MessageBox("Типы отделений не были загружены!");
        }
        // TypeBoxChanged
        /// <summary>
        /// Загрузка списка подразделений
        /// </summary>
        private void LoadSubdivisions()
        {
            // получаем активный тип подразделений
            string Text = this.TypeBox.SelectedItem.ToString();
            if(Text.Length > 0)
            {
                // создаём экземпляр класса активного подразделения (Роты, Отделения и т.д.)
                // и запоминаем его
                this.ActiveSubdivision = Ext.Create<Subdivisions>(this.DB)?.GetSubdivision(Text);
                // если данный экземпляр реализует интерфейс 
                if(this.ActiveSubdivision is IDataBaseElement<ISubdivision> Table)
                {
                    // очищаем список подразделений
                    this.ListBox.Items.Clear();
                    // получаем все элементы и заполняем список подраздлелений
                    foreach(ISubdivision Item in Table.Select())
                        this.ListBox.Items.Add(Item.Title);
                } else
                    // если нет - выводим сообщение об ошибке
                    Ext.MessageBox("Отделения не были загружены!");
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
            // получаем название выбранного подразделения
            string Subdiv = this.ListBox.SelectedItem?.ToString();
            // если название выбрано и существует класс активного подразделения
            if(Subdiv?.Length > 0 && this.ActiveSubdivision != null)
            {
                // если класс реализует интерфейсы, необходимые для подразделений
                if(this.ActiveSubdivision is IDataBaseElement<ISubdivision> Table
                    && this.ActiveSubdivision is IDataBaseSubdivisions tmp)
                {
                    // получаем данные из таблицы
                    List<ISubdivision> Item = Table.Select($"{tmp.TitleName} Like '{Subdiv}'");
                    // если данные были получены
                    if(Item.Count > 0)
                    {
                        // запоминаем полученные данные
                        this.ActiveItem = Item[0];
                        // выводим наддые о ИД и Названии в поля формы
                        this.ID_Value.Text = this.ActiveItem.ID.ToString();
                        this.Name_Value.Text = this.ActiveItem.Title.ToString();

                        var SubdivTable = Ext.Create<Subdivisions>(this.DB);
                        // Получаем тип вышестоящего подразделения
                        Subdivision Upper = SubdivTable.SelectFirst($"{Subdivisions.ID} = {this.ActiveItem.Subdivision}");
                        if(Upper != null)
                        {
                            var UpperTable = SubdivTable.GetSubdivision(Upper.Table);
                            if(UpperTable is IDataBaseElement<ISubdivision> UpperSubd && UpperTable is IDataBaseSubdivisions temp)
                            {
                                var UpperData = UpperSubd.Select($"{temp.IdName} = {this.ActiveItem.SubdivisionID}");
                                if(UpperData.Count > 0)
                                    this.Upper_value.Text = UpperData[0].Title;
                            }
                        }

                        // создаём экземпляр класса "Люди"
                        Peoples PeoplesTable = Ext.Create<Peoples>(this.DB);
                        // получаем данные командира подразделения
                        People Commander = PeoplesTable.SelectFirst($"{Peoples.ID} = {this.ActiveItem.Commander}");
                        // если данные получены
                        if(Commander != null)
                        {
                            // выводим данные в поля формы
                            this.Comander_name.Text = Commander.Name;
                            // получаем звание командира
                            Rank Rank = Ext.Create<Ranks>(this.DB).SelectFirst($"{Ranks.ID} = {Commander.Rank}");
                            // если данные получены, то выводим данные в поля формы
                            if(Rank != null)
                                this.Comander_Rank.Text = Rank.Title;
                        }
                        

                        this.PropertiesGrid.Children.Clear();

                        var SubdivType = SubdivTable.SelectFirst($"{Subdivisions.Table} Like '{this.TypeBox.SelectedItem}'");
                        var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
                        var PropertyOnSundivTable = Ext.Create<PropertiesInSubdivissions>(this.DB);
                        var PropertiesItems = PropertyTable.Select();
                        if(SubdivType != null)
                        {
                            // получаем колчиество людей в подраздленении
                            int Count = PeoplesTable.Count($"{Peoples.Subdivision} = {SubdivType.ID} and {Peoples.SubdivisionID} = {this.ActiveItem.ID}");
                            // выводим данные в поля формы
                            this.Peoples_Value.Text = Count.ToString();

                            foreach(var PropertyItem in PropertiesItems)
                            {
                                int count = PropertyOnSundivTable.Count(
                                    $"{PropertiesInSubdivissions.Subdivision} = {SubdivType.ID} and {PropertiesInSubdivissions.SubdivisionID} = {this.ActiveItem.ID}" +
                                    $"and {PropertiesInSubdivissions.Property} = {PropertyItem.ID}");
                                Grid Grid = new Grid()
                                {
                                    Height = 30
                                };
                                Grid.RowDefinitions.Add(new RowDefinition());
                                Grid.ColumnDefinitions.Add(new ColumnDefinition());
                                Grid.ColumnDefinitions.Add(new ColumnDefinition());

                                Grid.Children.Add(new TextBlock()
                                {
                                    Text = $"{PropertyItem.Title}:",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    Margin = new Thickness(10, 0, 10, 0),
                                    Height = 25
                                });
                                var Item2 = new TextBlock()
                                {
                                    Text = count.ToString(),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    Margin = new Thickness(10, 0, 10, 0),
                                    Height = 25,
                                };
                                Grid.Children.Add(Item2);

                                Grid.SetColumn(Item2, 1);

                                this.PropertiesGrid.Children.Add(Grid);
                            }
                        }
                    }
                } else
                    Ext.MessageBox("Отделения не были загружены!");
            }
        }

        // ------------------------------------- add --------------
        /// <summary>
        /// Добавление нового подразделения
        /// </summary>
        /// <param name="Type">Имя типа подразделения</param>
        /// <param name="NewTitle">Название нового подразделения</param>
        private void AddSubdidision(String Type, String NewTitle)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(NewTitle.Length > 0)
            {
                // создаём экземпляр класса "Подразделения"
                Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                // создаём экземпляр класса подразделения по названию типа
                IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(Type);
                // если класс подразделения реализует интерфейс подраздления
                if(Subdivision is IDataBaseSubdivisions subd)
                {
                    // создаём таблицу для добавления
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        {subd.IdName, Subdivision.Max(subd.IdName) + 1},
                        {subd.TitleName, NewTitle}
                    };
                    // добавляем данные в таблицу
                    if(Subdivision.Insert(Fields)) {
                        // если было добавлено - обновляем данные формы
                        this.LoadSubdivisions();
                        isAccept = true;
                    } else
                        // если нет, то запоминаем текст ошибки БД
                        this.LastError = Subdivision.GetLastError();
                } else
                    this.LastError = "Ошибка метода GetSubdivision. Получен неверный класс подразделения!";
            } else
                this.LastError = "Имя подразделения не было введено!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }
        /// <summary>
        /// Метод изменения вышестоящей для подразделения
        /// </summary>
        /// <param name="TypeName">Имя типа подразделения</param>
        /// <param name="UpperName">Название вышестоящего подразделения</param>
        private void AddUpper(String TypeName, String UpperName)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(UpperName.Length > 0)
            {
                // создаём экземпляр класса "Подразделения"
                Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                // создаём экземпляр класса подразделения по названию типа
                IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(TypeName);
                // если класс подразделения реализует интерфейс подраздления
                if(Subdivision is IDataBaseElement<ISubdivision> Table && Subdivision is IDataBaseSubdivisions subd)
                {
                    // получаем данные выбранной вышестоящей
                    List<ISubdivision> Item = Table.Select($"{subd.TitleName} like '{UpperName}'");
                    // получаем данные о типе вышестоящей
                    Subdivision Item2 = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} like '{TypeName}'");
                    if(Item.Count > 0 && Item2 != null)
                    {
                        // создаём таблицу для изменения
                        Dictionary<string, object> Fields = new Dictionary<string, object>()
                        {
                            {subd.SubdivisionTableName, Item2.ID},
                            {subd.SubdivisionIDName, Item[0].ID}
                        };
                        // обновляем данные
                        if(this.ActiveSubdivision.Update(Fields, $"{subd.IdName} = {this.ID_Value.Text}"))
                        {
                            // если всё прошло хорошо - обновляем данные формы
                            this.LoadSubdivisions();
                            isAccept = true;
                        } else
                            // если нет, то запоминаем текст ошибки БД
                            this.LastError = this.ActiveSubdivision.GetLastError();
                    } else
                        this.LastError = $"{UpperName} не найден!";
                } else
                    this.LastError = "Ошибка метода GetSubdivision. Получен неверный класс подразделения!";
            } else
                this.LastError = "Имя подразделения не было введено!";
            if(isAccept)
            {
                if(this.ActiveItem != null)
                {
                    this.PrintSetUpper(this.ActiveItem.Title, UpperName);
                }
            }
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void AddComander(String PeopleName)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(PeopleName.Length > 0)
            {
                Dictionary<string, object> Fields = new Dictionary<string, object>();
                if(this.ActiveItem != null && this.ActiveSubdivision is IDataBaseSubdivisions subd)
                {
                    Peoples PeoplesTable = Ext.Create<Peoples>(this.DB);
                    Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                    People People = PeoplesTable.SelectFirst($"{Peoples.Name} Like '{PeopleName}'");
                    Subdivision PeopleSubd = SubdivisionsTable.SelectFirst($"{Subdivisions.ID} = '{People.Subdivision}'");
                    if(PeopleSubd != null)
                    {
                        IDataBaseElement SubdClss = SubdivisionsTable.GetSubdivision(PeopleSubd.Table);
                        if(SubdClss is IDataBaseSubdivisions temp)
                        {
                            Fields.Clear();
                            Fields.Add(subd.CommanderName, null);
                            if(!SubdClss.Update(Fields, $"{temp.IdName} = {People.SubdivisionID}"))
                                this.LastError = SubdClss.GetLastError();
                        }
                    }
                    if(People != null)
                    {
                        Fields.Clear();
                        Fields.Add(subd.CommanderName, People.ID);
                        if(this.ActiveSubdivision.Update(Fields, $"{subd.IdName} = {this.ActiveItem.ID}"))
                        {
                            if(Int32.TryParse(this.ID_Value.Text, out int SubdID))
                            {
                                Subdivision SubdTypeID = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} Like '{this.TypeBox.SelectedItem}'");
                                if(SubdTypeID != null)
                                {
                                    Fields.Clear();
                                    Fields.Add(Peoples.Subdivision, SubdTypeID.ID);
                                    Fields.Add(Peoples.SubdivisionID, SubdID);
                                    if(!PeoplesTable.Update(Fields, $"{Peoples.ID} = {People.ID}"))
                                        this.LastError = PeoplesTable.GetLastError();
                                }
                            }
                            isAccept = true;
                            this.LoadSubdivisions();
                        } else
                            this.LastError = this.ActiveSubdivision.GetLastError();
                    }
                } else
                    this.LastError = "Имя человека не было введено!";
            } else
                this.LastError = "Подразделение не выбрано!";
            if(isAccept)
            {
                if(this.ActiveItem != null)
                {
                    this.PrintSetComander(PeopleName, this.ActiveItem.Title);
                }
            }
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void SetProperties(Dictionary<String, List<String>> LastProperties)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";

            if(this.ActiveItem != null)
            {
                int sch = 0;
                var SubdivTable = Ext.Create<Subdivisions>(this.DB);
                var Subdiv = SubdivTable.SelectFirst($"{Subdivisions.Table} Like '{this.TypeBox.SelectedItem}'");
                var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
                var PropertyOnSubdivTable = Ext.Create<PropertiesInSubdivissions>(this.DB);
                
                if(Subdiv != null)
                {
                    foreach(var Item in LastProperties)
                    {
                        int sch2 = 0;
                        var PropType = PropertyTable.SelectFirst($"{PropertiesTable.Title} Like '{Item.Key}'");
                        var proptable = PropertyTable.GetProperty(Item.Key);
                        if(PropType != null && proptable is IDataBaseElement<IProperty> propTable &&
                           proptable is IDataBaseProperties temp)
                        {
                            
                            foreach(string item in Item.Value)
                            {
                                var Property = propTable.Select($"{temp.TitleName} Like '{item}'");
                                if(Property.Count > 0)
                                {
                                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                                    {
                                        { PropertiesInSubdivissions.Property, PropType.ID },
                                        { PropertiesInSubdivissions.PropertyID, Property[0].ID },
                                        { PropertiesInSubdivissions.Subdivision, Subdiv.ID },
                                        { PropertiesInSubdivissions.SubdivisionID, this.ActiveItem.ID },
                                    };
                                    if(!PropertyOnSubdivTable.Delete($"{PropertiesInSubdivissions.Property} = {PropType.ID} and {PropertiesInSubdivissions.PropertyID} = {Property[0].ID}"))
                                        this.LastError = PropertyOnSubdivTable.GetLastError();
                                    if(PropertyOnSubdivTable.Insert(Fields))
                                        sch2++;
                                    else
                                        this.LastError = PropertyOnSubdivTable.GetLastError();
                                } else
                                    this.LastError = "Имущество не найдено!";
                            }
                        } else
                            this.LastError = "Ошибка GetProperty. Класс имущества не найден!";
                        if(sch2 == Item.Value.Count)
                            sch++;
                    }
                }
                isAccept = (sch == LastProperties.Count);
            } else
                this.LastError = "Не выбрано подразделение!";

            if(isAccept)
                // если всё хорошо, то обновляем данные формы
                this.LoadSubdivisions();
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void ToBuilding(String Title)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если постройка была выбрана
            if(Title.Length > 0)
            {
                if(this.ActiveItem != null)
                {
                    Building Building = Ext.Create<Buildings>(this.DB).SelectFirst($"{Buildings.Title} Like '{Title}'");
                    if(Building != null)
                        isAccept = this.ToBuilding(Building.ID, this.ActiveSubdivision, this.ActiveItem.ID);
                    else
                        this.LastError = "Выбранное здание не найдено!";
                } else
                    this.LastError = "Подразделение не выбрано!";
            } else
                this.LastError = "Здание не выбрано!";
            if(isAccept)
                // если всё хорошо, то обновляем данные формы
                this.LoadSubdivisions();
            // выводим результат работы операции на пользователю
            if(isAccept)
            {
                if(this.ActiveItem != null)
                {
                    this.PrintToBuilding(this.ActiveItem.Title, Title);
                }
            }
            this.ViewResult(isAccept);
        }

        private Boolean ToBuilding(Int32 BuildingID, IDataBaseElement Subdivision, Int32 ItemID)
        {
            if(Subdivision != null && BuildingID > -1)
            {
                // если подразделение равно - отделы
                if(Subdivision is Branches)
                {
                    // создаём таблицу для изменения
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        {Branches.Building, BuildingID},
                    };
                    // перемещаем в подразделение
                    if(Subdivision.Update(Fields))
                        return true;
                    else
                        this.LastError = Subdivision.GetLastError();
                // если не равно - то идём на уровень ниже
                } else
                {
                    bool Result = false;
                    // создаём таблицу "Подразделения"
                    Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                    // получаем данные о подразделении
                    Subdivision SubDiv = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} Like '{Subdivision.GetTableName()}'");
                    // если получено
                    if(SubDiv != null)
                    {
                        // получаем список подразделений у которых в вышестоящей указан тип изменяемого подразделения
                        List<Subdivision> DownedSubdvs = SubdivisionsTable.Select($"CONCAT(',',{Subdivisions.Upper},',') LIKE '%,{SubDiv.ID},%'");
                        Result = (DownedSubdvs.Count > 0);
                        // для всех таких подразделений
                        foreach(Subdivision DownedSubdv in DownedSubdvs)
                        {
                            // получаем таблицу для работы
                            IDataBaseElement Table = SubdivisionsTable.GetSubdivision(DownedSubdv.Table);
                            // если класс реализует интерфейсы подразделений
                            if(Table is IDataBaseElement<ISubdivision> WorkTable && Table is IDataBaseSubdivisions tanp)
                            {
                                // получаем список подразделений, у которых в качестве вышестоящей указано изменяемое подразделение
                                List<ISubdivision> Items = WorkTable.Select($"{tanp.SubdivisionIDName} = {ItemID}");
                                // для каждого такого подразделения
                                foreach(ISubdivision Item in Items)
                                    // рекурсивно вызываем метод
                                    if(!this.ToBuilding(BuildingID, Table, Item.ID))
                                        // если хоть одно изменение прошло не успешно - выводим результат
                                        Result = false;
                            }
                        }
                    } else
                        this.LastError = "Не найдено подразделение!";
                    return Result;
                }
            } else
                this.LastError = "Не указано здание или подразделение!";
            return false;
        }

        // ------------------------------------ get --------------

        /// <summary>
        /// Генерация списка подходящих для таблицы "Изменить вышестоящую"
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, List<String>> GetValidUpper()
        {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            // если существует выбранный тип подразделения
            if(this.ActiveSubdivision != null)
            {
                // создаём экземпляр класса "Подразделения"
                Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                // получаем данные о типе подразделения
                Subdivision items = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} Like '{this.TypeBox.SelectedItem}'");
                string[] uppers = null;
                // если данные были получены, то получаем индексы подразделений, которые могут быть вышестоящими
                if(items != null)
                    uppers = items.Upper.Split(',');
                // для каждого индекса
                foreach(string item in uppers)
                {
                    List<string> values = new List<string>();
                    // получаем данные о типе подразделения по индексу
                    Subdivision subd = SubdivisionsTable.SelectFirst($"{Subdivisions.ID} = {item}");
                    // получаем класс подразделения
                    IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(subd.Table);
                    // если класс реализует интерфейс подрезделений
                    if(Subdivision is IDataBaseElement<ISubdivision> Table)
                    {
                        // заполняем список подразделений
                        foreach(ISubdivision Item in Table.Select())
                            values.Add(Item.Title);
                    }
                    Result.Add(subd.Table, values);
                }
            } else
                Ext.MessageBox("Выберите подразделение!");
            return Result;
        }

        public Dictionary<String, List<String>> GetPeoplesInRanks() {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            List<Rank> Ranks = Ext.Create<Ranks>(this.DB).Select();
            if(Ranks.Count > 0)
            {
                Peoples PeoplesTable = Ext.Create<Peoples>(this.DB);
                foreach(Rank Rank in Ranks)
                {
                    List<string> Items = new List<string>();
                    List<People> peoples = PeoplesTable.Select($"{Peoples.Rank} = {Rank.ID}");
                    foreach(People people in peoples)
                        Items.Add(people.Name);
                    Result.Add(Rank.Table, Items);
                }
            }
            return Result;
        }

        public Dictionary<String, List<String>> GetPeoperties()
        {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
            var Items = PropertyTable.Select();
            foreach(var Item in Items)
            {
                List<string> items = new List<string>();
                var Property = PropertyTable.GetProperty(Item.Table);
                if(Property is IDataBaseElement<IProperty> property)
                {
                    var props = property.Select();
                    foreach(var prop in props)
                        items.Add(prop.Title);
                }
                Result.Add(Item.Table, items);
            }
            return Result;
        }

        public Dictionary<String, List<String>> GetSubdivisionPeoperties()
        {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            var SubdivTable = Ext.Create<Subdivisions>(this.DB);
            var Subdiv = SubdivTable.SelectFirst($"{Subdivisions.Table} Like '{this.TypeBox.SelectedItem}'");
            var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
            var PropTypes = PropertyTable.Select();
            foreach(var Item in PropTypes)
                Result.Add(Item.Table, new List<string>());
            if(this.ActiveItem != null && Subdiv != null)
            {
                var Items = Ext.Create<PropertiesInSubdivissions>(this.DB).Select($"{PropertiesInSubdivissions.Subdivision} = {Subdiv.ID} and {PropertiesInSubdivissions.SubdivisionID} = {this.ActiveItem.ID}");
                foreach(var Item in Items)
                {
                    var Prop = PropertyTable.SelectFirst($"{PropertiesTable.ID} = {Item.Property}");
                    List<string> items = new List<string>();
                    var Property = PropertyTable.GetProperty(Prop.Table);
                    if(Property is IDataBaseElement<IProperty> property && Property is IDataBaseProperties temp)
                    {
                        var props = property.Select($"{temp.IdName} = {Item.PropertyID}");
                        foreach(var prop in props)
                            items.Add(prop.Title);
                    }
                    Result[Prop.Table] = items;
                }
            }
            return Result;
        }

        public List<String> GetBuildings()
        {
            List<String> Result = new List<string>();
            List<Building> Buildings = Ext.Create<Buildings>(this.DB).Select();
            foreach(Building Build in Buildings)
                Result.Add(Build.Title);
            return Result;
        }

        // ------------------------------------ print --------------

        private void PrintSetComander(String PeopleName, String SubdivisionName)
        {
            if(SubdivisionName.Length > 0 && PeopleName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Назначение_командира.docx"))
                {
                    if(this.Word == null)
                        this.Word = new WordApp();
                    WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Назначение_командира.docx");
                    if(oDoc.Bookmarks.Exists("name"))
                        oDoc.Bookmarks["name"].Range.Text = PeopleName;
                    if(oDoc.Bookmarks.Exists("subdivision"))
                        oDoc.Bookmarks["subdivision"].Range.Text = SubdivisionName;
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
                        oDoc.SaveAs("Назначение командиром.docx");
                    }
                    oDoc.Close();
                } else
                    Ext.MessageBox("Шаблон Назначение_командира.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите подразделение!");
        }

        private void PrintSetUpper(String SubdivisionName, String UpperName)
        {
            if(SubdivisionName.Length > 0 && UpperName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Назначение_вышестоящей.docx"))
                {
                    if(this.Word == null)
                        this.Word = new WordApp();
                    WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Назначение_вышестоящей.docx");
                    if(oDoc.Bookmarks.Exists("basic"))
                        oDoc.Bookmarks["basic"].Range.Text = SubdivisionName;
                    if(oDoc.Bookmarks.Exists("subdivision"))
                        oDoc.Bookmarks["subdivision"].Range.Text = UpperName;
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
                        oDoc.SaveAs("Назначение вышестоящей.docx");
                    }
                    oDoc.Close();
                } else
                    Ext.MessageBox("Шаблон Назначение_вышестоящей.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите подразделение!");
        }

        private void PrintToBuilding(String SubdivisionName, String BuildingName)
        {
            if(SubdivisionName.Length > 0 && BuildingName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Переместить_в_здание.docx"))
                {
                    if(this.Word == null)
                        this.Word = new WordApp();
                    WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Переместить_в_здание.docx");
                    if(oDoc.Bookmarks.Exists("basic"))
                        oDoc.Bookmarks["basic"].Range.Text = SubdivisionName;
                    if(oDoc.Bookmarks.Exists("building"))
                        oDoc.Bookmarks["building"].Range.Text = BuildingName;
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
                        oDoc.SaveAs("Переместить в здание.docx");
                    }
                    oDoc.Close();
                } else
                    Ext.MessageBox("Шаблон Переместить_в_здание.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите подразделение!");
        }

        private void PrintToStudy(String SubdivisionName)
        {
            if(SubdivisionName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Учения.docx"))
                {
                    SharedForms.OwerWorkTemplate form = new SharedForms.OwerWorkTemplate();
                    if(form.ShowDialog() == true)
                    {
                        DateTime start = (form.StartDate != null) ? form.StartDate.Value : DateTime.Now;
                        DateTime end = (form.EndDate != null) ? form.EndDate.Value : DateTime.Now;
                        if(this.Word == null)
                            this.Word = new WordApp();
                        WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Учения.docx");
                        if(oDoc.Bookmarks.Exists("subdivision"))
                            oDoc.Bookmarks["subdivision"].Range.Text = SubdivisionName;
                        if(oDoc.Bookmarks.Exists("start_date"))
                            oDoc.Bookmarks["start_date"].Range.Text = start.ToString("dd.MM.yyyy");
                        if(oDoc.Bookmarks.Exists("end_date"))
                            oDoc.Bookmarks["end_date"].Range.Text = end.ToString("dd.MM.yyyy");
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
                            oDoc.SaveAs("Учения.docx");
                        }
                        oDoc.Close();
                    }
                } else
                    Ext.MessageBox("Шаблон Учения.dotx не найден в папке templates!");
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
            this.LoadSubdivisionsType();
        }

        // добавить подразделение
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Form = new SubdivisionForms.AddSubdivision(this.TypeBox.Items.ToStringList(), this.TypeBox.SelectedIndex);
            if (Form.ShowDialog() == true)
                this.AddSubdidision(Form.Type, Form.NewTitle);       
        }
        // назначить командира
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var Form = new SubdivisionForms.SetComander(this.Name_Value.Text, this.GetPeoplesInRanks());
            if(Form.ShowDialog() == true)
                this.AddComander(Form.PeopleName);
        }
        // назначить вышестоящую
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var Form = new SubdivisionForms.SetUpper(this.Name_Value.Text, this.GetValidUpper());
            if (Form.ShowDialog() == true)
                this.AddUpper(Form.Type, Form.Upper);
        }
        // переместить в здание
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var Form = new SubdivisionForms.ToBuilding(this.Name_Value.Text, this.GetBuildings());
            if(Form.ShowDialog() == true)
                this.ToBuilding(Form.Building);
        }
        // работа с имуществом
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var Form = new SubdivisionForms.PropertyWorks(this.Name_Value.Text, this.GetPeoperties(), this.GetSubdivisionPeoperties());
            if(Form.ShowDialog() == true)
                this.SetProperties(Form.LastProperties);
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadSubdivisions();
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadData();
        }

        // при нажатии на квадрат сообщений внизу
        private void InfoColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // если ошибка существует - вывести на экран
            if(this.LastError?.Length > 0)
                new SharedForms.MessageBox(this.LastError, "Текст последней ошибки БД").ShowDialog();
        }

        // сохранение изменений
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveSubdivision is IDataBaseSubdivisions subd && this.ActiveItem != null)
            {
                // создаём список на изменение
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    {subd.TitleName, this.Name_Value.Text},
                };
                // изменяем
                if (this.ActiveSubdivision.Update(Fields, $"{subd.IdName} = {this.ActiveItem.ID}"))
                {
                    // если всё хорошо - обновляем данные формы
                    isAccept = true;
                    this.LoadSubdivisions();
                }
                else
                    // если нет, то запоминаем последнюю ошибку БД
                    this.LastError = this.ActiveSubdivision.GetLastError();
            } else
                this.LastError = "Не выбрано подразделение!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }
        // удалить подразделение
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool? isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveSubdivision is IDataBaseSubdivisions subd && this.ActiveItem != null)
            {
                // выводим сообщение, действительно ли пользователь хочет удалить подразделение
                if(Ext.MessageBox("Вы действительно хотите удалить подразделение", "Внимание!", MessageBoxButton.YesNo) == true)
                {
                    // удаляем подразделение
                    if(this.ActiveSubdivision.Delete($"{subd.IdName} = {this.ActiveItem.ID}"))
                    {
                        // если всё хорошо - обновляем данные формы
                        isAccept = true;
                        this.LoadSubdivisions();
                    }
                } else
                    isAccept = null;
            } else
                this.LastError = "Не выбрано подразделение!";
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
        // Техника в подразделении
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
            {
                Dictionary<String, String> Data = new Dictionary<String, String>();
                Dictionary<String, int> TempData = new Dictionary<string, int>();
                var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
                var PropertyItems = Ext.Create<PropertiesInSubdivissions>(this.DB).Select($"{PropertiesInSubdivissions.SubdivisionID} = {this.ActiveItem.ID}");
                foreach(var PropertyItem in PropertyItems)
                {

                    if(PropertyItem.Subdivision == 1 || PropertyItem.Subdivision > 3 && PropertyItem.Subdivision < 6)
                    {
                        String Type = "";
                        switch(PropertyItem.Subdivision)
                        {
                            case 1:
                                Type = "Артиллерия";
                                break;
                            case 4:
                                Type = "Автомобили";
                                break;
                            case 5:
                                Type = "БМП";
                                break;
                            case 6:
                                Type = "Тягачи";
                                break;
                        }
                        var Prop = PropertyTable.SelectFirst($"{PropertiesTable.ID} = {PropertyItem.Property}");
                        if(Prop != null)
                        {
                            var PropItem = PropertyTable.GetProperty(Prop.Table);
                            if(PropItem is IDataBaseElement<IProperty> prop && PropItem is IDataBaseProperties temp)
                            {
                                var propData = prop.Select($"{temp.IdName} = {PropertyItem.PropertyID}");
                                if(propData.Count > 0)
                                {
                                    if(TempData.TryGetValue(Type, out _))
                                        TempData[Type] += propData.Count;
                                    else
                                        TempData.Add(Type, propData.Count);
                                }
                            }
                        }
                    }
                }
                foreach(var Item in TempData)
                    Data.Add(Item.Key, Item.Value.ToString());
                new ReportForm("Техника в подразделении", Data).ShowDialog();
            } else
                Ext.MessageBox("Выберите подразделение!");
        }
        // Имущество в подразделении
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
            {
                Dictionary<String, String> Data = new Dictionary<String, String>();
                Dictionary<String, int> TempData = new Dictionary<string, int>();
                var PropertyTable = Ext.Create<PropertiesTable>(this.DB);
                var PropertyItems = Ext.Create<PropertiesInSubdivissions>(this.DB).Select($"{PropertiesInSubdivissions.SubdivisionID} = {this.ActiveItem.ID}");
                foreach(var PropertyItem in PropertyItems)
                {
                    if(PropertyItem.Subdivision == 0 || PropertyItem.Subdivision > 1 && PropertyItem.Subdivision < 4)
                    {
                        String Type = "";
                        switch(PropertyItem.Subdivision)
                        {
                            case 0:
                                Type = "Автоматы";
                                break;
                            case 2:
                                Type = "Карабины";
                                break;
                            case 3:
                                Type = "Ракетное оружие";
                                break;
                        }
                        var Prop = PropertyTable.SelectFirst($"{PropertiesTable.ID} = {PropertyItem.Property}");
                        if(Prop != null)
                        {
                            var PropItem = PropertyTable.GetProperty(Prop.Table);
                            if(PropItem is IDataBaseElement<IProperty> prop && PropItem is IDataBaseProperties temp)
                            {
                                var propData = prop.Select($"{temp.IdName} = {PropertyItem.PropertyID}");
                                if(propData.Count > 0)
                                {
                                    if(TempData.TryGetValue(Type, out _))
                                        TempData[Type] += propData.Count;
                                    else
                                        TempData.Add(Type, propData.Count);
                                }
                            }
                        }
                    }
                }
                foreach(var Item in TempData)
                    Data.Add(Item.Key, Item.Value.ToString());
                new ReportForm("Имущество в подразделении", Data).ShowDialog();
            } else
                Ext.MessageBox("Выберите подразделение!");
        }
        // Люди в подразделении
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
            {
                var RanksTable = Ext.Create<Ranks>(this.DB);
                Dictionary<String, String> Data = new Dictionary<String, String>();

                Subdivision Subd = Ext.Create<Subdivisions>(this.DB).SelectFirst($"{Subdivisions.Table} Like '{this.ActiveSubdivision.GetTableName()}'");
                if(Subd != null)
                {
                    List<People> PeoplesItems = Ext.Create<Peoples>(this.DB).Select($"{Peoples.Subdivision} = {Subd.ID} and {Peoples.SubdivisionID} = {this.ActiveItem.ID}");
                    foreach(People PeopleItem in PeoplesItems)
                    {
                        var Rank = RanksTable.SelectFirst($"{Ranks.ID} = {PeopleItem.Rank}");
                        Data.Add(PeopleItem.Name, Rank.Title);
                    }
                    new ReportForm("Люди в подразделении", Data).ShowDialog();
                }
            } 
            else
                Ext.MessageBox("Выберите подразделение!");
        }
        // шаблон подразделения на учения
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
                this.PrintToStudy(this.ActiveItem.Title);
            else
                Ext.MessageBox("Выберите подразделение!");
        }
    }
}
