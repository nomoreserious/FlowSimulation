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

namespace FlowSimulation.View
{
    /// <summary>
    /// Логика взаимодействия для SimulationView.xaml
    /// </summary>
    public partial class SimulationView : MahApps.Metro.Controls.MetroWindow
    {
        private SplashScreen _ss;
        public SimulationView()
        {
            _ss = new SplashScreen(@"Images/splash.jpg");
            _ss.Show(true, true);
            InitializeComponent();
        }
    }
}
