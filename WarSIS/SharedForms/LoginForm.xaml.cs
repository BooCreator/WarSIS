using System;
using System.Windows;
using System.Collections.Generic;

using WarSISDataBase.DataBase;
using WarSISModelsDB.Models.DataBase;

namespace WarSIS.SharedForms
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        MSSQLEngine DB = null;
        public Int32 PeopleID { get; private set; } = -1;
        public Int32 Role { get; private set; } = -1;
        public LoginForm(MSSQLEngine DataBase)
        {
            this.InitializeComponent();
            this.DB = DataBase;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool register = false;

            var UserTable = new Users() { Editor = this.DB };
            string Login = Ext.SHA1(this.LoginBox.Text);
            string Password = Ext.SHA1(this.PasswordBox.Password);
            if (!register)
            {
                var User = UserTable.SelectFirst($"{Users.Login} like '{Login}' and {Users.Password} like '{Password}'");
                if (User != null)
                {
                    this.DialogResult = true;
                    this.PeopleID = User.People;
                    this.Role = User.Role;
                }
                else
                    new MessageBox("Неверный логин или пароль").ShowDialog();
            } else
            {
                Dictionary<string, object> Fields = new Dictionary<string, object>()
                {
                    { Users.ID, (UserTable.Max(Users.ID) + 1) },
                    { Users.Login, Login },
                    { Users.Password, Password }
                };
                if (UserTable.Insert(Fields))
                {
                    new SharedForms.MessageBox("Ok", Buttons: MessageBoxButton.OK).ShowDialog();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoginBox.Focus();
        }
    }
}
