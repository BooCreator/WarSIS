using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using WarSISDataBase.DataBase;

namespace WarSIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance = null;

        MSSQLEngine DB = null;
        public Int32 PeopleID { get; set; } = -1;
        public Int32 PeopleRole { get; set; } = -1;
        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DB = new MSSQLEngine(Properties.Settings.Default.ConnectionString);
            } catch (Exception error)
            {
                new SharedForms.MessageBox(error.ToString(), "Ошибка подключения к базе данных!").ShowDialog();
                this.Close();
            }
            var Login = new SharedForms.LoginForm(this.DB);
            this.Hide();
            if(Login.ShowDialog() != true)
            {
                this.Close();
            } else
            {
                this.Show();
                this.PeopleID = Login.PeopleID;
                this.PeopleRole = Login.Role;
                if(this.PeopleRole < 0)
                {
                    this.PropertiesButton.IsEnabled = true;
                    this.BuildingsButton.IsEnabled = true;
                    this.SundivisionsButton.IsEnabled = true;
                }
                if(this.PeopleRole < 1)
                {
                    this.PeoplesButton.IsEnabled = true;
                    this.RaspisButton.IsEnabled = true;
                }
                if(this.PeopleRole == 1)
                {
                    this.AdminButton.IsEnabled = true;
                    this.UsersButton.IsEnabled = true;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var AdminForm = new AdminForms.Main(this.DB);
            AdminForm.Show();
            this.Hide();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var PeoplesForm = new MainForms.PeoplesForm(this.DB);
            PeoplesForm.Show();
            this.Hide();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var SubdivisionForm = new MainForms.SubdivisionsForm(this.DB);
            SubdivisionForm.Show();
            this.Hide();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var PropertiesForm = new MainForms.PropertiesForm(this.DB);
            PropertiesForm.Show();
            this.Hide();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var BuildingsForm = new MainForms.BuildingsForm(this.DB);
            BuildingsForm.Show();
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !(new SharedForms.MessageBox("Вы действительно хотите выйти?", "", MessageBoxButton.YesNo).ShowDialog() == true);
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            var UsersForm = new AdminForms.UsersForm(this.DB);
            UsersForm.Show();
            this.Hide();
        }

        private void RaspisButton_Click(object sender, RoutedEventArgs e)
        {
            var RaspisForm = new MainForms.RaspisForm(this.DB);
            RaspisForm.Show();
            this.Hide();
        }
    }
}
