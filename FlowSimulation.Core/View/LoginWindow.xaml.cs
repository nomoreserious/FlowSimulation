using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlowSimulation.Helpers.MVVM;

namespace FlowSimulation.View
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string _path;

        public LoginWindow(string path)
        {
            _path = path;
            DataContext = this;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        public ICommand LoginCommand
        {
            get
            {
                return new DelegateCommand(() =>
                    {
                        string login = txtName.Text, pass = txtPass.Password;
                        if ((login == "vidovas" && pass == "miit2013") ||
                            (login == "leon" && pass == "22010406"))
                        {
                            SimulationView wnd = new SimulationView();
                            if (!string.IsNullOrEmpty(_path))
                            {
                                wnd.DataContext = new ViewModel.SimulationViewModel(_path);
                            }
                            else
                            {
                                wnd.DataContext = new ViewModel.SimulationViewModel();
                            }
                            wnd.Show();
                            Application.Current.MainWindow = wnd;
                            this.Close();
                        }
                        else
                        {
                            txtInfo.Text = "Логин или пароль введен неверно";
                        }
                    }, () => !string.IsNullOrWhiteSpace(txtName.Text) && !string.IsNullOrWhiteSpace(txtPass.Password));
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtInfo.Text = string.Empty;
        }

        private void txtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = string.Empty;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (LoginCommand.CanExecute(null))
                    LoginCommand.Execute(null);
            }
            base.OnKeyDown(e);
        }
    }
}
