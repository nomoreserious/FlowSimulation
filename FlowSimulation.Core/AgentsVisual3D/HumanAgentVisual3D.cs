using System;
using System.Windows.Media.Media3D;

namespace FlowSimulation.AgentsVisual3D
{
    class HumanAgentVisual3D : AgentVisual3DBase
    {
        //public override System.Windows.Media.Media3D.MeshGeometry3D CreateAgentGeometry(System.Windows.Media.Media3D.Point3D position, System.Windows.Media.Media3D.Size3D size)
        //{
        //    Point3D p1 = new Point3D(position.X + 0, position.Y + 0, position.Z + 0);
        //    Point3D p2 = new Point3D(position.X + size.X, position.Y + 0, position.Z + 0);
        //    Point3D p3 = new Point3D(position.X + size.X, position.Y + 0, position.Z + size.Z);
        //    Point3D p4 = new Point3D(position.X + 0, position.Y + 0, position.Z + size.Z);
        //    Point3D p5 = new Point3D(position.X + 0, position.Y + size.Y, position.Z + 0);
        //    Point3D p6 = new Point3D(position.X + size.X, position.Y + size.Y, position.Z + 0);
        //    Point3D p7 = new Point3D(position.X + size.X, position.Y + size.Y, position.Z + size.Z);
        //    Point3D p8 = new Point3D(position.X + 0, position.Y + size.Y, position.Z + size.Z);

        //    return CubeModel(p1, p2, p3, p4, p5, p6, p7, p8,);
        //}

        public static MeshGeometry3D AddAgentGeometry(System.Windows.Media.Media3D.Point3D position, System.Windows.Media.Media3D.Size3D size, System.Windows.Media.Media3D.MeshGeometry3D mesh)
        {
            Point3D p1 = new Point3D(position.X + 0, position.Y + 0, position.Z + 0);
            Point3D p2 = new Point3D(position.X + size.X, position.Y + 0, position.Z + 0);
            Point3D p3 = new Point3D(position.X + size.X, position.Y + 0, position.Z + size.Z);
            Point3D p4 = new Point3D(position.X + 0, position.Y + 0, position.Z + size.Z);
            Point3D p5 = new Point3D(position.X + 0, position.Y + size.Y, position.Z + 0);
            Point3D p6 = new Point3D(position.X + size.X, position.Y + size.Y, position.Z + 0);
            Point3D p7 = new Point3D(position.X + size.X, position.Y + size.Y, position.Z + size.Z);
            Point3D p8 = new Point3D(position.X + 0, position.Y + size.Y, position.Z + size.Z);

            return CubeModel(p1, p2, p3, p4, p5, p6, p7, p8, mesh);
        }
    }
}
