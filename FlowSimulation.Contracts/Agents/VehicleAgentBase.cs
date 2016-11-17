using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlowSimulation.Enviroment;
using System.Windows.Media;
using FlowSimulation.Helpers.Graph;
using FlowSimulation.Contracts.Services;
using System.Windows.Media.Media3D;

namespace FlowSimulation.Contracts.Agents
{
    public abstract class VehicleAgentBase : AgentBase
    {
        protected bool _go;

        public Graph<WayPoint, string> RoadGraph { get; set; }

        public int MaxCapasity { get; set; }
        public int CurrentAgentCount { get; set; }
        public double InputFactor { get; set; }
        public double OutputFactor { get; set; }
        public bool ReadyToIOOperation { get; set; }
        public double Angle { get; protected set; }

        public VehicleAgentBase(Map map, IEnumerable<AgentServiceBase> services)
            : base(map, services)
        { Angle = 999; }

        public virtual void Go()
        {
            _go = true;
        }

        public virtual Model3DGroup GetModel()
        {
            MeshGeometry3D meshVehicle = new MeshGeometry3D();
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(1, 0, 0);
            Point3D p3 = new Point3D(1, 0, 1);
            Point3D p4 = new Point3D(0, 0, 1);
            Point3D p5 = new Point3D(0, 1, 0);
            Point3D p6 = new Point3D(1, 1, 0);
            Point3D p7 = new Point3D(1, 1, 1);
            Point3D p8 = new Point3D(0, 1, 1);
            Helpers.Geometry.Geometry3DHelper.CubeModel(p1, p2, p3, p4, p5, p6, p7, p8, ref meshVehicle);

            GeometryModel3D geom = new GeometryModel3D(meshVehicle, new DiffuseMaterial(Brushes.OrangeRed));

            Model3DGroup group = new Model3DGroup();
            //geom.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle));
            group.Children.Add(geom);

            Transform3DGroup trgr = new Transform3DGroup();
            trgr.Children.Add(new ScaleTransform3D(Size.Y, Size.X, Size.Z));
            trgr.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 3), -Angle)));
            trgr.Children.Add(new TranslateTransform3D(Position.Y, Position.X, 0));
            group.Transform = trgr;
            return group;
        }
    }
}
