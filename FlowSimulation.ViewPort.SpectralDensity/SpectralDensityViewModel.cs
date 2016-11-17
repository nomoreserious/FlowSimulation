using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Contracts.ViewPort;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.Attributes;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace FlowSimulation.ViewPort.SpectralDensity
{
    [Export(typeof(IViewPort))]
    [ViewPortMetadata("S", "Спектр плотности", "pack://application:,,,/FlowSimulation.ViewPort.SpectralDensity;component/spectr.png")]
    public class SpectralDensityViewModel : ViewModelBase, IViewPort
    {
        private UserControl _view;
        private Enviroment.Map _map;
        private uint[,] _passengerDensity;
        private ulong[,] _passengerLastLocations;
        private Brush _spectralBrush;
        //private BackgroundWorker _worker;

        public Brush SpectralBrush
        {
            get { return _spectralBrush; }
            set { _spectralBrush = value; OnPropertyChanged("SpectralBrush"); }
        }

        public Brush Background { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public System.Windows.Controls.UserControl ViewPort
        {
            get
            {
                if (_view == null)
                {
                    _view = new SpectralDensityView();
                    _view.DataContext = this;
                }
                return _view;
            }
        }

        public void Update(IEnumerable<Contracts.Agents.AgentBase> agents)
        {
            try
            {
                foreach (var agent in agents)
                {
                    var pos = agent.Position;
                    if (_passengerLastLocations[pos.X, pos.Y] != agent.Id)
                    {
                        _passengerLastLocations[pos.X, pos.Y] = agent.Id;
                        _passengerDensity[pos.X, pos.Y] += 2U;
                        if (pos.X > 0)
                            _passengerDensity[pos.X - 1, pos.Y] += 1U;
                        if (pos.X < _passengerDensity.GetLength(0) - 1)
                            _passengerDensity[pos.X + 1, pos.Y] += 1U;
                        if (pos.Y > 0)
                            _passengerDensity[pos.X, pos.Y - 1] += 1U;
                        if(pos.Y < _passengerDensity.GetLength(1))
                        _passengerDensity[pos.X, pos.Y + 1] += 1U;
                    }
                }
                SpectralBrush = GetSpectorImageBrush();
            }
            catch (InvalidOperationException)
            {
                //TODO костыль
            }
            //if (!_worker.IsBusy)
            //{
            //    _worker.RunWorkerAsync();
            //}
        }

        public void Initialize(Enviroment.Map map, Dictionary<string, object> settings)
        {
            _map = map;
            if (map.Count == 0)
            {
                Width = 800;
                Height = 600;
                Background = Brushes.Gainsboro;
            }
            else
            {
                Width = map[0].Width;
                Height = map[0].Height;
                if (map[0].Substrate != null)
                {
                    Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(map[0].Substrate));
                }
                else
                {
                    Background = Brushes.Gainsboro;
                }
            }

            

            if (_passengerDensity == null || _passengerLastLocations == null)
            {
                _passengerDensity = new uint[Width, Height];
                _passengerLastLocations = new ulong[Width, Height];
            }
            //_worker = new BackgroundWorker();
            //_worker.DoWork += (s, e) =>
            //{
            //    _spectralBrush = GetSpectorImageBrush();
            //};
            //_worker.RunWorkerCompleted += (s, e) =>
            //{
            //    OnPropertyChanged("SpectralBrush");
            //    //if (e.Result is ImageBrush)
            //    //{
            //    //    SpectralBrush = (ImageBrush)e.Result;
            //    //}
            //};

            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
            OnPropertyChanged("Background");
        }

        public Dictionary<string, Contracts.Configuration.ParamDescriptor> CreateSettings()
        {
            return new Dictionary<string, Contracts.Configuration.ParamDescriptor>();
        }

        private Brush GetSpectorImageBrush()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(_passengerDensity.GetLength(0), _passengerDensity.GetLength(1));
            uint max = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (max < _passengerDensity[i, j])
                    {
                        max = _passengerDensity[i, j];
                    }
                }
            }

            if (max == 0)
            {
                return Brushes.Transparent;
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (_passengerDensity[i, j] == 0)
                        continue;
                    double value = (double)_passengerDensity[i, j] / max;
                    if (value < 0.2)
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255, 255 - Convert.ToInt32(255 * (value == 0 ? value : value + 0.3)), 255, 255 - Convert.ToInt32(255 * (value == 0 ? value : value + 0.3))));
                    }
                    else if (value < 0.6)
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255, 255 - Convert.ToInt32(255 * (value + 0.1)), 255 - Convert.ToInt32(255 * (value + 0.1)), 255));
                    }
                    else
                    {
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255, 255, 255 - Convert.ToInt32(255 * value), 255 - Convert.ToInt32(255 * value)));
                    }
                }
            }
            return new System.Windows.Media.ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(bmp));
        }
    }
}
