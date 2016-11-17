using System;
using System.Collections.Generic;
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
using System.Data;
using System.Windows.Threading;
using FlowSimulation.SimulationScenario;
using FlowSimulation.SimulationScenario.IO;

namespace FlowSimulation.Analisis
{
    /// <summary>
    /// Логика взаимодействия для wndDiagrammsConfig.xaml
    /// </summary>
    public partial class wndDiagrammsConfig : Window
    {
        private AnalisisCollector analisis;
        private ConfigWindows.ExperimentConfig cnfg;
        private DataTable dataSource;
        private List<PaintObject> paintObjectList;

        public wndDiagrammsConfig(AnalisisCollector analisis, ConfigWindows.ExperimentConfig cnfg, List<PaintObject> pol)
        {
            this.analisis = analisis;
            this.cnfg = cnfg;
            this.paintObjectList = pol;

            InitializeComponent();

            if (cnfg.AgentAverangeLenght)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_LENGHT_NAME);
            }
            if (cnfg.AgentAverangeLenghtByGroup)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_LENGHT_BY_GROUP_NAME);
            }
            if (cnfg.AgentAverangeSpeed)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_SPEED_NAME);
            }
            if (cnfg.AgentAverangeSpeedByGroup)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_SPEED_BY_GROUP_NAME);
            }
            if (cnfg.AgentAverangeTime)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_TIME_NAME);
            }
            if (cnfg.AgentAverangeTimeByGroup)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_AVERANGE_TIME_BY_GROUP_NAME);
            }
            if (cnfg.AgentCountOnMap)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_COUNT_ON_MAP_NAME);
            }
            if (cnfg.AgentCountOnMapByGroup)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME);
            }
            if (cnfg.AgentInputOutput)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_INPUT_OUTPUT_NAME);
            }
            if (cnfg.AgentInputOutputByGroup)
            {
                cbDataSource.Items.Add(AnalisisConstants.AGENT_INPUT_OUTPUT_BY_GROUP_NAME);
            }
            if (cnfg.SpectralDensity)
            {
                cbDataSource.Items.Add(AnalisisConstants.SPECTRAL_DENSITY_NAME);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dataSource != null)
            {
                wndGraphic wndG = new wndGraphic();
                wndG.Owner = this;
                wndG.Show();
                wndG.Data = dataSource;
            }
            else if(cnfg.SpectralDensity)
            {
                wndSpectralDensity wndSD = new wndSpectralDensity(analisis.PassengerDensity, paintObjectList);
                wndSD.Owner = this;
                wndSD.Show();
            }
        }

        private void cbDataSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDataSource.SelectedIndex == -1)
            {
                e.Handled = true;
                return;
            }
            switch ((string)cbDataSource.SelectedItem)
            {
                case AnalisisConstants.AGENT_AVERANGE_LENGHT_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распредение средней длины маршрута, пройденого пассажиром";
                    break;
                case AnalisisConstants.AGENT_AVERANGE_LENGHT_BY_GROUP_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распредение средней длины маршрута, пройденого пассажиром, по группам";
                    break;
                case AnalisisConstants.AGENT_AVERANGE_SPEED_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распредение средней скорости агентов во времени";
                    break;
                case AnalisisConstants.AGENT_AVERANGE_SPEED_BY_GROUP_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распредение средней скорости агентов во времени, разделенных по группам";
                    break;
                case AnalisisConstants.AGENT_AVERANGE_TIME_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распределение среднего времени нахождения агентов на карте во времени";
                    break;
                case AnalisisConstants.AGENT_AVERANGE_TIME_BY_GROUP_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распределение среднего времени нахождения агентов на карте во времени, разделенных по группам";
                    break;
                case AnalisisConstants.AGENT_COUNT_ON_MAP_NAME:
                    dataSource = analisis.GetAgentsCount();
                    lblNote.Content = "Данное сведение отображает распределение количества агентов на карте во времени";
                    break;
                case AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME:
                    dataSource = analisis.GetAgentsCountByGroup();
                    lblNote.Content = "Данное сведение отображает распределение количества агентов на карте во времени, разделенных по группам";
                    break;
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    dataSource = analisis.GetAgentInputOutput();
                    lblNote.Content = "Данное сведение отображает распределение входных и выходных пассажиропотоков во времени";
                    break;
                case AnalisisConstants.AGENT_INPUT_OUTPUT_BY_GROUP_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает распределение входных и выходных пассажиропотоков во времени, разделенных по группам";
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    dataSource = null;
                    lblNote.Content = "Данное сведение отображает среднюю плотность пассажиропотока за все время моделирования в виде цветового спектра";
                    break;
                default:
                    dataSource = null;
                    lblNote.Content = "Неизвестное сведение";
                    break;

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (Window wnd in this.OwnedWindows)
            {
                wnd.Close();
            }
            Close();
        }
    }
}
