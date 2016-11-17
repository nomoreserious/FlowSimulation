using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace FlowSimulation.AgentsVisual3D
{
    abstract class AgentVisual3DBase
    {
        //public abstract MeshGeometry3D CreateAgentGeometry(Point3D position, Size3D size);

        //public abstract MeshGeometry3D AddAgentGeometry(Point3D position, Size3D size, MeshGeometry3D mesh);

        protected static MeshGeometry3D CubeModel(Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p5, Point3D p6, Point3D p7, Point3D p8, MeshGeometry3D mesh)
        {
            mesh = CreateTriangle(p1, p3, p4, mesh);
            mesh = CreateTriangle(p1, p2, p3, mesh);

            mesh = CreateTriangle(p1, p5, p6, mesh);
            mesh = CreateTriangle(p1, p6, p2, mesh);

            mesh = CreateTriangle(p1, p4, p8, mesh);
            mesh = CreateTriangle(p1, p8, p5, mesh);

            mesh = CreateTriangle(p7, p6, p5, mesh);
            mesh = CreateTriangle(p7, p5, p8, mesh);

            mesh = CreateTriangle(p7, p2, p6, mesh);
            mesh = CreateTriangle(p7, p3, p2, mesh);

            mesh = CreateTriangle(p7, p8, p4, mesh);
            mesh = CreateTriangle(p7, p4, p3, mesh);

            return mesh;
        }

        protected static MeshGeometry3D CreateTriangle(Point3D p0, Point3D p1, Point3D p2, MeshGeometry3D mesh)
        {
            int index = mesh.Positions.Count;
            Vector3D normal = CalculateNormal(p0, p1, p2);

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(index);
            mesh.TriangleIndices.Add(index + 1);
            mesh.TriangleIndices.Add(index + 2);

            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            return mesh;
        }

        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        public static MeshGeometry3D Add(MeshGeometry3D base_geom, MeshGeometry3D add_geom)
        {
            foreach (var position in add_geom.Positions)
            {
                base_geom.Positions.Add(position);
            }
            foreach (var normal in add_geom.Normals)
            {
                base_geom.Normals.Add(normal);
            }
            int last_index = base_geom.Positions.Count;
            foreach (var index in add_geom.TriangleIndices)
            {
                base_geom.TriangleIndices.Add(last_index + index);
            }
            foreach (var coordinate in add_geom.TextureCoordinates)
            {
                base_geom.TextureCoordinates.Add(coordinate);
            }
            return base_geom;
        }
    }
}
