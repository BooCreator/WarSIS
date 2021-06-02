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

using WarSISDataBase.DataBase;

using WarSISModelsDB.Models.DataBase;

namespace WarSIS.AdminForms
{
    /// <summary>
    /// Interaction logic for UsersForm.xaml
    /// </summary>
    public partial class UsersForm : Window
    {
        MSSQLEngine DB = null;
        Users UsersTable = null;
        public UsersForm(MSSQLEngine DataBase)
        {
            this.InitializeComponent();
            this.DB = DataBase;
            this.UsersTable = Ext.Create<Users>(this.DB);
        }

        #region

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

        private void LoadPeoples()
        {
            this.PeopleBox.Items.Clear();
            var Items = Ext.Create<Peoples>(this.DB).Select();
            this.PeopleBox.Items.Add("- Без человека -");
            foreach(var item in Items)
                this.PeopleBox.Items.Add(item.Name);
            if(this.PeopleBox.Items.Count > 0)
                this.PeopleBox.SelectedIndex = 0;
        }
        private void LoadRoles()
        {
            this.RoleBox.Items.Clear();
            var Items = Ext.Create<Roles>(this.DB).Select();
            this.RoleBox.Items.Add("- Без роли -");
            foreach(var item in Items)
                this.RoleBox.Items.Add(item.Title);
            if(this.RoleBox.Items.Count > 0)
                this.RoleBox.SelectedIndex = 0;
        }
        private void LoadData()
        {
            this.ListBox.Items.Clear();
            var Items = this.UsersTable.Select();
            this.ListBox.Items.Add("- Новый -");
            foreach(var item in Items)
                this.ListBox.Items.Add(item.ID);
            if(this.ListBox.Items.Count > 0)
                this.ListBox.SelectedIndex = 0;
        }
        private void LoadValues()
        {
            this.ID_Box.Text = "";
            this.LoginBox.Text = "";
            this.PasswordBox.Text = "";
            this.PeopleBox.SelectedIndex = 0;
            this.RoleBox.SelectedIndex = 0;

            int id = this.GetActiveUserID();
            if(id > -1)
            {
                var User = this.UsersTable.SelectFirst($"{Users.ID} = {id}");
                this.ID_Box.Text = User.ID.ToString();
                var People = Ext.Create<Peoples>(this.DB).SelectFirst($"{Peoples.ID} = {User.People}");
                if(People != null)
                    this.PeopleBox.SelectedItem = People.Name;
                var Role = Ext.Create<Roles>(this.DB).SelectFirst($"{Roles.ID} = {User.Role}");
                if(Role != null)
                    this.RoleBox.SelectedItem = Role.Title;
            }
        }

        private void SaveData(SaveDataType Type = SaveDataType.Chesk)
        {
            int tmp = -1;
            bool isAccept = false;
            int id = this.GetActiveUserID();

            if(Type == SaveDataType.Chesk)
            {
                Type = (this.UsersTable.Count($"{Users.ID} = {id}") > 0)
                    ? SaveDataType.Update
                    : SaveDataType.Insert;
            }

            Dictionary<string, object> Fields =  new Dictionary<string, object>();

            if(this.LoginBox.Text.Length > 0)
                Fields.Add(Users.Login, Ext.SHA1(this.LoginBox.Text));
            else if(Type == SaveDataType.Insert)
                Fields.Add(Users.Login, Ext.SHA1(""));

            if(this.PasswordBox.Text.Length > 0)
                Fields.Add(Users.Password, Ext.SHA1(this.PasswordBox.Text));
            else if(Type == SaveDataType.Insert)
                Fields.Add(Users.Password, Ext.SHA1(""));

            int people = this.GetPeople();
            if(people > -1)
                Fields.Add(Users.People, people);
            else
                Fields.Add(Users.People, null);

            int role = this.GetRole();
            if(role > -1)
                Fields.Add(Users.Role, role);
            else
                Fields.Add(Users.Role, null);

            if(Type == SaveDataType.Update)
            {
                if(this.ListBox.SelectedIndex > -1 && Fields.Count > 0
                    && this.UsersTable.Update(Fields, $"{Users.ID} = {id}"))
                {
                    tmp = this.ListBox.SelectedIndex;
                    isAccept = true;
                }
            } else
            {
                Fields.Add(Users.ID, this.UsersTable.Max(Users.ID) + 1);
                if(Fields.Count > 0 && this.UsersTable.Insert(Fields))
                {
                    tmp = this.ListBox.Items.Count;
                    isAccept = true;
                }
            }
            if(isAccept)
            {
                this.LoadData();
                if(tmp > -1 && this.ListBox.Items.Count > tmp)
                    this.ListBox.SelectedIndex = tmp;
            }

            this.ViewResult(isAccept);
        }

        private void DeleteData()
        {
            int tmp = 0;
            bool isAccept = false;
            int id = this.GetActiveUserID();
            if(id > -1
                && this.UsersTable.Delete($"{Users.ID} = {id}"))
            {
                tmp = (this.ListBox.SelectedIndex < this.ListBox.Items.Count - 1)
                    ? this.ListBox.SelectedIndex
                    : this.ListBox.Items.Count - 2;
                isAccept = true;
            }

            if(isAccept)
            {
                this.LoadData();
                if(tmp > -1 && this.ListBox.Items.Count > tmp)
                    this.ListBox.SelectedIndex = tmp;
            }
            this.ViewResult(isAccept);
        }

        private Int32 GetActiveUserID()
        {
            if(this.ListBox.SelectedIndex > -1)
            {
                if(Int32.TryParse(ListBox.SelectedItem.ToString(), out int res))
                    return res;
            }
            return -1;
        }
        private Int32 GetPeople()
        {
            if(this.PeopleBox.SelectedIndex > -1)
            {
                var People = Ext.Create<Peoples>(this.DB).SelectFirst($"{Peoples.Name} Like '{this.PeopleBox.SelectedItem}'");
                return (People != null) ? People.ID : -1;
            }
            return -1;
        }
        private Int32 GetRole()
        {
            if(this.RoleBox.SelectedIndex > -1)
            {
                var Role = Ext.Create<Roles>(this.DB).SelectFirst($"{Roles.Title} Like '{this.RoleBox.SelectedItem}'");
                return (Role != null) ? Role.ID : -1;
            }
            return -1;
        }
        
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadPeoples();
            this.LoadRoles();
            this.LoadData();
        }

        private void InfoColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.UsersTable.GetLastError()?.Length > 0)
                new SharedForms.MessageBox(this.UsersTable.GetLastError(), "Текст последней ошибки БД").ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int tmp = this.ListBox.SelectedIndex;
            this.LoadData();
            if(tmp > -1)
                this.ListBox.SelectedIndex = tmp;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DeleteData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.SaveData(SaveDataType.Update);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.SaveData(SaveDataType.Insert);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.ListBox.SelectedIndex > -1)
                this.LoadValues();
        }
    }
}
