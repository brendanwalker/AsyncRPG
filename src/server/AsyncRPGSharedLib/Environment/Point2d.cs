using System;
using System.Text;

namespace AsyncRPGSharedLib.Environment
{
    public struct Point2d
    {
        public float x;
        public float y;

        public Point2d(Point2d p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public Point2d(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Point2d other)
        {
            return x == other.x && y == other.y;
        }

        public void Set(Point2d p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public void Set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2d operator +(Point2d p, Vector2d v)
        {
            return new Point2d(p.x + v.i, p.y + v.j);
        }

        public static Point2d operator -(Point2d p, Vector2d v)
        {
            return new Point2d(p.x - v.i, p.y - v.j);
        }

        public static Vector2d operator -(Point2d a, Point2d b)
        {
            return new Vector2d(a.x - b.x, a.y - b.y);
        }

        public static float Distance(Point2d a, Point2d b)
        {
            return (float)Math.Sqrt(Point2d.DistanceSquared(a, b));
        }

        public static float Distance(Point3d a, Point3d b)
        {
            return (float)Math.Sqrt(Point2d.DistanceSquared(a, b));
        }

        public static float DistanceSquared(Point2d a, Point2d b)
        {
            float xDelta = b.x - a.x;
            float yDelta = b.y - a.y;

            return (xDelta * xDelta) + (yDelta * yDelta);
        }

        public static float DistanceSquared(Point3d a, Point3d b)
        {
            float xDelta = b.x - a.x;
            float yDelta = b.y - a.y;

            return (xDelta * xDelta) + (yDelta * yDelta);
        }

        public static Point2d Min(Point2d a, Point2d b)
        {
            return new Point2d(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }

        public static Point2d Max(Point2d a, Point2d b)
        {
            return new Point2d(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }

        public static Point2d Interpolate(Point2d a, Point2d b, float u)
        {
            float inv_u = (1.0f - u);

            return new Point2d(inv_u * a.x + u * b.x, inv_u * a.y + u * b.y);
        }

        public Point2d Offset(float dx, float dy)
        {
            return new Point2d(x + dx, y + dy);
        }

        public Point2d Offset(Vector2d v)
        {
            return new Point2d(x + v.i, y + v.j);
        }

        public Point3d ToPoint3d()
        {
            return new Point3d(x, y, 0.0f);
        }
    }
}
