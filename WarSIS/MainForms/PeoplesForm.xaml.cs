using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WarSISDataBase.DataBase;

using WarSISModelsDB;
using WarSISModelsDB.Models;
using WarSISModelsDB.Models.Data;
using WarSISModelsDB.Models.DataBase;
using WarSISModelsDB.Models.DataBase.Rank;
using WarSISModelsDB.Models.DataBase.Subdivision;

using WordApp = Microsoft.Office.Interop.Word.Application;
using WordDoc = Microsoft.Office.Interop.Word.Document;

namespace WarSIS.MainForms
{
    /// <summary>
    /// Interaction logic for Peoples.xaml
    /// </summary>
    public partial class PeoplesForm : Window
    {
        MSSQLEngine DB = null;
        String LastError = "";

        IDataBaseElement ActiveRank = null;
        People ActiveItem = null;

        BitmapImage NewPhoto = null;
        BitmapImage BlankPhoto = null;

        String RankNameData = "";

        WordApp Word = null;

        public PeoplesForm(MSSQLEngine DataBase)
        {
            this.InitializeComponent();
            this.DB = DataBase;
            this.BlankPhoto = new BitmapImage();
            this.BlankPhoto.BeginInit();
            this.BlankPhoto.UriSource = new Uri(Environment.CurrentDirectory + "\\images\\profile.png", UriKind.RelativeOrAbsolute);
            this.BlankPhoto.EndInit();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadRanks();
            this.LoadStates();
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
            this.RankName.Text = "";
            this.RankNameData = "";
            this.State_Value.SelectedIndex = 0;
            this.SubdivisionID.Text = "";
            this.SubdivisionName.Text = "";
            this.SubdivisionRank.Text = "";
            this.SpecialitiesList.Children.Clear();
            this.NewPhoto = null;
            this.DataTitle.Text = "Дата:";
            this.DataName.Text = "";
            this.State_Value.SelectedIndex = -1;
        }

        // ------------------------------------ load -----------

        // WindowLoaded
        /// <summary>
        /// Загрузка званий
        /// </summary>
        private void LoadRanks()
        {
            this.RankBox.Items.Add("Все");
            this.RankBox.Items.Add("Без звания");
            // создаём экземпляр класса "Звания"
            Ranks Table = Ext.Create<Ranks>(this.DB);
            // получаем все элементы и добавляем их в выпадающий список
            foreach(Rank Item in Table.Select())
                this.RankBox.Items.Add(Item.Table);
            // если элементы есть, то выбираем первый элемент
            if(this.RankBox.Items.Count > 0)
                this.RankBox.SelectedIndex = 0;
            else
                // иначе выдаём сообщение
                Ext.MessageBox("Типы имущества не были загружены!");
        }

        // WindowLoaded
        /// <summary>
        /// Загрузка состояний
        /// </summary>
        private void LoadStates()
        {
            // создаём экземпляр класса "Состояния"
            States Table = Ext.Create<States>(this.DB);
            // получаем все элементы и добавляем их в выпадающий список
            foreach(State Item in Table.Select())
                this.State_Value.Items.Add(Item.Title);
        }

