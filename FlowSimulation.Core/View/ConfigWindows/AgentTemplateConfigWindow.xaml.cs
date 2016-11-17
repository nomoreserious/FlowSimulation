using System.Windows;
using FlowSimulation.Agents;
using System;
using FlowSimulation.Map.Model;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для AgentTemplateConfigWindow.xaml
    /// </summary>
    public partial class AgentTemplateConfigWindow : Window
    {
        private AgentTemplate template;

        public AgentTemplateConfigWindow()
        {
            InitializeComponent();
            for (int i = 0; i < AgentBase.AgentsTypesList.Length; i++)
            {
                cbType.Items.Add(AgentBase.AgentsTypesList[i]);
            }
            template = new AgentTemplate();
            this.DataContext = template;
        }

        public AgentTemplateConfigWindow(AgentTemplate template)
        {
            InitializeComponent();
            this.template = template;
            this.DataContext = this.template;
            for (int i = 0; i < AgentBase.AgentsTypesList.Length; i++)
            {
                cbType.Items.Add(AgentBase.AgentsTypesList[i]);
                if (AgentBase.AgentsTypesList[i].Type.Name == template.Type)
                {
                    cbType.SelectedIndex = i;
                }
            }
        }

        public AgentTemplate GetTemplate()
        {
            return template;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(template.Name))
            {
                MessageBox.Show("Укажите все параметры");
                return;
            }
            if (template.WayPointsList.Count == 0)
            {
                MessageBox.Show("Задайте путевые точки");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            if (cbType.SelectedIndex == -1)
            {
                MessageBox.Show("Сначала укажите тип агента");
                return;
            }
            AgentTypesNames typeName = (AgentTypesNames)cbType.SelectedItem;
            if (typeName.Type == typeof(HumanAgent))
            {
                wndWayPointsConfig wnd;
                if (template.WayPointsList != null && template.WayPointsList.Count != 0)
                {
                    wnd = new wndWayPointsConfig(template.WayPointsList);
                }
                else
                {
                    wnd = new wndWayPointsConfig();
                }
                if (wnd.ShowDialog().GetValueOrDefault())
                {
                    template.WayPointsList = wnd.WayPointsList;
                    lvWayPointsList.ItemsSource = null;
                    lvWayPointsList.ItemsSource = template.WayPointsList;
                }
            }
            else if (typeName.Type == typeof(BusAgent) || typeName.Type == typeof(TrainAgent))
            {
                var roadGraph = (Application.Current.MainWindow as MainWindow).Scena.RoadGraph;
                if (roadGraph.Nodes == null)
                {
                    MessageBox.Show("Сначала задайте дорожную сеть");
                    return;
                }
                VehicleWayPointsConfigWindow wnd = new VehicleWayPointsConfigWindow(roadGraph);
                if (wnd.ShowDialog().GetValueOrDefault())
                {
                    template.WayPointsList = wnd.GetWayPointsList();
                    lvWayPointsList.ItemsSource = null;
                    lvWayPointsList.ItemsSource = template.WayPointsList;
                }
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvWayPointsList.SelectedIndex != -1)
            {
                template.WayPointsList.RemoveAt(lvWayPointsList.SelectedIndex);
                lvWayPointsList.ItemsSource = null;
                lvWayPointsList.ItemsSource = template.WayPointsList;
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            if (lvWayPointsList.SelectedIndex > 0)
            {
                WayPoint wp = template.WayPointsList[lvWayPointsList.SelectedIndex];
                template.WayPointsList.RemoveAt(lvWayPointsList.SelectedIndex);
                template.WayPointsList.Insert(lvWayPointsList.SelectedIndex - 1, wp);
                lvWayPointsList.ItemsSource = null;
                lvWayPointsList.ItemsSource = template.WayPointsList;
            }
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (lvWayPointsList.SelectedIndex != -1 && lvWayPointsList.SelectedIndex < template.WayPointsList.Count - 1)
            {
                WayPoint wp = template.WayPointsList[lvWayPointsList.SelectedIndex];
                template.WayPointsList.RemoveAt(lvWayPointsList.SelectedIndex);
                template.WayPointsList.Insert(lvWayPointsList.SelectedIndex + 1, wp);
                lvWayPointsList.ItemsSource = null;
                lvWayPointsList.ItemsSource = template.WayPointsList;
            }
        }

        private void cbType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AgentTypesNames atn = (AgentTypesNames)cbType.SelectedItem;
            template.Type = atn.Type.Name;
            if (atn.Type == typeof(HumanAgent))
            {
                tbCapasity.IsEnabled = false;
                tbNumberOfCarriges.IsEnabled = false;
            }
            if (atn.Type == typeof(BusAgent))
            {
                tbCapasity.IsEnabled = true;
                tbNumberOfCarriges.IsEnabled = false;
            }
            if (atn.Type == typeof(TrainAgent))
            {
                tbCapasity.IsEnabled = true;
                tbNumberOfCarriges.IsEnabled = true;
            }
        }
    }
}
