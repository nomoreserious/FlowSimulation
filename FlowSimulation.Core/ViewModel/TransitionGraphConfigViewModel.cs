using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Helpers.MVVM;
using FlowSimulation.Scenario.Model;

namespace FlowSimulation.ViewModel
{
    public class TransitionGraphConfigViewModel : ViewModelBase, ICloseable
    {
        private Graph<WayPoint, double> _transitionGraph;
        private double _width;
        private double _height;
        private Brush _background;

        public TransitionGraphConfigViewModel(Scenario.Model.ScenarioModel scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException("scenario is null");
                        
            Width = scenario.Map[0].Width;
            Height = scenario.Map[0].Height;
            if (scenario.Map[0].Substrate != null)
            {
                Background = new ImageBrush(Helpers.Imaging.ImageManager.BitmapToBitmapImage(scenario.Map[0].Substrate));
            }
            Services = scenario.Services;
            //_transitionGraph = scenario.TransitionGraph;

            //OutputPoints = scenario.InputOutputPoints;
            //if (OutputPoints == null)
            //{
            //    OutputPoints = new List<WayPoint>();
            //}

            //_transitionGraph = null;
            NodesAndEdges = new List<object>();
            if (_transitionGraph == null)
            {
                _transitionGraph = new Graph<WayPoint, double>();

                //Для каждого слоя связываем все точки
                for (int n = 0; n < scenario.Map.Count; n++)
                {
                    var points = scenario.InputOutputPoints.Where(wp => wp.LayerId == n);
                    foreach (var point in points)
                    {
                        _transitionGraph.Add(point);
                    }

                    if (points.Count() > 1)
                    {
                        var nodes = _transitionGraph.Nodes.Where(node=>node.LayerId == n).ToList();
                        for (int i = 0; i < nodes.Count;i++)
                        {
                            if (!(nodes[i].IsOutput && !nodes[i].IsInput))
                            {
                                for (int j = 0; j < nodes.Count; j++)
                                {
                                    if (i != j && !(!nodes[j].IsOutput && nodes[j].IsInput))
                                    {
                                        _transitionGraph.AddEdge(nodes[i], nodes[j], 1.0);
                                    }
                                }
                            }
                        }
                    }
                }
                //Связываем точки - порталы
                foreach (var portal in _transitionGraph.Nodes.Where(node => node.IsLinked))
                {
                    var link = _transitionGraph.Nodes.FirstOrDefault(n => n.IsLinked && n.LinkedPoint == portal);
                    if (link != null)
                    {
                        _transitionGraph.AddEdge(portal, link, 1.0);
                    }
                    else
                    {
                        portal.IsLinked = false;
                        portal.LinkedPoint = null;
                    }
                }
            }
            NodesAndEdges.AddRange(_transitionGraph.Edges.Cast<object>());
            NodesAndEdges.AddRange(_transitionGraph.Nodes);
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged("Width"); }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); }
        }

        public Brush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged("Background"); }
        }

        public Graph<WayPoint, double> TransitionGraph { get { return _transitionGraph; } }

        public IEnumerable<WayPoint> OutputPoints { get; private set; }
        public IEnumerable<ServiceModel> Services { get; private set; }

        private WayPoint _selectedPoint;
        public WayPoint SelectedPoint
        {
            get { return _selectedPoint; }
            set {
                if (_selectedPoint == value)
                    return;
                _selectedPoint = value;
                OnPropertyChanged("SelectedPoint");
            }
        }

        private ServiceBase _selectedService;
        public ServiceBase SelectedService
        {
            get { return _selectedService; }
            set
            {
                if (_selectedService == value)
                    return;
                _selectedService = value;
                OnPropertyChanged("SelectedService");
            }
        }

        public List<object> NodesAndEdges { get; private set; }

        private object _selectedValue;
        public object SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (_selectedValue == value)
                    return;
                _selectedValue = value;
                OnPropertyChanged("SelectedValue");
            }
        }


        public ICommand AddVertexCommand
        {
            get { return new DelegateCommand<int>(AddVertex, (i)=> SelectedValue!=null); }
        }
        public ICommand SaveCommand
        {
            get { return new DelegateCommand(() => { DialogResult = true; CloseView = true; }); }
        }



        private void AddVertex(int count)
        {

        }

        #region ICloseable
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        private bool _closeView;
        public bool CloseView
        {
            get { return _closeView; }
            set
            {
                _closeView = value;
                OnPropertyChanged("CloseView");
            }
        }
        #endregion
    }

    //public class TransitionGraphLayout : GraphSharp.Controls.GraphLayout<TransitionNode, TransitionEdge, TransitionGraph>
    //{   }

    //public class TransitionGraph : QuickGraph.BidirectionalGraph<TransitionNode, TransitionEdge>
    //{   }

    public class TransitionEdge
    {
        public double Value { get; set; }

        public TransitionEdge(double value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class TransitionNode : IEquatable<TransitionNode>,ICloneable,ISerializable
    {
        public TransitionNode(bool isFirstPoint = false)
        {
            IsFirstPoint = isFirstPoint;
        }

        public ServiceBase Service { get; set; }
        public WayPoint OutputPoint { get; set; }

        public bool IsFirstPoint { get; private set; }
        public bool IsEndPoint { get { return OutputPoint != null; } }
        public bool IsService { get { return Service != null; } }

        public bool Equals(TransitionNode other)
        {
            return true;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