        // TypeBoxChanged
        /// <summary>
        /// Загрузка списка людей
        /// </summary>
        private void LoadPeoples()
        {
            // получаем активный тип имущества
            string Text = this.RankBox.SelectedItem.ToString();
            string PeoplesWhere = "";
            if(MainWindow.Instance.PeopleRole > -1)
            {
                People People = Ext.Create<Peoples>(this.DB).SelectFirst($"{Peoples.ID} = {MainWindow.Instance.PeopleID}");
                if(People != null)
                {
                    PeoplesWhere = $"{Peoples.Subdivision} = {People.Subdivision} and {Peoples.SubdivisionID} = {People.SubdivisionID} and {Peoples.ID} != {People.ID}";
                    this.Title = $"Люди вашего подразделения, {People.Name}";
                }
            }
                
            if(Text.Length > 0)
            {
                this.ActiveRank = null;
                // очищаем список подразделений
                this.ListBox.Items.Clear();
                if(Text.ToUpper().CompareTo("ВСЕ") == 0)
                {
                    List<People> Items = Ext.Create<Peoples>(this.DB).Select(PeoplesWhere);
                    foreach(People Item in Items)
                        this.ListBox.Items.Add(Item.Name);
                } else if(Text.ToUpper().CompareTo("Без звания".ToUpper()) == 0)
                {
                    PeoplesWhere = "and " + PeoplesWhere;
                    List<People> Items = Ext.Create<Peoples>(this.DB).Select($"{Peoples.Rank} = -1 or {Peoples.Rank} IS NULL {PeoplesWhere}");
                    foreach(People Item in Items)
                        this.ListBox.Items.Add(Item.Name);
                } else
                {
                    PeoplesWhere = "and " + PeoplesWhere;
                    // создаём экземпляр класса активного звания
                    // и запоминаем его
                    this.ActiveRank = Ext.Create<Ranks>(this.DB).GetRank(Text);
                    // если данный экземпляр реализует интерфейс 
                    if(this.ActiveRank is IDataBaseElement<IRank> Table)
                    {
                        // очищаем список людей
                        this.ListBox.Items.Clear();
                        // получаем все элементы и заполняем список людей
                        foreach(IRank Item in Table.Select())
                        {
                            People People = Ext.Create<Peoples>(this.DB).SelectFirst($"{Peoples.ID} = {Item.People} {PeoplesWhere}");
                            if(People != null)
                                this.ListBox.Items.Add(People.Name);
                        }
                    } else
                        // если нет - выводим сообщение об ошибке
                        Ext.MessageBox("Люди не были загружены!");
                }
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
            // получаем название выбранного человека
            string Pipl = this.ListBox.SelectedItem?.ToString();
            // если название выбрано и существует класс активного человека
            if(Pipl?.Length > 0)
            {
                People People = Ext.Create<Peoples>(this.DB).SelectFirst($"{Peoples.Name} Like '{Pipl}'");
                if(People != null)
                {
                    // запоминаем полученные данные
                    this.ActiveItem = People;

                    this.ID_Value.Text = this.ActiveItem.ID.ToString();
                    this.Name_Value.Text = this.ActiveItem.Name;
                    Ranks RanksTable = Ext.Create<Ranks>(this.DB);
                    Rank rank = RanksTable.SelectFirst($"{Ranks.ID} = {this.ActiveItem.Rank}");
                    if(rank != null)
                    {
                        this.RankName.Text = rank.Title;
                        this.RankNameData = rank.Table;
                        IDataBaseElement Rank = RanksTable.GetRank(rank.Table);
                        if(Rank is IDataBaseElement<IRank> rankk && Rank is IDataBaseRanks temp)
                        {
                            List<IRank> Items = rankk.Select($"{temp.PeopleName} = {this.ActiveItem.ID}");
                            if(Items.Count > 0)
                                this.DataTitle.Text = temp.DateName + ":";
                                this.DataName.Text = Items[0].Date.ToString();
                        }
                    }
                        
                    Subdivisions SudbTable = Ext.Create<Subdivisions>(this.DB);
                    Subdivision subdiv = SudbTable.SelectFirst($"{Subdivisions.ID} = {this.ActiveItem.Subdivision}");
                    if(subdiv != null)
                    {
                        IDataBaseElement subd = SudbTable.GetSubdivision(subdiv.Table);
                        if(subd is IDataBaseElement<ISubdivision> temp &&
                            subd is IDataBaseSubdivisions SUBD)
                        {
                            List<ISubdivision> Items = temp.Select($"{SUBD.IdName} = {this.ActiveItem.SubdivisionID}");
                            if(Items.Count > 0)
                            {
                                this.SubdivisionID.Text = Items[0].ID.ToString();
                                this.SubdivisionName.Text = Items[0].Title;
                                this.SubdivisionRank.Text = (Items[0].Commander == this.ActiveItem.ID) ? "Командир" : "Служащий";
                            }
                        }
                    }

                    OwnedSpecialities SpecialtiesOnPeople = Ext.Create<OwnedSpecialities>(this.DB);
                    Specialities SpecialtiesTable = Ext.Create<Specialities>(this.DB);
                    List<OwnedSpeciality> SpecialtyItems = SpecialtiesOnPeople.Select($"{OwnedSpecialities.People} = {this.ActiveItem.ID}");
                    foreach(OwnedSpeciality Speciality in SpecialtyItems)
                    {
                        Speciality Specialty = SpecialtiesTable.SelectFirst($"{Specialities.ID} = {Speciality.Speciality}");
                        if(Specialty != null)
                        {
                            this.SpecialitiesList.Children.Add(new TextBlock() { 
                                Text = Specialty.Title,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = new Thickness(10, 0, 10, 0),
                                Height = 25
                            });
                        }
                    }

                    State State = Ext.Create<States>(this.DB).SelectFirst($"{States.ID} = {this.ActiveItem.State}");
                    if(State != null)
                        this.State_Value.SelectedItem = State.Title;

                    // load photo

                    byte[] photo = this.ActiveItem.Photo as byte[];
                    this.NewPhoto = (photo != null) ? Ext.BytesToImage(photo) : this.BlankPhoto;
                    this.PhotoBox.Source = this.NewPhoto;

                } else
                    Ext.MessageBox("Человек не был загружен!");
            } 
        }

        // ------------------------------------ add ------------
        /// <summary>
        /// Добавление нового человека
        /// </summary>
        /// <param name="Name">Имя человека</param>
        private void AddPeople(String Name)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(Name.Length > 0)
            {
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    { Peoples.ID, Ext.Create<Peoples>(this.DB).Max(Peoples.ID) + 1 },
                    { Peoples.Name, Name }
                };
                if(Ext.Create<Peoples>(this.DB).Insert(Fields))
                {
                    // если было добавлено - обновляем данные формы
                    this.LoadPeoples();
                    isAccept = true;
                } else
                    this.LastError = Peoples.LastError;
            } else
                this.LastError = "Имя не было введено!";
            // выводим результат работы операции пользователю
            this.ViewResult(isAccept);
        }

