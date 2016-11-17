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
using FlowSimulation.Analisis;

namespace FlowSimulation.ConfigWindows
{
    /// <summary>
    /// Логика взаимодействия для wndExperimentConfig.xaml
    /// </summary>
    public partial class wndExperimentConfig : Window
    {
        public ExperimentConfig Cnfg { get; set; }
        DoubleSlider slider;

        public wndExperimentConfig()
        {
            Cnfg = new ExperimentConfig();

            InitializeComponent();

            slider = new DoubleSlider(0, 1440, 5);
            slider.FromValue = 0;
            slider.ToValue = 1440;
            SliderPanel.Children.Add(slider);

            cbAgentAverangeLenght.Content = AnalisisConstants.AGENT_AVERANGE_LENGHT_NAME;
            cbAgentAverangeLenghtByGroup.Content = AnalisisConstants.AGENT_AVERANGE_LENGHT_BY_GROUP_NAME;
            cbAgentAverangeSpeed.Content = AnalisisConstants.AGENT_AVERANGE_SPEED_NAME;
            cbAgentAverangeSpeedByGroup.Content = AnalisisConstants.AGENT_AVERANGE_SPEED_BY_GROUP_NAME;
            cbAgentAverangeTime.Content = AnalisisConstants.AGENT_AVERANGE_TIME_NAME;
            cbAgentAverangeTimeByGroup.Content = AnalisisConstants.AGENT_AVERANGE_TIME_BY_GROUP_NAME;
            cbAgentCountOnMap.Content = AnalisisConstants.AGENT_COUNT_ON_MAP_NAME;
            cbAgentCountOnMapByGroup.Content = AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME;
            cbAgentInputOutput.Content = AnalisisConstants.AGENT_INPUT_OUTPUT_NAME;
            cbAgentInputOutputByGroup.Content = AnalisisConstants.AGENT_INPUT_OUTPUT_BY_GROUP_NAME;
            cbSpectralDensity.Content = AnalisisConstants.SPECTRAL_DENSITY_NAME;
        }

