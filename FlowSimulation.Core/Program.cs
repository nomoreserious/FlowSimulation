using System;
using System.Windows;
using Microsoft.Win32;

namespace FlowSimulation
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application app = new Application();
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml") });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml") });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml") });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Resources/ResourceDictionary.xaml", UriKind.Relative) });

            string[] args = Environment.GetCommandLineArgs();
            string path = string.Empty;
            if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
            {
                path = args[1];
            }

            //RegFile(Environment.CurrentDirectory.Replace(@"\", @"\\"));    
#if DEBUG
            View.SimulationView wnd = new View.SimulationView();
            wnd.DataContext = new ViewModel.SimulationViewModel(path);
            app.Run(wnd);
#else
            try
            {
                var wnd = new View.LoginWindow(path);
                app.Run(wnd);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
#endif
            Properties.Settings.Default.Save();
        }

        static void RegFile(string path)
        {
            try
            {
                RegistryKey pReg;
                pReg = Registry.ClassesRoot.CreateSubKey(".scn");
                pReg.SetValue(null, "FlowSimulation");
                pReg = Registry.ClassesRoot.CreateSubKey("FlowSimulation");
                pReg.SetValue(null, "FlowSimulation");
                pReg = Registry.ClassesRoot.CreateSubKey("FlowSimulation\\shell\\open\\command");
                pReg.SetValue(null, "\"" + path + "\\FlowSimulation.exe\" \"%1\"");
                pReg = Registry.ClassesRoot.CreateSubKey("FlowSimulation\\DefaultIcon");
                pReg.SetValue(null, path + "\\Custom.ico");
                pReg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + ".scn");
                pReg.SetValue(null, "FlowSimulation");
                pReg.Close();
            }
            catch (Exception)
            { }
        }
    }
}
