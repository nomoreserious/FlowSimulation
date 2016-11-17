using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FlowSimulation.Agents;


namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndAgentsGroupConfig.xaml
    /// </summary>
    public partial class wndAgentsGroupConfig : Window
    {
        private AgentsGroup group;
        private List<AgentsGroup> groupList;

        public wndAgentsGroupConfig()
        {
            InitializeComponent();
            cbGridRow.Height = new GridLength(0);
            group = new AgentsGroup();
            group.AgentTemplateList = new List<AgentTemplate>();
            group.AgentDistribution = new List<int>();
        }

        public wndAgentsGroupConfig(AgentsGroup group)
        {
            InitializeComponent();
            cbGridRow.Height = new GridLength(0);
            this.group = group;
            tbName.Text = group.Name;
            if (group.IsNetworkGroup)
            {
                rbNetwork.IsChecked = true;
            }
            else
            {
                rbTimer.IsChecked = true;
            }
            if (group.IsNetworkGroup)
            {
                tbAddress.Text = group.Address;
                tbPort.Text = group.Port.ToString();
            }
            lvAgentsTemplate.ItemsSource = group.AgentTemplateList;
        }

        public wndAgentsGroupConfig(List<AgentsGroup> groupList)
        {
            this.groupList = groupList;

            InitializeComponent();

            foreach (var group in groupList)
            {
                cbGroup.Items.Add(group);
            }
        }

        public AgentsGroup GetGroup()
        {
            return group;
        }

        public List<AgentsGroup> GetGroupList()
        {
            return groupList;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AgentTemplateConfigWindow temp = new AgentTemplateConfigWindow();
            if (temp.ShowDialog().GetValueOrDefault())
            {
                group.AgentTemplateList.Add(temp.GetTemplate());
            }
            lvAgentsTemplate.ItemsSource = null;
            lvAgentsTemplate.ItemsSource = group.AgentTemplateList;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lvAgentsTemplate.SelectedIndex >= 0)
            {               
                group.AgentTemplateList.RemoveAt(lvAgentsTemplate.SelectedIndex);
            }
            lvAgentsTemplate.ItemsSource = null;
            lvAgentsTemplate.ItemsSource = group.AgentTemplateList;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvAgentsTemplate.SelectedIndex >= 0)
            {
                AgentTemplateConfigWindow temp = new AgentTemplateConfigWindow(group.AgentTemplateList[lvAgentsTemplate.SelectedIndex]);
                if (temp.ShowDialog().GetValueOrDefault())
                {
                    group.AgentTemplateList[lvAgentsTemplate.SelectedIndex] = temp.GetTemplate();
                }
            }
            lvAgentsTemplate.ItemsSource = null;
            lvAgentsTemplate.ItemsSource = group.AgentTemplateList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                group.IsNetworkGroup = !rbTimer.IsChecked.GetValueOrDefault();
                if (group.IsNetworkGroup)
                {
                    group.Address = System.Net.IPAddress.Parse(tbAddress.Text).ToString();
                    group.Port = int.Parse(tbPort.Text);
                }
                if (string.IsNullOrEmpty(tbName.Text))
                {
                    MessageBox.Show("Введите имя группы");
                    return;
                }
                if (!group.IsNetworkGroup && group.AgentDistribution == null)
                {
                    MessageBox.Show("Введите имя группы");
                    return;
                }
                group.Name = tbName.Text;
            }
            catch
            { }
            DialogResult = true;
            Close();
        }

        private void btnCansel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void cbGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbGroup.SelectedIndex != -1)
            {
                group = (AgentsGroup)cbGroup.SelectedValue;

                tbName.Text = group.Name;
                if (group.IsNetworkGroup)
                {
                    rbNetwork.IsChecked = true;
                }
                else
                {
                    rbTimer.IsChecked = true;
                }
                if (group.IsNetworkGroup)
                {
                    tbAddress.Text = group.Address;
                    tbPort.Text = group.Port.ToString();
                }
                lvAgentsTemplate.ItemsSource = group.AgentTemplateList;
            }
        }

        private void btnRemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (cbGroup.SelectedIndex != -1)
            {
                if (MessageBox.Show("Вы дейтвительно хотите удалить группу агентов?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string name = cbGroup.SelectedValue.ToString();
                    cbGroup.Items.Remove(cbGroup.SelectedValue);
                    groupList.RemoveAll(delegate(AgentsGroup ag) { return ag.Name == name; });
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (group == null)
            {
                MessageBox.Show("Сначала выберите группу");
                return;
            }
            wndInitPointConfig cnfg;
            int max_per_min;
            if (group.AgentTemplateList != null && group.AgentTemplateList.Count > 0 && group.AgentTemplateList[0].Type != typeof(HumanAgent).Name)
            {
                max_per_min = 5;
            }
            else
            {
                max_per_min = 180;
            }
            if (group!=null && group.AgentDistribution!=null)
            {
                cnfg = new wndInitPointConfig(group.AgentDistribution.ToArray(), max_per_min);
            }
            else
            {
                cnfg = new wndInitPointConfig(max_per_min);
            }
            if (cnfg.ShowDialog().GetValueOrDefault())
            {
                group.AgentDistribution = cnfg.InitPointDistribution;
            }
        }
    }
}
