using System;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows;

namespace FlowSimulation.AgentsVisual3D
{
    class BusAgentVisual3D : AgentVisual3DBase
    {
        internal static MeshGeometry3D AddAgentGeometry(Point3D position, Size3D size, MeshGeometry3D mesh)
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

        internal static ModelVisual3D GetModel(Point3D position, Size3D size, double angle)
        {

            Model3DGroup group = LoadModel();
            Transform3DGroup trgr = new Transform3DGroup();
            trgr.Children.Add(new ScaleTransform3D(size.X, size.Y, size.Z));
            trgr.Children.Add(new TranslateTransform3D(position.X, position.Y, position.Z));
            trgr.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 3, 0), angle)));
            group.Transform = trgr;
            return new ModelVisual3D()
            {
                Content = group
            };
        }

        private static Model3DGroup LoadModel()
        {
            Model3DGroup group = null;
            try
            {
                StreamReader mysr = new StreamReader(Application.ResourceAssembly.Location + @"\ObjectModels\bus.xaml");
                DependencyObject rootObject = System.Windows.Markup.XamlReader.Load(mysr.BaseStream) as DependencyObject;
                if (rootObject is Model3DGroup)
                {
                    group = rootObject as Model3DGroup;
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
            }
            return group;
        }
    }
}
