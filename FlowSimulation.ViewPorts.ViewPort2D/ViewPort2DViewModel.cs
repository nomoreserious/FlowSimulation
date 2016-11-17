using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using FlowSimulation.Contracts.ViewPort;
using FlowSimulation.Contracts.Attributes;
using FlowSimulation.Contracts.Configuration;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Shapes;
using FlowSimulation.Helpers.MVVM;
using System.Drawing.Imaging;
using FlowSimulation.Helpers.Geometry;

namespace FlowSimulation.ViewPort.ViewPort2D
{
    [Export(typeof(IViewPort))]
    [ViewPortMetadata("2D", "Двухмерная визуализация", "pack://application:,,,/FlowSimulation.ViewPort.ViewPort2D;component/2d.png")]
    public class ViewPort2DViewModel : ViewModelBase, IViewPort
    {
        private UserControl _view;

        public System.Windows.Media.Brush Background { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public IEnumerable<Shape> _agents;

        public IEnumerable<Shape> Agents
        {
            get { return _agents; }
            set
            {
                _agents = value;
                OnPropertyChanged("Agents");
            }
        }

        public UserControl ViewPort
        {
            get
            {
                if (_view == null)
                {
                    _view = new ViewPort2DView();
                    _view.DataContext = this;
                }
                return _view;
            }
        }
        /// <summary>
        /// Подложка/маска триггер
        /// </summary>
        private bool _showSubstrate;
        public bool ShowSubstrate
        {
            get { return _showSubstrate; }
            set
            {
                _showSubstrate = value;
                OnPropertyChanged("ShowSubstrate");
                if (_selectedLayer != null)
                {
                    if (!_showSubstrate)
                    {
                        if (_selectedLayer.Mask != null)
                            Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(_selectedLayer.Mask));
                        else
                            Background = Brushes.Gainsboro;
                    }
                    else
                    {
                        if (_selectedLayer.Substrate != null)
                            Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(_selectedLayer.Substrate));
                        else
                            Background = Brushes.Gainsboro;
                    }
                    OnPropertyChanged("Background");
                }
            }
        }

        public List<Enviroment.Model.Layer> Layers { get; private set; }

        private Enviroment.Model.Layer _selectedLayer;
        public Enviroment.Model.Layer SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (_selectedLayer == value)
                    return;
                _selectedLayer = value;
                OnPropertyChanged("SelectedLayer");
                if (SelectedLayer != null)
                {
                    Width = _selectedLayer.Width;
                    Height = _selectedLayer.Height;
                    if (!_showSubstrate)
                    {
                        if (_selectedLayer.Mask != null)
                            Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(_selectedLayer.Mask));
                        else
                            Background = Brushes.Gainsboro;
                    }
                    else
                    {
                        if (_selectedLayer.Substrate != null)
                            Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(_selectedLayer.Substrate));
                        else
                            Background = Brushes.Gainsboro;
                    }
                    OnPropertyChanged("Height");
                    OnPropertyChanged("Width");
                    OnPropertyChanged("Background");
                    Agents = null;
                }
            }
        }

        public void Update(IEnumerable<Contracts.Agents.AgentBase> agents)
        {
            if (_selectedLayer == null)
                return;
            var shapes = new List<Shape>();

            Agents = null;
            var items = agents.Where(a => a.LayerId == Layers.IndexOf(SelectedLayer)).ToList();
            //var img = (System.Drawing.Bitmap)SelectedLayer.Mask.Clone();
            //using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img))
            //{
            foreach (var item in items)
            {
                //g.FillEllipse(System.Drawing.Brushes.Red, item.Position.X, item.Position.Y, (float)item.Size.X * 2.5F, (float)item.Size.Y * 2.5F);
                //g.DrawEllipse(new System.Drawing.Pen(System.Drawing.Brushes.Black, 0.2F), item.Position.X, item.Position.Y, (float)item.Size.X * 2.5F, (float)item.Size.Y * 2.5F);
                var shape = item.GetAgentShape();
                shape.Fill = Geometry3DHelper.GetColorByGroup(item.GroupId);
                shape.RenderTransform = new TranslateTransform(item.Position.X - 5.5, item.Position.Y - 12.5);
                if (item is Contracts.Agents.VehicleAgentBase)
                {
                    double angle = (item as Contracts.Agents.VehicleAgentBase).Angle;
                    TransformGroup tg = new TransformGroup();
                    tg.Children.Add(new RotateTransform(angle));
                    tg.Children.Add(new TranslateTransform(item.Position.X - 5.5, item.Position.Y - 12.5));
                    shape.RenderTransform = tg;
                }
                shapes.Add(shape);
            }
            //}
            //Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(img));
            //OnPropertyChanged("Background");
            Agents = shapes;
        }

        public Dictionary<string, ParamDescriptor> CreateSettings()
        {
            return new Dictionary<string, ParamDescriptor>();
        }

        public void Initialize(FlowSimulation.Enviroment.Map map, Dictionary<string, object> settings)
        {
            Layers = map;
            if (Layers.Count > 0)
            {
                SelectedLayer = Layers.First();
            }
            ShowSubstrate = true;
        }
    }
}
