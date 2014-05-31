using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Environment
{
    public class AABB3d
    {
        private Point3d m_pMin;
        private Point3d m_pMax;

        public AABB3d()
        {
            m_pMin = new Point3d(MathConstants.REAL_MAX, MathConstants.REAL_MAX, MathConstants.REAL_MAX);
            m_pMax = new Point3d(MathConstants.REAL_MIN, MathConstants.REAL_MIN, MathConstants.REAL_MIN);
        }

        public AABB3d(Point3d p0, Point3d p1)
        {
            this.m_pMin = new Point3d(p0);
            this.m_pMax = new Point3d(p1);
        }

        public AABB3d(AABB3d other)
        {
            this.m_pMin = new Point3d(other.m_pMin);
            this.m_pMax = new Point3d(other.m_pMax);
        }

        public bool Equals(AABB3d other)
        {
            return m_pMin.Equals(other.m_pMin) && m_pMax.Equals(other.m_pMax);
        }

        public Point3d Min
        {
            get { return m_pMin; }
            set { m_pMin = value; }
        }

        public Point3d Max
        {
            get { return m_pMax; }
            set { m_pMax = value; }
        }

        public Point3d Center
        {
            get { return Point3d.Interpolate(m_pMin, m_pMax, 0.5f); }
        }

        public Vector3d Extents
        {
            get { return m_pMax - m_pMin; }
        }

        public float XWidth
        {
            get { return m_pMax.x - m_pMin.x; }
        }

        public float YWidth
        {
            get { return m_pMax.y - m_pMin.y; }
        }

        public float ZWidth
        {
            get { return m_pMax.z - m_pMin.z; }
        }

        public void SetBounds3d(Point3d p0, Point3d p1)
        {
            this.m_pMin = p0;
            this.m_pMax = p1;
        }

        public void SetBounds2d(float x0, float y0, float x1, float y1)
        {
            this.m_pMin.Set(x0, y0, 0.0f);
			this.m_pMax.Set(x1, y1, 0.0f);
        }

        public void SetPointBounds(Point3d p0, Point3d p1)
        {
            m_pMin = new Point3d(p0);
            m_pMax = new Point3d(p1);
        }

        public void SetPointBounds2d(Point2d p0, Point2d p1)
        {
            m_pMin = new Point3d(p0.x, p0.y, 0.0f);
            m_pMax = new Point3d(p1.x, p1.y, 0.0f);
        }

        public void EnclosePoint(Point3d point)
        {
            m_pMin.x = Math.Min(m_pMin.x, point.x);
            m_pMin.y = Math.Min(m_pMin.y, point.y);
            m_pMin.z = Math.Min(m_pMin.z, point.z);

            m_pMax.x = Math.Max(m_pMax.x, point.x);
            m_pMax.y = Math.Max(m_pMax.y, point.y);
            m_pMax.z = Math.Max(m_pMax.z, point.z);
        }

        public AABB3d EncloseAABB(AABB3d other)
        {
            return new AABB3d(Point3d.Min(m_pMin, other.m_pMin), Point3d.Max(m_pMax, other.m_pMax));
        }

        public bool ContainsPoint2d(Point2d p)
        {
            return p.x >= m_pMin.x && p.y >= m_pMin.y &&
                    p.x <= m_pMax.x && p.y <= m_pMax.y;
        }

        public bool ContainsPoint(Point3d p)
        {
            return p.x >= m_pMin.x && p.y >= m_pMin.y && p.z >= m_pMin.z &&
                    p.x <= m_pMax.x && p.y <= m_pMax.y && p.z <= m_pMax.z;
        }

        public bool ClipRay(
            Point3d rayOrigin,
            Vector3d rayDirection,
            out float clipMinT,
            out float clipMaxT)
        {
            bool intersects = false;
            Vector3d tMin = new Vector3d();
            Vector3d tMax = new Vector3d();

            // Compute the ray intersection times along the x-axis
            if (rayDirection.i > MathConstants.EPSILON)
            {
                // ray has positive x component
                tMin.i = (m_pMin.x - rayOrigin.x) / rayDirection.i;
                tMax.i = (m_pMax.x - rayOrigin.x) / rayDirection.i;
            }
            else if (rayDirection.i < -MathConstants.EPSILON)
            {
                // ray has negative x component
                tMin.i = (m_pMax.x - rayOrigin.x) / rayDirection.i;
                tMax.i = (m_pMin.x - rayOrigin.x) / rayDirection.i;
            }
            else
            {
                // Ray has no x component (parallel to x-axis)
                tMin.i = MathConstants.REAL_MIN;
                tMax.i = MathConstants.REAL_MAX;
            }

            // Compute the ray intersection times along the y-axis
            if (rayDirection.j > MathConstants.EPSILON)
            {
                // ray has positive y component
                tMin.j = (m_pMin.y - rayOrigin.y) / rayDirection.j;
                tMax.j = (m_pMax.y - rayOrigin.y) / rayDirection.j;
            }
            else if (rayDirection.j < -MathConstants.EPSILON)
            {
                // ray has negative y component
                tMin.j = (m_pMax.y - rayOrigin.y) / rayDirection.j;
                tMax.j = (m_pMin.y - rayOrigin.y) / rayDirection.j;
            }
            else
            {
                // Ray has no y component (parallel to y-axis)
                tMin.j = MathConstants.REAL_MIN;
                tMax.j = MathConstants.REAL_MAX;
            }

            // Compute the ray intersection times along the z-axis
            if (rayDirection.k > MathConstants.EPSILON)
            {
                // ray has positive z component
                tMin.k = (m_pMin.z - rayOrigin.z) / rayDirection.k;
                tMax.k = (m_pMax.z - rayOrigin.z) / rayDirection.k;
            }
            else if (rayDirection.k < -MathConstants.EPSILON)
            {
                // ray has negative z component
                tMin.k = (m_pMax.z - rayOrigin.z) / rayDirection.k;
                tMax.k = (m_pMin.z - rayOrigin.z) / rayDirection.k;
            }
            else
            {
                // Ray has no z component (parallel to z-axis)
                tMin.k = MathConstants.REAL_MIN;
                tMax.k = MathConstants.REAL_MAX;
            }

            // Ray only intersects AABB if minT is before maxT and maxT is non-negative
            clipMinT = tMin.MaxComponent();
            clipMaxT = tMax.MinComponent();
            intersects = clipMinT < clipMaxT && clipMaxT >= 0;

            return intersects;
        }

        public Point3d ClipPoint(Point3d p)
        {
            return new Point3d(
                Math.Min(Math.Max(p.x, m_pMin.x), m_pMax.x),
                Math.Min(Math.Max(p.y, m_pMin.y), m_pMax.y),
                Math.Min(Math.Max(p.z, m_pMin.z), m_pMax.z));
        }

        public void Move(Vector3d v)
        {
            m_pMin += v;
            m_pMax += v;
        }

        public AABB3d ScaleAboutCenter(float scale)
        {
            float u = Math.Max(scale * 0.5f + 0.5f, 0.5f);
            Point3d newP1 = Point3d.Interpolate(m_pMax, m_pMin, u);
            Point3d newP0 = Point3d.Interpolate(m_pMin, m_pMax, u);

            return new AABB3d(newP0, newP1);
        }
    }
}