        /// <summary>
        /// Присвоение звания
        /// </summary>
        /// <param name="RankName">Звание</param>
        /// <param name="FieldValues">Список дополнительных полей</param>
        private void AddRank(String RankName, List<String> FieldValues)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если название было введено
            if(RankName.Length > 0)
            {
                if(this.ActiveItem != null)
                {
                    if(RankName.CompareTo(this.RankNameData) != 0)
                    {
                        Ranks RanksTable = Ext.Create<Ranks>(this.DB);
                        // удаляем человека из его предыдущего звания
                        if(this.RankNameData?.Length > 0)
                        {
                            IDataBaseElement PeopleRank = RanksTable.GetRank(this.RankNameData);
                            if(PeopleRank is IDataBaseRanks piplRank)
                            {
                                Dictionary<string, object> Fields = new Dictionary<string, object>()
                                {
                                    { Peoples.Rank, null }
                                };
                                if(!PeopleRank.Delete($"{piplRank.PeopleName} = {this.ActiveItem.ID}"))
                                    this.LastError = PeopleRank.GetLastError();
                                if(!Ext.Create<Peoples>(this.DB).Update(Fields, $"{Peoples.ID} = {this.ActiveItem.ID}"))
                                    this.LastError = Peoples.LastError;
                            }
                        }
                        // проверяем чтобы новое звание небыло "Без звания"
                        if(RankName.ToUpper().CompareTo("Без звания".ToUpper()) != 0)
                        {
                            // создаём сласс выбранного звания
                            IDataBaseElement Rank = RanksTable.GetRank(RankName);
                            int RankID = -1;
                            Rank tmp = RanksTable.SelectFirst($"{Ranks.Table} Like '{RankName}'");
                            if(tmp != null)
                                RankID = tmp.ID;
                            // проверяем реализует ли он необходимые интерфейсы
                            if(Rank is IDataBaseRanks temp)
                            {
                                // инициализируем данные для вставки
                                Dictionary<string, object> Fields = new Dictionary<string, object>()
                                {
                                    { temp.PeopleName, this.ActiveItem.ID },
                                    { temp.DateName, DateTime.Now }
                                };
                                // вставляем
                                if(Rank.Insert(Fields))
                                {
                                    Fields.Clear();
                                    if(RankID > -1)
                                        Fields.Add(Peoples.Rank, RankID);
                                    else
                                        Fields.Add(Peoples.Rank, null);
                                    Ext.Create<Peoples>(this.DB).Update(Fields, $"{Peoples.ID} = {this.ActiveItem.ID}");
                                    isAccept = true;
                                } else
                                    // если плохо - запоминаем ошибку БД
                                    this.LastError = Rank.GetLastError();
                            } else
                                this.LastError = "Ошибка GetRank! Звание не было найдено!";
                        } else
                            isAccept = true;
                    } else
                        isAccept = true;
                } else
                    this.LastError = "Человек не выбран!";
            } else
                this.LastError = "Звание не было выбрано!";
            if(isAccept)
            {
                if(this.ActiveItem != null)
                {
                    Rank Rank = Ext.Create<Ranks>(this.DB).SelectFirst($"{Ranks.Table} Like '{RankName}'");
                    this.PrintNewRank(this.ActiveItem.Name, (Rank != null) ? Rank.Title : RankName);
                }
                // если всё хорошо, то обновляем данные формы
                this.LoadPeoples();
            }
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void AddToSubdivision(String SubdType, String SubdName, Boolean IsComander)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            if(this.ActiveItem != null)
            {
                Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
                Dictionary<string, object> Fields = new Dictionary<string, object>();
                if(SubdName.CompareTo(this.SubdivisionName.Text) != 0)
                {
                    // удаляем командира подразделения, если человек в подразделении был командиром
                    if(this.SubdivisionName.Text?.Length > 0)
                    {
                        Subdivision tmp = SubdivisionsTable.SelectFirst($"{Subdivisions.ID} = {this.ActiveItem.Subdivision}");
                        if(tmp != null)
                        {
                            IDataBaseElement PeopleSubd = SubdivisionsTable.GetSubdivision(tmp.Table);
                            if(PeopleSubd is IDataBaseSubdivisions piplSubd)
                            {
                                Fields.Clear();
                                Fields.Add(piplSubd.CommanderName, null);
                                if(!PeopleSubd.Update(Fields, $"{piplSubd.IdName} = {this.ActiveItem.SubdivisionID} and {piplSubd.CommanderName} = {this.ActiveItem.ID}"))
                                    this.LastError = Peoples.LastError;
                            }
                        }
                    }
                    Subdivision Subd = SubdivisionsTable.SelectFirst($"{Subdivisions.Table} Like '{SubdType}'");
                    IDataBaseElement subd = SubdivisionsTable.GetSubdivision(SubdType);
                    if(Subd != null && subd is IDataBaseElement<ISubdivision> sudbDB &&
                        subd is IDataBaseSubdivisions temp)
                    {
                        List<ISubdivision> items = sudbDB.Select($"{temp.TitleName} Like '{SubdName}'");
                        if(items.Count > 0)
                        {
                            Fields.Clear();
                            Fields.Add(Peoples.Subdivision, Subd.ID);
                            Fields.Add(Peoples.SubdivisionID, items[0].ID);
                            if(Ext.Create<Peoples>(this.DB).Update(Fields, $"{Peoples.ID} = {this.ActiveItem.ID}"))
                                isAccept = true;
                            else
                                this.LastError = Peoples.LastError;
                        } else
                            this.LastError = "Подразделение не найдено!";
                    } else
                        this.LastError = "Тип подразделения не найден!";
                }
                IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(SubdType);
                if(Subdivision is IDataBaseSubdivisions ThisSubd)
                {
                    Fields.Clear();
                    if(IsComander)
                        Fields.Add(ThisSubd.CommanderName, this.ActiveItem.ID);
                    if(Subdivision.Update(Fields, $"{ThisSubd.TitleName} Like '{SubdName}'"))
                        isAccept = true;
                    else
                        this.LastError = Peoples.LastError;
                }
            } else
                this.LastError = "Человек не выбран!";
            if(isAccept)
            {
                if(this.ActiveItem != null)
                {
                    this.PrintNewSubdivision(this.ActiveItem.Name, SubdName);
                    if(IsComander)
                        this.PrintSetComander(this.ActiveItem.Name, SubdName);
                }
                // если всё хорошо, то обновляем данные формы
                this.LoadPeoples();
            }
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        private void SetSpecialties(List<String> PeopleSpecialties)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            if(this.ActiveItem != null)
            {
                int sch = 0;
                OwnedSpecialities SpecialtiesOnPeople = Ext.Create<OwnedSpecialities>(this.DB);
                SpecialtiesOnPeople.Delete($"{ OwnedSpecialities.People} = {this.ActiveItem.ID}");
                Specialities SpecialtiesTable = Ext.Create<Specialities>(this.DB);
                foreach(string Item in PeopleSpecialties)
                {
                    Speciality Specialty = SpecialtiesTable.SelectFirst($"{Specialities.Title} Like '{Item}'");
                    if(Specialty != null)
                    {
                        Dictionary<string, object> Fields = new Dictionary<string, object>()
                        {
                            { OwnedSpecialities.ID, SpecialtiesOnPeople.Max(OwnedSpecialities.ID) + 1 },
                            { OwnedSpecialities.Speciality, Specialty.ID },
                            { OwnedSpecialities.People, this.ActiveItem.ID },
                        };
                        if(SpecialtiesOnPeople.Insert(Fields))
                            sch++;
                        else
                            this.LastError = SpecialtiesOnPeople.GetLastError();
                    }
                }
                isAccept = (sch == PeopleSpecialties.Count);
            } else
                this.LastError = "Не выбран человек!";
            if(isAccept)
                // если всё хорошо, то обновляем данные формы
                this.LoadPeoples();
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        // ------------------------------------ get ------------

