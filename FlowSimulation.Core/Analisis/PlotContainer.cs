using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OxyPlot;
using FlowSimulation.Helpers.MVVM;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace FlowSimulation.Analisis
{
    public class PlotContainer : ViewModelBase
    {
        private PlotModel _model;
        private string _modelName;

        public PlotContainer(string name)
        {
            _modelName = name;
            _model = new PlotModel(_modelName);
            _model.LegendPosition = LegendPosition.TopLeft;            
            switch (_modelName)
            {
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    _model.Axes.Add(new LinearAxis(AxisPosition.Bottom, _modelName));
                    _model.Axes.Add(new TimeSpanAxis(AxisPosition.Bottom, "Время"));
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    break;
                default:
                    _model.Axes.Add(new LinearAxis(AxisPosition.Left, _modelName));
                    _model.Axes.Add(new TimeSpanAxis(AxisPosition.Bottom, "Время"));
                    break;
            }
        }

        public string ModelName 
        {
            get { return _modelName; }
        }

        public PlotModel Model 
        {
            get { return _model; }
        }

        private void AddSeries(string title)
        {
            switch (_modelName)
            {
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    _model.Series.Add(new LineSeries(title) { Smooth = true });
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    _model.Series.Add(new ScatterSeries(title, OxyColor.Interpolate(OxyColors.Blue, OxyColors.Black, 1)));
                    break;
                default:
                    _model.Series.Add(new LineSeries(title));
                    break;
            }
        }

        /// <summary>
        /// Method add point on plot
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="lineTitle"></param>
        public void AddPoint(double xValue, double yValue, string lineTitle)
        {
            if (string.IsNullOrEmpty(lineTitle))
            {
                lineTitle = "Value";
            }
            if (!_model.Series.Any(s => s.Title == lineTitle))
            {
                AddSeries(lineTitle);
            }
            switch (_modelName)
            {
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    (_model.Series.First(s => s.Title == lineTitle) as ScatterSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
                default:
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
            }
            OnPropertyChanged("Model");
        }

        /// <summary>
        /// Method set point on plot
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="yValue"></param>
        /// <param name="lineTitle"></param>
        public void SetPoint(double xValue, double yValue, string lineTitle)
        {
            if (string.IsNullOrEmpty(lineTitle))
            {
                lineTitle = "Value";
            }
            if (!_model.Series.Any(s => s.Title == lineTitle))
            {
                AddSeries(lineTitle);
            }
            switch (_modelName)
            {
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Clear();
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    (_model.Series.First(s => s.Title == lineTitle) as ScatterSeries).Points.Clear();
                    (_model.Series.First(s => s.Title == lineTitle) as ScatterSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
                default:
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Clear();
                    (_model.Series.First(s => s.Title == lineTitle) as LineSeries).Points.Add(new DataPoint(xValue, yValue));
                    break;
            }
            OnPropertyChanged("Model");
        }

        public void Optimize()
        {
            switch (_modelName)
            {
                case AnalisisConstants.AGENT_INPUT_OUTPUT_NAME:
                    for (int j = 0; j < _model.Series.Count; j++)
                    {
                        LineSeries oldSeries = _model.Series[j] as LineSeries;
                        LineSeries newSeries = new LineSeries(oldSeries.Title);
                        for (int i = 1; i < oldSeries.Points.Count; i += 2)
                        {
                            newSeries.Points.Add(new DataPoint((oldSeries.Points[i - 1].X + oldSeries.Points[i].X), (oldSeries.Points[i - 1].Y + oldSeries.Points[i].Y) / 2));
                        }
                        oldSeries = newSeries;
                    }
                    break;
                case AnalisisConstants.SPECTRAL_DENSITY_NAME:
                    break;
                default:
                    for (int j = 0; j < _model.Series.Count; j++)
                    {
                        LineSeries oldSeries = _model.Series[j] as LineSeries;
                        LineSeries newSeries = new LineSeries(oldSeries.Title);
                        for (int i = 1; i < oldSeries.Points.Count; i += 2)
                        {
                            newSeries.Points.Add(new DataPoint((oldSeries.Points[i - 1].X + oldSeries.Points[i].X) / 2, (oldSeries.Points[i - 1].Y + oldSeries.Points[i].Y) / 2));
                        }
                        oldSeries = newSeries;
                    }
                    break;
            }
        }
    }
}
