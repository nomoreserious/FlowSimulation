using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace FlowSimulation.Helpers.Geometry
{
    public static class Geometry3DHelper
    {
        public static void CubeModel(Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7, Point3D p8, ref MeshGeometry3D mesh)
        {
            CreateTriangle(p1, p3, p4, ref mesh);
            CreateTriangle(p1, p2, p3, ref mesh);

            CreateTriangle(p1, p5, p6, ref mesh);
            CreateTriangle(p1, p6, p2, ref mesh);
            
            CreateTriangle(p1, p4, p8, ref mesh);
            CreateTriangle(p1, p8, p5, ref mesh);

            CreateTriangle(p7, p6, p5, ref mesh);
            CreateTriangle(p7, p5, p8, ref mesh);

            CreateTriangle(p7, p2, p6, ref mesh);
            CreateTriangle(p7, p3, p2, ref mesh);

            CreateTriangle(p7, p8, p4, ref mesh);
            CreateTriangle(p7, p4, p3, ref mesh);
        }

        private static void CreateTriangle(Point3D p0, Point3D p1, Point3D p2, ref MeshGeometry3D mesh)
        {
            int index = mesh.Positions.Count;
            Vector3D normal = CalculateNormal(p0, p1, p2);

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(index + 2);
            mesh.TriangleIndices.Add(index + 1);
            mesh.TriangleIndices.Add(index + 0);

            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
        }

        public static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        public static Brush GetColorByGroup(ulong id)
        {
            switch (id)
            {
                case 1:
                    return Brushes.BlueViolet;
                case 2:
                    return Brushes.LimeGreen;
                case 3:
                    return Brushes.Red;
                case 4:
                    return Brushes.DeepSkyBlue;
                case 5:
                    return Brushes.Yellow;
                default:
                    Random rnd = new Random((int)id);
                    byte[] rgb = new byte[3];
                    rnd.NextBytes(rgb);
                    return new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
            }
        }
    }
}