        private Dictionary<String, List<String>> GetRankFields()
        {
            Dictionary<String, List<String>> Result = new Dictionary<string, List<string>>();

            // fields

            return Result;
        }

        /// <summary>
        /// Генерация списка подходящих для таблицы "Изменить вышестоящую"
        /// </summary>
        /// <returns></returns>
        private Dictionary<String, List<String>> GetSubdivisions()
        {
            Dictionary<String, List<String>> Result = new Dictionary<String, List<String>>();
            // создаём экземпляр класса "Подразделения"
            Subdivisions SubdivisionsTable = Ext.Create<Subdivisions>(this.DB);
            // получаем все подразделения
            List<Subdivision> items = SubdivisionsTable.Select();
            // для каждого подразделения
            foreach(Subdivision Item in items)
            {
                List<string> values = new List<string>();
                // получаем класс подразделения
                IDataBaseElement Subdivision = SubdivisionsTable.GetSubdivision(Item.Table);
                // если класс реализует интерфейс подрезделений
                if(Subdivision is IDataBaseElement<ISubdivision> Table)
                {
                    // заполняем список подразделений
                    foreach(ISubdivision subd in Table.Select())
                        values.Add(subd.Title);
                }
                Result.Add(Item.Table, values);
            }
            return Result;
        }

