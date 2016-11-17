using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;
using FlowSimulation.Agents;

namespace FlowSimulation.AgentsVisual2D
{
    abstract class  AgentVisualBase : FrameworkElement
    {
        #region Registration location property

        public static readonly DependencyProperty LocationProperty;
        public static readonly DependencyProperty AngleProperty;
        public AgentBase agentBase { get; set; }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set
            {
                if (value != double.NaN && value!= null)
                {
                    SetValue(AngleProperty, value);
                }
            }
        }

        public Point Location
        {
            get { return (Point)GetValue(LocationProperty); }
            set 
            {
                //Console.WriteLine(string.Format("Перемещение агента id:{0} from [{1},{2}] to [{3},{4}]", agentBase.ID, Location.X, Location.Y, value.X, value.Y));
                SetValue(LocationProperty, value);
            }
        }

        public double AngleAn
        {
            set
            {
                DoubleAnimation anA = new DoubleAnimation(Angle, value, new Duration(TimeSpan.FromMilliseconds(500 * 2 / agentBase.SpeedRatio)), FillBehavior.Stop);
                Storyboard board = new Storyboard();
                board.Children.Add(anA);
                Storyboard.SetTargetProperty(anA, new PropertyPath(AgentVisualBase.AngleProperty));
                Storyboard.SetTarget(anA, this);
                board.Begin(this);

                SetValue(AngleProperty, value);
            }
        }

        public Point LocationAn
        {
            set 
            {
                if (Location.X > 0 && Location.Y > 0)
                {
                    PointAnimation anL = new PointAnimation(Location, value, new Duration(TimeSpan.FromMilliseconds(500 / agentBase.SpeedRatio)), FillBehavior.Stop);
                    Storyboard board = new Storyboard();
                    board.Children.Add(anL);
                    Storyboard.SetTargetProperty(anL, new PropertyPath(AgentVisualBase.LocationProperty));
                    Storyboard.SetTarget(anL, this);
                    //DoubleAnimation anA = new DoubleAnimation(Angle, Math.Acos((value.X - Location.X) / Math.Sqrt(Math.Pow(value.X - Location.X, 2) + Math.Pow(value.Y - Location.Y, 2))) / Math.PI * 180, new Duration(TimeSpan.FromMilliseconds(500 / agentBase.SpeedRatio)), FillBehavior.Stop);
                    //board.Children.Add(anA);
                    //Storyboard.SetTargetProperty(anA, new PropertyPath(AgentVisualBase.AngleProperty));
                    //Storyboard.SetTarget(anA, this);
                    board.Begin(this);
                }
                //Location = value;
                SetValue(LocationProperty, value);
            }
        }

        static AgentVisualBase()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();
            metadata.DefaultValue = new Point();
            metadata.AffectsRender = true;
            metadata.PropertyChangedCallback += OnLocationPropertyChanged;

            LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(AgentVisualBase), metadata);

            FrameworkPropertyMetadata metadata2 = new FrameworkPropertyMetadata();
            metadata2.DefaultValue = 0.0;
            metadata2.AffectsRender = true;
            metadata2.PropertyChangedCallback += OnAnglePropertyChanged;
            AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(AgentVisualBase), metadata2);
        }

        static void OnLocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as AgentVisualBase).InvalidateVisual();
        }

        static void OnAnglePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as AgentVisualBase).InvalidateVisual();
        }

        #endregion

        public AgentVisualBase(AgentBase agentBase)
        {
            this.agentBase = agentBase;
            if (agentBase is HumanAgent)
            {
                this.Location = new Point(agentBase.GetCell().X, agentBase.GetCell().Y);
            }
            else if (agentBase is BusAgent)
            {
                this.Location = agentBase.GetPosition();
            }
            System.Windows.Controls.ToolTip tip = new System.Windows.Controls.ToolTip();
            tip.Content = string.Format("id:{0} group:{1} location:{2},{3} speed:{4}",agentBase.ID, agentBase.Group,agentBase.GetCell().X,agentBase.GetCell().Y,Math.Round(MapOld.CellSize *3600/agentBase.MaxSpeed,2));
            this.ToolTip = tip;
        }

        internal static Brush GetGroupColor(int id)
        {
            Brush color;
            switch (id)
            {
                case 0:
                    color = Brushes.Magenta;
                    break;
                case 1:
                    color = Brushes.Green;
                    break;
                case 2:
                    color = Brushes.Blue;
                    break;
                case 3:
                    color = Brushes.Yellow;
                    break;
                case 4:
                    color = Brushes.Red;
                    break;
                case 5:
                    color = Brushes.Brown;
                    break;
                default:
                    Random rnd = new Random(id);
                    byte[] rgb = new byte[3];
                    rnd.NextBytes(rgb);
                    color = new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
                    break;
            }
            return color;
        }

        protected abstract override void OnRender(DrawingContext drawingContext);
    }
}
