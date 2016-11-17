using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows;

namespace FlowSimulation.AgentsVisual3D
{
    class TrainAgentVisual3D : AgentVisual3DBase
    {
        protected static Model3DGroup DefaultModel;

        static TrainAgentVisual3D()
        {
            try
            {
                StreamReader mysr = new StreamReader(Application.ResourceAssembly.Location.Remove(Application.ResourceAssembly.Location.LastIndexOf(@"\")) + @"\ObjectModels\train.xaml");
                DependencyObject rootObject = System.Windows.Markup.XamlReader.Load(mysr.BaseStream) as DependencyObject;
                if (rootObject is Model3DGroup)
                {
                    DefaultModel = (Model3DGroup)rootObject;
                }
            }
            catch
            {
                GeometryModel3D model = new GeometryModel3D();
                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(new Point3D(-0.5, 0, 0.5));
                mesh.Positions.Add(new Point3D(0.5, 0, 0.5));
                mesh.Positions.Add(new Point3D(0.5, 0, -0.5));
                mesh.Positions.Add(new Point3D(-0.5, 0, -0.5));
                mesh.Positions.Add(new Point3D(-0.5, 1, 0.5));
                mesh.Positions.Add(new Point3D(0.5, 1, 0.5));
                mesh.Positions.Add(new Point3D(0.5, 1, -0.5));
                mesh.Positions.Add(new Point3D(-0.5, 1, -0.5));
                mesh.TriangleIndices = new System.Windows.Media.Int32Collection(new int[] { 0, 1, 5, 0, 5, 4, 1, 2, 6, 1, 6, 5, 2, 3, 7, 2, 7, 6, 0, 4, 7, 0, 7, 3, 4, 5, 6, 4, 6, 7 });
                mesh.TextureCoordinates.Add(new Point(0, 1));
                mesh.TextureCoordinates.Add(new Point(1, 1));
                mesh.TextureCoordinates.Add(new Point(1, 0));
                mesh.TextureCoordinates.Add(new Point(0, 0));
                mesh.TextureCoordinates.Add(new Point(0.10135, 0.66666666));
                mesh.TextureCoordinates.Add(new Point(0.89865, 0.66666666));
                mesh.TextureCoordinates.Add(new Point(0.89865, 0.33333333));
                mesh.TextureCoordinates.Add(new Point(0.10135, 0.33333333));
                model.Geometry = mesh;
                model.Material = new DiffuseMaterial(System.Windows.Media.Brushes.DarkRed);
                DefaultModel = new Model3DGroup();
                DefaultModel.Children.Add(model);
            }
        }
        internal static MeshGeometry3D AddAgentGeometry(System.Windows.Media.Media3D.Point3D position, System.Windows.Media.Media3D.Size3D size, System.Windows.Media.Media3D.MeshGeometry3D mesh)
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
            Model3DGroup group = DefaultModel.Clone();
            Transform3DGroup trgr = new Transform3DGroup();
            trgr.Children.Add(new ScaleTransform3D(size.X, size.Y, size.Z));
            trgr.Children.Add(new TranslateTransform3D(position.X, position.Y, position.Z));
            trgr.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), -angle), position));
            group.Transform = trgr;
            return new ModelVisual3D()
            {
                Content = group
            };
        }
    }
}
