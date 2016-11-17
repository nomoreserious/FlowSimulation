using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using System.Windows.Input;
using FlowSimulation.Helpers.MVVM;

namespace FlowSimulation.Analisis
{
    public class AnalisisViewModel : ViewModelBase
    {
        #region Private members

        private ObservableCollection<PlotContainer> _plotsCollection;
        private PlotContainer _selectedPlot;
        private int _analisisStepCounter;
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public AnalisisViewModel()
        {
            StepBetweenAnalisisCount = 1;
            _analisisStepCounter = 100;
            _plotsCollection = new ObservableCollection<PlotContainer>();
            _plotsCollection.CollectionChanged += PlotsCollectionChanged;
            SaveCommand = new DelegateCommand(Save, CanSave);
        }

        #endregion

        #region Public members

        /// <summary>
        /// Command
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Plot Collection
        /// </summary>
        public ObservableCollection<PlotContainer> PlotCollection
        {
            get { return _plotsCollection; }
        }

        /// <summary>
        /// Plot current selection
        /// </summary>
        public PlotContainer SelectedPlot
        {
            get { return _selectedPlot; }
            set
            {
                if (value == _selectedPlot)
                    return;
                _selectedPlot = value;
                OnPropertyChanged("SelectedPlot");
            }

        }

        public int StepBetweenAnalisisCount { get; private set; }

        /// <summary>
        /// Add Point on Plot
        /// </summary>
        /// <param name="plotName"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void AddPoint(string plotName, string lineTitle, double xValue, double yValue)
        {
            if (!_plotsCollection.Any(p => p.ModelName == plotName))
            {
                _plotsCollection.Add(new PlotContainer(plotName));
            }
            PlotContainer plot = _plotsCollection.First(p => p.ModelName == plotName);
            plot.AddPoint(xValue, yValue, lineTitle);
            if (plot == SelectedPlot)
            {
                OnPropertyChanged("SelectedPlot");
            }
        }

        /// <summary>
        /// Set Point on Plot
        /// </summary>
        /// <param name="plotName"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void SetPoint(string plotName, string lineTitle, double xValue, double yValue)
        {
            if (!_plotsCollection.Any(p => p.ModelName == plotName))
            {
                _plotsCollection.Add(new PlotContainer(plotName));
            }
            PlotContainer plot = _plotsCollection.First(p => p.ModelName == plotName);
            plot.SetPoint(xValue, yValue, lineTitle);
            if (plot == SelectedPlot)
            {
                OnPropertyChanged("SelectedPlot");
            }
        }

        public void AnalisisComplit()
        {
            _analisisStepCounter--;
            if (_analisisStepCounter == 0)
            {
                _analisisStepCounter = 50;
                StepBetweenAnalisisCount *= 2;
                foreach (var plot in _plotsCollection)
                {
                    plot.Optimize();
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Plot collection changed callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                OnPropertyChanged("PlotCollection");
            }
        }

        /// <summary>
        /// Save command Action
        /// </summary>
        private void Save()
        {
            //SelectedPlot.Model.ToSvg(1280, 800);
        }

        /// <summary>
        /// Save command trigger
        /// </summary>
        private bool CanSave()
        {
            if (SelectedPlot != null && SelectedPlot.Model.Series.Count != 0)
                return true;
            return false;
        }

        #endregion
    }
}
