using System;
using System.Text;

namespace AsyncRPGSharedLib.Environment
{
    [Serializable]
    public struct Point3d
    {
        public float x;
        public float y;
        public float z;

        public Point3d(Point3d p)
        {
            this.x = p.x;
            this.y = p.y;
            this.z = p.z;
        }

        public Point3d(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool Equals(Point3d other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public void Set(Point3d p)
        {
            this.x = p.x;
            this.y = p.y;
            this.z = p.z;
        }

        public void Set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Point3d operator +(Point3d p, Vector3d v)
        {
            return new Point3d(p.x + v.i, p.y + v.j, p.z + v.k);
        }

        public static Point3d operator -(Point3d p, Vector3d v)
        {
            return new Point3d(p.x - v.i, p.y - v.j, p.z - v.k);
        }

        public static Vector3d operator -(Point3d a, Point3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static float Distance(Point3d a, Point3d b)
        {
            return (float)Math.Sqrt(Point3d.DistanceSquared(a, b));
        }

        public static float DistanceSquared(Point3d a, Point3d b)
        {
            float xDelta = b.x - a.x;
            float yDelta = b.y - a.y;
            float zDelta = b.z - a.z;

            return (xDelta * xDelta) + (yDelta * yDelta) + (zDelta * zDelta);
        }

        public static Point3d Min(Point3d a, Point3d b)
        {
            return new Point3d(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        public static Point3d Max(Point3d a, Point3d b)
        {
            return new Point3d(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }

        public static Point3d Interpolate(Point3d a, Point3d b, float u)
        {
            float inv_u = (1.0f - u);

            return new Point3d(inv_u * a.x + u * b.x, inv_u * a.y + u * b.y, inv_u * a.z + u * b.z);
        }

        public Point3d Offset(float dx, float dy, float dz)
        {
            return new Point3d(x + dx, y + dy, z + dz);
        }

        public Point3d Offset(Vector3d v)
        {
            return new Point3d(x + v.i, y + v.j, z + v.k);
        }

        public Point2d ToPoint2d()
        {
            return new Point2d(x, y);
        }
    }
}
