using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using FlowSimulation.Contracts.Services;
using FlowSimulation.Enviroment;

namespace FlowSimulation.Contracts.Agents
{
    public abstract class AgentBase: IAgent 
    {
        public readonly ulong Id = Generator.GetAgentId();

        protected IEnumerable<AgentServiceBase> _services;
        protected Map _map;

        protected double _maxSpeed;
        protected double _acceleration;
        protected double _deceleration;

        protected Point? _direction;

        public ulong GroupId { get; set; }
        public Point Position { get; set; }
        public int LayerId { get; set; }
        public List<WayPoint> RouteList { get; set; }
        public Size3D Size { get;  set; }
        public double CurrentSpeed { get;  set; }

        public double Weigth { get { return Size.X * Size.Y / Enviroment.Constants.CELL_VOLUME; } }

        public AgentBase(Map map, IEnumerable<AgentServiceBase> services)
        {
            _map = map;
            _services = services;
            RouteList = new List<WayPoint>();
        }

        public abstract void DoStep(double msInterval);

        public abstract void DoStepAsync(double msInterval);
        protected Shape _shape;
        public virtual Shape GetAgentShape()
        {
            if (_shape == null)
            {
                _shape = new System.Windows.Shapes.Ellipse()
                {
                    Width = Size.X * 2.5,
                    Height = Size.Y * 2.5,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 0.1
                };
            }
            return _shape;
        }

        public virtual void AddAgentGeometry(double zCoordinate, ref MeshGeometry3D baseGeometry)
        {
            var scale = Enviroment.Constants.CELL_SIZE;
            Point3D p1 = new Point3D(Position.Y * scale, Position.X * scale, zCoordinate);
            Point3D p2 = new Point3D(Position.Y * scale + Size.Y, Position.X * scale, zCoordinate);
            Point3D p3 = new Point3D(Position.Y * scale + Size.Y, Position.X * scale + Size.X, zCoordinate);
            Point3D p4 = new Point3D(Position.Y * scale, Position.X * scale + Size.X, zCoordinate);

            Point3D p5 = new Point3D(Position.Y * scale, Position.X * scale, zCoordinate + Size.Z);
            Point3D p6 = new Point3D(Position.Y * scale + Size.Y, Position.X * scale, zCoordinate + Size.Z);
            Point3D p7 = new Point3D(Position.Y * scale + Size.Y, Position.X * scale + Size.X, zCoordinate + Size.Z);
            Point3D p8 = new Point3D(Position.Y * scale, Position.X * scale + Size.X, zCoordinate + Size.Z);


            //Point3D p1 = new Point3D(Position.X * scale, Position.Y * scale, layerHeigth);
            //Point3D p2 = new Point3D(Position.X * scale + Size.X, Position.Y * scale, layerHeigth);
            //Point3D p3 = new Point3D(Position.X * scale + Size.X, Position.Y * scale + Size.Y, layerHeigth);
            //Point3D p4 = new Point3D(Position.X * scale, Position.Y * scale + Size.Y, layerHeigth);

            //Point3D p5 = new Point3D(Position.X * scale, Position.Y * scale, layerHeigth + Size.Z);
            //Point3D p6 = new Point3D(Position.X * scale + Size.X, Position.Y * scale, layerHeigth + Size.Z);
            //Point3D p7 = new Point3D(Position.X * scale + Size.X, Position.Y * scale + Size.Y, layerHeigth + Size.Z);
            //Point3D p8 = new Point3D(Position.X * scale, Position.Y * scale + Size.Y, layerHeigth + Size.Z);
            Helpers.Geometry.Geometry3DHelper.CubeModel(p1, p2, p3, p4, p5, p6, p7, p8, ref baseGeometry);
        }

        public abstract void Initialize(Dictionary<string, object> settings);
        //{
        //    ////Position = (Point)settings["startPosition"];
        //    //RouteList = new List<WayPoint>((List<WayPoint>)settings["checkPoints"]);
        //    //Size = (Size3D)settings["size"];
        //    //Random rand = new Random();
        //    //_maxSpeed = 1000 * Constants.CELL_SIZE * rand.NextDouble() / Convert.ToDouble(settings["maxSpeed"]);
        //    //_acceleration = Convert.ToDouble(settings["acceleration"]);
        //    //_deceleration = Convert.ToDouble(settings["deceleration"]);
        //    //_direction = null;
        //    //RouteList = new List<WayPoint>((List<WayPoint>)settings["checkPoints"]);
        //    Size = new Size3D(0.5, 1.8, 0.3);
        //    Random rand = new Random();
        //    _maxSpeed = 1000 * Constants.CELL_SIZE * rand.NextDouble() / 2.0;
        //    _acceleration = 0;
        //    _deceleration = 0;
        //    _direction = null;
        //}
    }
}
