using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FlowSimulation.Agents;
using FlowSimulation.Service;
using FlowSimulation.SimulationScenario;
using FlowSimulation.Map.Model;


namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndAgentsGroupConfig.xaml
    /// </summary>
    public partial class ServiceConfigWindow : Window
    {
        private ServiceBase service;
        private List<ServiceBase> serviceList;
        private Scenario scenario;

        public ServiceConfigWindow(Scenario scenario, bool config)
        {
            this.scenario = scenario;
            this.serviceList = scenario.ServicesList;
            InitializeComponent();
            if (config)
            {
                for (int i = 0; i < serviceList.Count; i++)
                {
                    cbServiseSelector.Items.Add(serviceList[i]);
                }
                cbServiceType.IsEnabled = false;
            }
            else
            {
                cbGridRow.Height = new GridLength(0);
            }
        }

        public ServiceBase GetService()
        {
            return service;
        }

        public List<ServiceBase> GetServiceList()
        {
            return serviceList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                service.Name = tbName.Text;
                service.scenario = scenario;
            }
            DialogResult = true;
            Close();
        }

        private void btnCansel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DeleteInputPoint_Click(object sender, RoutedEventArgs e)
        {
            (service as StopService).InputPoints.Remove(lvInputPoints.SelectedValue as WayPoint);
            lvInputPoints.ItemsSource = null;
            lvInputPoints.ItemsSource = (service as StopService).InputPoints;
        }

        private void DeleteOutputPoint_Click(object sender, RoutedEventArgs e)
        {
            (service as StopService).OutputPoints.Remove(lvOutputPoints.SelectedValue as WayPoint);
            lvOutputPoints.ItemsSource = null;
            lvOutputPoints.ItemsSource = (service as StopService).OutputPoints;
        }

        private void ConfigInputPoint_Click(object sender, RoutedEventArgs e)
        {
            wndWayPointsConfig wpc;
            if (service is StopService)
            {
                StopService serv = service as StopService;
                if (serv.InputPoints == null)
                {
                    wpc = new wndWayPointsConfig();
                }
                else
                {
                    wpc = new wndWayPointsConfig(serv.InputPoints);
                }
                if (wpc.ShowDialog() == true)
                {
                    (service as StopService).InputPoints = wpc.WayPointsList;
                    lvInputPoints.ItemsSource = null;
                    lvInputPoints.ItemsSource = (service as StopService).InputPoints;
                }
            }
            else if (service is QueueService)
            {
                QueueService serv = service as QueueService;
                if (serv.InputPoints == null)
                {
                    wpc = new wndWayPointsConfig();
                }
                else
                {
                    wpc = new wndWayPointsConfig(serv.InputPoints);
                }
                if (wpc.ShowDialog() == true)
                {
                    (service as QueueService).InputPoints = wpc.WayPointsList;
                    lvInputQueuePoints.ItemsSource = null;
                    lvInputQueuePoints.ItemsSource = (service as QueueService).InputPoints;
                }
            }
        }

        private void ConfigOutputPoint_Click(object sender, RoutedEventArgs e)
        {
            wndWayPointsConfig wpc;
            if ((service as StopService).OutputPoints == null)
            {
                wpc = new wndWayPointsConfig();
            }
            else
            {
                wpc = new wndWayPointsConfig((service as StopService).OutputPoints);
            }
            if (wpc.ShowDialog() == true)
            {
                (service as StopService).OutputPoints = wpc.WayPointsList;
            }
            lvOutputPoints.ItemsSource = null;
            lvOutputPoints.ItemsSource = (service as StopService).OutputPoints;
        }

        private void cbServiceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((cbServiceType.SelectedValue as ComboBoxItem).Content.ToString())
            {
                case "Остановка общественного транспорта":
                    stopServiseConfigGrid.Visibility = Visibility.Visible;
                    trunstileServiceConfigGrid.Visibility = Visibility.Hidden;
                    queueServiceConfigGrid.Visibility = Visibility.Hidden;
                    if (service == null || service is StopService == false)
                    {
                        service = new StopService();
                        stopServiseConfigGrid.DataContext = null;
                        stopServiseConfigGrid.DataContext = service;
                    }
                    break;

                case "Турникетная группа":
                    stopServiseConfigGrid.Visibility = Visibility.Hidden;
                    trunstileServiceConfigGrid.Visibility = Visibility.Visible;
                    queueServiceConfigGrid.Visibility = Visibility.Hidden;
                    if (service == null || service is TurnstileService == false)
                    {
                        service = new TurnstileService();
                        trunstileServiceConfigGrid.DataContext = null;
                        trunstileServiceConfigGrid.DataContext = service;
                    }
                    break;

                case "Очередь":
                    stopServiseConfigGrid.Visibility = Visibility.Hidden;
                    trunstileServiceConfigGrid.Visibility = Visibility.Hidden;
                    queueServiceConfigGrid.Visibility = Visibility.Visible;
                    if (service == null || service is QueueService == false)
                    {
                        service = new QueueService();
                        queueServiceConfigGrid.DataContext = null;
                        queueServiceConfigGrid.DataContext = service;
                    }
                    break;
            }
        }

        private void cbServiseSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            service = cbServiseSelector.SelectedItem as ServiceBase;
            tbName.Text = service.Name;
            if (service is StopService)
            {
                cbServiceType.SelectedIndex = 0;
                stopServiseConfigGrid.DataContext = null;
                stopServiseConfigGrid.DataContext = service as StopService;             
            }
            else if (service is TurnstileService)
            {
                cbServiceType.SelectedIndex = 1;
                trunstileServiceConfigGrid.DataContext = null;
                trunstileServiceConfigGrid.DataContext = cbServiseSelector.SelectedItem as TurnstileService;
            }
            else if (service is QueueService)
            {
                cbServiceType.SelectedIndex = 2;
                queueServiceConfigGrid.DataContext = null;
                queueServiceConfigGrid.DataContext = cbServiseSelector.SelectedItem as QueueService;
            }
        }

        private void ButtonGroupConfig_Click(object sender, RoutedEventArgs e)
        {
            wndAgentsGroupConfig wagc;
            if ((service as StopService).PassengersGroup == null)
            {
                wagc = new wndAgentsGroupConfig();
            }
            else
            {
                wagc = new wndAgentsGroupConfig((service as StopService).PassengersGroup);
            }
            if (wagc.ShowDialog() == true)
            {
                AgentsGroup group = wagc.GetGroup();
                group.IsServiceGroup = true;
                if (group.ID == -1)
                {
                    group.ID = GroupIDEnumerator.GetNextID();
                }
                (service as StopService).PassengersGroup = group;
            }
        }

        private void ConfigPoint_Click(object sender, RoutedEventArgs e)
        {
            wndRoadGraphConfig wrgc = new wndRoadGraphConfig();
            if ((service as TurnstileService).TurnstileGeometry == null)
            {
                wrgc = new wndRoadGraphConfig();
            }
            if (wrgc.ShowDialog() == true)
            {
                foreach (var edge in wrgc.RoadGraph.Edges)
                {
                    (service as TurnstileService).TurnstileGeometry = edge.Data;
                    break;
                }
            }
        }
    }
}
