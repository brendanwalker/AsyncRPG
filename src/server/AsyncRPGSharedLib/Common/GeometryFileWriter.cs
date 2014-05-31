using System;
using System.Collections.Generic;
using System.IO;
using AsyncRPGSharedLib.Environment;

namespace AsyncRPGSharedLib.Common
{
    public class GeometryFileWriter
    {
        class Face
        {
            private int[] vertex_indices;

            public Face(int v0, int v1, int v2)
            {
                vertex_indices = new int[3] { v0, v1, v2 };
            }

            public Face(int v0, int v1, int v2, int v3)
            {
                vertex_indices = new int[4] { v0, v1, v2, v3 };
            }

            public override string ToString()
            {
                if (vertex_indices.Length == 3)
                {
                    return string.Format("f {0} {1} {2}",
                        vertex_indices[0], vertex_indices[1], vertex_indices[2]);
                }
                else
                {
                    return string.Format("f {0} {1} {2} {3}",
                        vertex_indices[0], vertex_indices[1], vertex_indices[2], vertex_indices[3]);
                }
            }
        }

        private List<Point3d> vertices;
        private List<Face> faces;

        public GeometryFileWriter()
        {
            vertices = new List<Point3d>();
            faces = new List<Face>();
        }

        public void AppendAABB(AABB3d aabb)
        {
            Point3d p0 = aabb.Min;
            Point3d p1 = aabb.Max;

            int start = vertices.Count + 1; // Vertex indices are 1-based, not 0-based

            vertices.Add(new Point3d(p0.x, p0.y, p0.z)); // 0
            vertices.Add(new Point3d(p1.x, p0.y, p0.z)); // 1
            vertices.Add(new Point3d(p1.x, p1.y, p0.z)); // 2
            vertices.Add(new Point3d(p0.x, p1.y, p0.z)); // 3
            vertices.Add(new Point3d(p0.x, p0.y, p1.z)); // 4
            vertices.Add(new Point3d(p1.x, p0.y, p1.z)); // 5
            vertices.Add(new Point3d(p1.x, p1.y, p1.z)); // 6
            vertices.Add(new Point3d(p0.x, p1.y, p1.z)); // 7

            faces.Add(new Face(start + 0, start + 1, start + 5, start + 4)); // -Y face
            faces.Add(new Face(start + 1, start + 2, start + 6, start + 5)); // +X face
            faces.Add(new Face(start + 2, start + 3, start + 7, start + 6)); // +Y face
            faces.Add(new Face(start + 3, start + 0, start + 4, start + 7)); // -X face
            faces.Add(new Face(start + 4, start + 5, start + 6, start + 7)); // +Z face
            faces.Add(new Face(start + 0, start + 3, start + 2, start + 1)); // -Z face
        }

        public bool SaveFile(string path, string filename, string header, out string result)
        {
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (Directory.Exists(path))
                {
                    StreamWriter file = new StreamWriter(string.Format("{0}/{1}.obj", path, filename));
                    file.WriteLine("# {0}", header);
                    file.WriteLine("# File Created: {0:MM/dd/yy H:mm:ss zzz}", DateTime.Now);

                    foreach (Point3d vertex in vertices)
                    {
                        file.WriteLine("v {0} {1} {2}", vertex.x, vertex.y, vertex.z);
                    }

                    foreach (Face face in faces)
                    {
                        file.WriteLine(face);
                    }
                }
                else
                {
                    result = string.Format("Can't create/find target directory: {0}", path);
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
                success = false;
            }

            return success;
        }
    }
}
