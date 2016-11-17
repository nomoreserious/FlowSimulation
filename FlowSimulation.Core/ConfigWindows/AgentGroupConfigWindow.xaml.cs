using System.Windows;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndAgentsGroupConfig.xaml
    /// </summary>
    public partial class AgentGroupConfigWindow : Window
    {
        public AgentGroupConfigWindow()
        {
            InitializeComponent();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