        private List<String> GetSpecialties()
        {
            List<String> Result = new List<string>();
            List<Speciality> Items = Ext.Create<Specialities>(this.DB).Select();
            foreach(Speciality Item in Items)
                Result.Add(Item.Title);
            return Result;
        }

        private List<String> GetPeopleSpecialties()
        {
            List<String> Result = new List<string>();
            if(this.ActiveItem != null)
            {
                List<OwnedSpeciality> Items = Ext.Create<OwnedSpecialities>(this.DB).Select($"{OwnedSpecialities.People} = {this.ActiveItem.ID}");
                Specialities Table = Ext.Create<Specialities>(this.DB);
                foreach(OwnedSpeciality Item in Items)
                {
                    Speciality Specialty = Table.SelectFirst($"{Specialities.ID} = {Item.Speciality}");
                    if(Specialty != null)
                        Result.Add(Specialty.Title);
                }
            }
            return Result;
        }

        // ------------------------------------ print ------------

        private void PrintNewRank(String PeopleName, String RoleName)
        {
            if(RoleName.Length > 0 && PeopleName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Звания.docx"))
                {
                    if(this.Word == null)
                        this.Word = new WordApp();
                    WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Звания.docx");
                    if(oDoc.Bookmarks.Exists("name"))
                        oDoc.Bookmarks["name"].Range.Text = PeopleName;
                    if(oDoc.Bookmarks.Exists("role"))
                        oDoc.Bookmarks["role"].Range.Text = RoleName;
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
                        oDoc.SaveAs("Присвоение звания.docx");
                    }
                    oDoc.Close();
                } else
                    Ext.MessageBox("Шаблон Звания.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите сотрудника!");
        }