        public wndExperimentConfig(ExperimentConfig cnfg)
        {
            Cnfg = cnfg;

            InitializeComponent();

            slider = new DoubleSlider(0, 1440, 5);
            slider.FromValue = cnfg.StartTime.TotalMinutes;
            slider.ToValue = cnfg.StopTime.TotalMinutes;
            SliderPanel.Children.Add(slider);

            cbAgentAverangeLenght.Content = AnalisisConstants.AGENT_AVERANGE_LENGHT_NAME;
            cbAgentAverangeLenght.IsChecked = cnfg.AgentAverangeLenght;
            cbAgentAverangeLenghtByGroup.Content = AnalisisConstants.AGENT_AVERANGE_LENGHT_BY_GROUP_NAME;
            cbAgentAverangeLenghtByGroup.IsChecked = cnfg.AgentAverangeLenghtByGroup;
            cbAgentAverangeSpeed.Content = AnalisisConstants.AGENT_AVERANGE_SPEED_NAME;
            cbAgentAverangeSpeed.IsChecked = cnfg.AgentAverangeSpeed;
            cbAgentAverangeSpeedByGroup.Content = AnalisisConstants.AGENT_AVERANGE_SPEED_BY_GROUP_NAME;
            cbAgentAverangeSpeedByGroup.IsChecked = cnfg.AgentAverangeSpeedByGroup;
            cbAgentAverangeTime.Content = AnalisisConstants.AGENT_AVERANGE_TIME_NAME;
            cbAgentAverangeTime.IsChecked = cnfg.AgentAverangeTime;
            cbAgentAverangeTimeByGroup.Content = AnalisisConstants.AGENT_AVERANGE_TIME_BY_GROUP_NAME;
            cbAgentAverangeTimeByGroup.IsChecked = cnfg.AgentAverangeTimeByGroup;
            cbAgentCountOnMap.Content = AnalisisConstants.AGENT_COUNT_ON_MAP_NAME;
            cbAgentCountOnMap.IsChecked = cnfg.AgentCountOnMap;
            cbAgentCountOnMapByGroup.Content = AnalisisConstants.AGENT_COUNT_ON_MAP_BY_GROUP_NAME;
            cbAgentCountOnMapByGroup.IsChecked = cnfg.AgentCountOnMapByGroup;
            cbAgentInputOutput.Content = AnalisisConstants.AGENT_INPUT_OUTPUT_NAME;
            cbAgentInputOutput.IsChecked = cnfg.AgentInputOutput;
            cbAgentInputOutputByGroup.Content = AnalisisConstants.AGENT_INPUT_OUTPUT_BY_GROUP_NAME;
            cbAgentInputOutputByGroup.IsChecked = cnfg.AgentInputOutputByGroup;
            cbSpectralDensity.Content = AnalisisConstants.SPECTRAL_DENSITY_NAME;
            cbSpectralDensity.IsChecked = cnfg.SpectralDensity;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).IsDefault)
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            Close();
        }

        private void ButtonAllNone_Click(object sender, RoutedEventArgs e)
        {
            bool value = true;
            if ((sender as Button).Content.ToString() == "Снять все")
            {
                value = false;
            }
            cbAgentAverangeLenght.IsChecked = value;
            cbAgentAverangeLenghtByGroup.IsChecked = value;
            cbAgentAverangeSpeed.IsChecked = value;
            cbAgentAverangeSpeedByGroup.IsChecked = value;
            cbAgentAverangeTime.IsChecked = value;
            cbAgentAverangeTimeByGroup.IsChecked = value;
            cbAgentCountOnMap.IsChecked = value;
            cbAgentCountOnMapByGroup.IsChecked = value;
            cbAgentInputOutput.IsChecked = value;
            cbAgentInputOutputByGroup.IsChecked = value;
            cbSpectralDensity.IsChecked = value;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult.GetValueOrDefault() == true)
            {
                Cnfg.StartTime = TimeSpan.FromMinutes(Math.Ceiling(slider.FromValue));
                Cnfg.StopTime = TimeSpan.FromMinutes(Math.Ceiling(slider.ToValue));
                Cnfg.AgentAverangeLenght = cbAgentAverangeLenght.IsChecked.GetValueOrDefault();
                Cnfg.AgentAverangeLenghtByGroup = cbAgentAverangeLenghtByGroup.IsChecked.GetValueOrDefault();
                Cnfg.AgentAverangeSpeed = cbAgentAverangeSpeed.IsChecked.GetValueOrDefault();
                Cnfg.AgentAverangeSpeedByGroup = cbAgentAverangeSpeedByGroup.IsChecked.GetValueOrDefault();
                Cnfg.AgentAverangeTime = cbAgentAverangeTime.IsChecked.GetValueOrDefault();
                Cnfg.AgentAverangeTimeByGroup = cbAgentAverangeTimeByGroup.IsChecked.GetValueOrDefault();
                Cnfg.AgentCountOnMap = cbAgentCountOnMap.IsChecked.GetValueOrDefault();
                Cnfg.AgentCountOnMapByGroup = cbAgentCountOnMapByGroup.IsChecked.GetValueOrDefault();
                Cnfg.AgentInputOutput = cbAgentInputOutput.IsChecked.GetValueOrDefault();
                Cnfg.AgentInputOutputByGroup = cbAgentInputOutputByGroup.IsChecked.GetValueOrDefault();
                Cnfg.SpectralDensity = cbSpectralDensity.IsChecked.GetValueOrDefault();

                Cnfg.AverangeQueueLenght = true;
            }
        }
    }

    public class ExperimentConfig
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan StopTime { get; set; }

        public bool AverangeQueueLenght { get; set; }
        public bool AgentAverangeLenght { get; set; }
        public bool AgentAverangeLenghtByGroup { get; set; }
        public bool AgentAverangeSpeed { get; set; }
        public bool AgentAverangeSpeedByGroup { get; set; }
        public bool AgentAverangeTime { get; set; }
        public bool AgentAverangeTimeByGroup { get; set; }
        public bool AgentCountOnMap { get; set; }
        public bool AgentCountOnMapByGroup { get; set; }
        public bool AgentInputOutput { get; set; }
        public bool AgentInputOutputByGroup { get; set; }
        public bool SpectralDensity { get; set; }
    }
}