        private void PrintNewSubdivision(String PeopleName, String SubdivisionName)
        {
            if(SubdivisionName.Length > 0 && PeopleName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Перевод_в_подразделение.docx"))
                {
                    if(this.Word == null)
                        this.Word = new WordApp();
                    WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Перевод_в_подразделение.docx");
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
                        oDoc.SaveAs("Перевод в подразделение.docx");
                    }
                    oDoc.Close();
                } else
                    Ext.MessageBox("Шаблон Перевод_в_подразделение.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите сотрудника!");
        }

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
                Ext.MessageBox("Выберите сотрудника!");
        }

        private void PrintSverh(String PeopleName, String Role)
        {
            if(PeopleName.Length > 0)
            {
                if(System.IO.File.Exists("templates\\Сверхурочные.docx"))
                {
                    SharedForms.OwerWorkTemplate form = new SharedForms.OwerWorkTemplate();
                    if(form.ShowDialog() == true)
                    {
                        DateTime start = (form.StartDate != null) ? form.StartDate.Value : DateTime.Now;
                        DateTime end = (form.EndDate != null) ? form.EndDate.Value : DateTime.Now;
                        DateTime now = DateTime.Now;
                        if(this.Word == null)
                            this.Word = new WordApp();
                        WordDoc oDoc = this.Word.Documents.Add(Environment.CurrentDirectory + "\\templates\\Сверхурочные.docx");
                        if(oDoc.Bookmarks.Exists("name"))
                            oDoc.Bookmarks["name"].Range.Text = PeopleName;
                        if(oDoc.Bookmarks.Exists("role"))
                            oDoc.Bookmarks["role"].Range.Text = Role;
                        if(oDoc.Bookmarks.Exists("s_day"))
                            oDoc.Bookmarks["s_day"].Range.Text = start.Day.ToString();
                        if(oDoc.Bookmarks.Exists("s_month"))
                            oDoc.Bookmarks["s_month"].Range.Text = start.Month.ToString();
                        if(oDoc.Bookmarks.Exists("s_year"))
                            oDoc.Bookmarks["s_year"].Range.Text = start.Year.ToString();
                        if(oDoc.Bookmarks.Exists("e_day"))
                            oDoc.Bookmarks["e_day"].Range.Text = end.Day.ToString();
                        if(oDoc.Bookmarks.Exists("e_month"))
                            oDoc.Bookmarks["e_month"].Range.Text = end.Month.ToString();
                        if(oDoc.Bookmarks.Exists("e_year"))
                            oDoc.Bookmarks["e_year"].Range.Text = end.Year.ToString();
                        if(oDoc.Bookmarks.Exists("n_day"))
                            oDoc.Bookmarks["n_day"].Range.Text = now.Day.ToString();
                        if(oDoc.Bookmarks.Exists("n_month"))
                            oDoc.Bookmarks["n_month"].Range.Text = now.Month.ToString();
                        if(oDoc.Bookmarks.Exists("n_year"))
                            oDoc.Bookmarks["n_year"].Range.Text = now.Year.ToString();
                        if(oDoc.Bookmarks.Exists("short_name"))
                        {
                            string first = PeopleName.Substring(0, PeopleName.IndexOf(" ")).Replace(" ", "");
                            string last2 = PeopleName.Substring(PeopleName.LastIndexOf(" ")).Replace(" ", "");
                            string last = PeopleName.Substring(first.Length, PeopleName.Length - first.Length - last2.Length).Replace(" ", "");
                            oDoc.Bookmarks["short_name"].Range.Text = $"{first} {last[0]}.{last2[0]}.";
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
                            oDoc.SaveAs("Новый шаблон.docx");
                        }
                        oDoc.Close();
                    }
                } else
                    Ext.MessageBox("Шаблон Сверхурочные.dotx не найден в папке templates!");
            } else
                Ext.MessageBox("Выберите сотрудника!");
        }

        #endregion

        // Добавить человека
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Form = new PeopleForms.AddPeople();
            if(Form.ShowDialog() == true)
                this.AddPeople(Form.PeopleName);
        }
        // Присвоить звание
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var Form = new PeopleForms.AddRank(this.Name_Value.Text, this.RankBox.Items.ToStringList(), this.GetRankFields());
            if(Form.ShowDialog() == true)
                this.AddRank(Form.RankName, Form.FieldValues);
        }
        // Перевести в подразделение
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var Form = new PeopleForms.AddSubdivision(this.Name_Value.Text, this.GetSubdivisions());
            if(Form.ShowDialog() == true)
                this.AddToSubdivision(Form.SType, Form.SName, Form.IsComander);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var Form = new PeopleForms.AddSpecialties(this.Name_Value.Text, this.GetSpecialties(), this.GetPeopleSpecialties());
            if(Form.ShowDialog() == true)
                this.SetSpecialties(Form.LastSpecialties);
        }

        private void RankBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadPeoples();
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
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveItem != null)
            {

                // создаём список на изменение
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    {Peoples.Name, this.Name_Value.Text},
                };
                if(this.NewPhoto != null)
                    Fields.Add(Peoples.Photo, Ext.ImageToBytes(this.NewPhoto));
                State State = Ext.Create<States>(this.DB).SelectFirst($"{States.Title} Like '{this.State_Value.SelectedItem}'");
                if(State != null)
                    Fields.Add(Peoples.State, State.ID);
                // изменяем
                if(Ext.Create<Peoples>(this.DB).Update(Fields, $"{Peoples.ID} = {this.ActiveItem.ID}"))
                {
                    // если всё хорошо - обновляем данные формы
                    isAccept = true;
                    this.LoadPeoples();
                } else
                    // если нет, то запоминаем последнюю ошибку БД
                    this.LastError = Peoples.LastError;
            } else
                this.LastError = "Не выбрано имущество!";
            // выводим результат работы операции на пользователю
            this.ViewResult(isAccept);
        }

        // удалить человека
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // переменная для для ошибок
            bool? isAccept = false;
            this.LastError = "";
            // если класс активного подразделения реализует интерфейс подразделений
            // и данные подразделения существуют (выбрано конкретное подразделение)
            if(this.ActiveItem != null)
            {
                // выводим сообщение, действительно ли пользователь хочет удалить подразделение
                if(Ext.MessageBox("Вы действительно хотите удалить человека", "Внимание!", MessageBoxButton.YesNo) == true)
                {
                    Peoples PeoplesTable = Ext.Create<Peoples>(this.DB);
                    People People = PeoplesTable.SelectFirst($"{Peoples.ID} = {this.ActiveItem.ID}");
                    // удаляем подразделение
                    if(PeoplesTable.Delete($"{Peoples.ID} = {this.ActiveItem.ID}"))
                    {
                        Dictionary<string, object> Fields = new Dictionary<string, object>();
                        // если всё хорошо - обновляем данные формы
                        isAccept = true;
                        Subdivisions SubdivisionTable = Ext.Create<Subdivisions>(this.DB);
                        Subdivision Subdivision = SubdivisionTable.SelectFirst($"{Subdivisions.ID} = {People.Subdivision}");
                        if(Subdivision != null)
                        {
                            IDataBaseElement Subdiv = SubdivisionTable.GetSubdivision(Subdivision.Table);
                            if(Subdiv is IDataBaseSubdivisions temp)
                            {
                                Fields.Clear();
                                Fields.Add(temp.CommanderName, null);
                                if(!Subdiv.Update(Fields, $"{temp.CommanderName} = {People.ID}"))
                                    this.LastError = Subdiv.GetLastError();
                            }
                        }

                        Ranks RanksTable = Ext.Create<Ranks>(this.DB);
                        Rank Rank = RanksTable.SelectFirst($"{Ranks.ID} = {People.Rank}");
                        if(Rank != null)
                        {
                            IDataBaseElement rank = RanksTable.GetRank(Rank.Table);
                            if(rank is IDataBaseRanks temp)
                            {
                                if(!rank.Delete($"{temp.PeopleName} = {People.ID}"))
                                    this.LastError = rank.GetLastError();
                            }
                        }
                        this.LoadPeoples();
                    } else
                        // если нет, то запоминаем последнюю ошибку БД
                        this.LastError = Peoples.LastError;
                } else
                    isAccept = null;
            } else
                this.LastError = "Не выбран человек!";
            // выводим результат работы операции на пользователю
            if(isAccept != null)
                this.ViewResult(isAccept.Value);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Фотографии (*.jpg)|*.jpg"
            };
            if(openFileDialog.ShowDialog() == true)
            {
                this.NewPhoto = new BitmapImage();
                this.NewPhoto.BeginInit();
                this.NewPhoto.UriSource = new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute);
                this.NewPhoto.EndInit();
                this.PhotoBox.Source = this.NewPhoto;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
            {
                Rank Rank = Ext.Create<Ranks>(this.DB).SelectFirst($"{Ranks.ID} = {this.ActiveItem.Rank}");
                this.PrintSverh(this.ActiveItem.Name, (Rank != null) ? Rank.Title : "Без звания");
            } 
            else 
                Ext.MessageBox("Выберите человека!");
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem owner = (MenuItem)sender;
            Popup child = (Popup)owner.Template.FindName("PART_Popup", owner);
            child.Placement = PlacementMode.Left;
            child.HorizontalOffset = -owner.ActualWidth;
            child.VerticalOffset = owner.ActualHeight;
        }

        // Командиры подразделений
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> Data = new Dictionary<String, String>();
            Peoples PeopleTable = Ext.Create<Peoples>(this.DB);
            Subdivisions SubdTable = Ext.Create<Subdivisions>(this.DB);
            List<Subdivision> SubdItems = SubdTable.Select();
            foreach(Subdivision SubdItem in SubdItems)
            {
                IDataBaseElement subd = SubdTable.GetSubdivision(SubdItem.Table);
                if(subd is IDataBaseElement<ISubdivision> temp && subd is IDataBaseSubdivisions tabl)
                {
                    List<ISubdivision> Subds = temp.Select($"{tabl.CommanderName} is NOT NULL");
                    if(Subds.Count > 0)
                    {
                        People People = PeopleTable.SelectFirst($"{Peoples.ID} = {Subds[0].Commander}");
                        if(People != null)
                        {
                            Data.Add(Subds[0].Title, People.Name);
                        }
                    }
                }
            }
            new ReportForm("Командиры подразделений", Data).ShowDialog();
        }
        // Цепочка подчинённости
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if(this.ActiveItem != null)
            {
                Dictionary<String, String> Data = new Dictionary<String, String>();

                Subdivisions SubdTable = Ext.Create<Subdivisions>(this.DB);
                Peoples PeopleTable = Ext.Create<Peoples>(this.DB);
                Ranks RanksTable = Ext.Create<Ranks>(this.DB);

                People People = PeopleTable.SelectFirst($"{Peoples.ID} = {this.ActiveItem.ID}");
                Rank Rank = RanksTable.SelectFirst($"{Ranks.ID} = {People.Rank}");
                if(Rank != null)
                    Data.Add($"{People.Name}", Rank.Title);
                else
                    Data.Add($"{People.Name}", "Без звания");
                Subdivision SubdivItem = SubdTable.SelectFirst($"{Subdivisions.ID} = {People.Subdivision}");
                if(SubdivItem != null)
                {
                    IDataBaseElement Subdiv = SubdTable.GetSubdivision(SubdivItem.Table);
                    while(Subdiv != null)
                    {
                        if(Subdiv is IDataBaseElement<ISubdivision> Subd && Subdiv is IDataBaseSubdivisions temp)
                        {
                            List<ISubdivision> subd = Subd.Select($"{temp.IdName} = {People.SubdivisionID}");
                            if(subd.Count > 0)
                            {
                                if(People.ID != subd[0].Commander)
                                {
                                    People = PeopleTable.SelectFirst($"{Peoples.ID} = {subd[0].Commander}");
                                    if(People != null)
                                        Data.Add($"↑ {People.Name}", Rank.Title);
                                }
                                Subdivision TempSubdivItem = SubdTable.SelectFirst($"{Subdivisions.ID} = {subd[0].Subdivision}");
                                Subdiv = (TempSubdivItem != null && SubdivItem.ID != TempSubdivItem.ID) ? SubdTable.GetSubdivision(TempSubdivItem.Table) : null;
                                SubdivItem = TempSubdivItem;
                            } 
                            else
                                Subdiv = null;
                        }
                    }
                    new ReportForm("Цепочка подчинённости", Data).ShowDialog();
                } else
                    Ext.MessageBox("Человек не находится в подразделении!");
            } else
                Ext.MessageBox("Выберите человека!");
        }
        // Отсутствующие люди
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Dictionary<String, Dictionary<String, String>> Data = new Dictionary<String, Dictionary<String, String>>();
            List<People> PeopleItems = Ext.Create<Peoples>(this.DB).Select($"{Peoples.State} != 0");
            States StatesTable = Ext.Create<States>(this.DB);
            var StatesData = StatesTable.Select($"{States.ID} > 0");
            foreach(var StateData in StatesData)
                Data.Add(StateData.Title, new Dictionary<string, string>());
            foreach(People PeopleItem in PeopleItems)
            {
                State State = StatesTable.SelectFirst($"{States.ID} = {PeopleItem.State}");
                if(State != null && Data.TryGetValue(State.Title, out Dictionary<string, string> Array))
                    Array.Add(PeopleItem.Name, State.Title);
            }
            new ReportForm("Отсутствующие люди", Data).ShowDialog();
        }
        // Люди по званиям
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Dictionary<String, Dictionary<String, String>> Data = new Dictionary<String, Dictionary<String, String>>();
            List<People> PeopleItems = Ext.Create<Peoples>(this.DB).Select($"{Peoples.State} != 0");
            Ranks RanksTable = Ext.Create<Ranks>(this.DB);
            var RanksData = RanksTable.Select();
            foreach(var RankData in RanksData)
                Data.Add(RankData.Table, new Dictionary<string, string>());
            foreach(People PeopleItem in PeopleItems)
            {
                Rank Rank = RanksTable.SelectFirst($"{Ranks.ID} = {PeopleItem.Rank}");
                if(Rank != null && Data.TryGetValue(Rank.Table, out Dictionary<string, string> Array))
                    Array.Add(PeopleItem.Name, Rank.Title);
            }
            new ReportForm("Люди по званиям", Data).ShowDialog();
        }
    }
}
