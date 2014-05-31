using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Environment
{
    public class AABB2d
    {
        private Point2d m_pMin;
        private Point2d m_pMax;

        public AABB2d()
        {
            m_pMin = new Point2d(MathConstants.REAL_MAX, MathConstants.REAL_MAX);
            m_pMax = new Point2d(MathConstants.REAL_MIN, MathConstants.REAL_MIN);
        }

        public AABB2d(Point2d pMin, Point2d pMax)
        {
            this.m_pMin = new Point2d(pMin);
            this.m_pMax = new Point2d(pMax);
        }

        public AABB2d(AABB2d other)
        {
            this.m_pMin = new Point2d(other.m_pMin);
            this.m_pMax = new Point2d(other.m_pMax);
        }

        public bool Equals(AABB2d other)
        {
            return m_pMin.Equals(other.m_pMin) && m_pMax.Equals(other.m_pMax);
        }

        public Point2d Min
        {
            get { return m_pMin; }
            set { m_pMin = value; }
        }

        public Point2d Max
        {
            get { return m_pMax; }
            set { m_pMax = value; }
        }

        public Point2d Center
        {
            get { return Point2d.Interpolate(m_pMin, m_pMax, 0.5f); }
        }

        public float Width
        {
            get { return m_pMax.x - m_pMin.x; }
        }

        public float Height
        {
            get { return m_pMax.y - m_pMin.y; }
        }
        
        public Vector2d Extents
        {
            get { return m_pMax - m_pMin; }
        }

        public void SetBounds(float x0, float y0, float x1, float y1)
        {
            this.m_pMin.Set(x0, y0);
            this.m_pMax.Set(x1, y1);
        }

        public void SetPointBounds(Point2d p0, Point2d p1)
        {
            m_pMin = new Point2d(p0);
            m_pMax = new Point2d(p1);
        }

        public void EnclosePoint(Point2d point)
        {
            m_pMin.x = Math.Min(m_pMin.x, point.x);
            m_pMin.y = Math.Min(m_pMin.y, point.y);

            m_pMax.x = Math.Max(m_pMax.x, point.x);
            m_pMax.y = Math.Max(m_pMax.y, point.y);
        }

        public AABB2d EncloseAABB(AABB2d other)
        {
            return new AABB2d(Point2d.Min(m_pMin, other.m_pMin), Point2d.Max(m_pMax, other.m_pMax));
        }

        public bool ContainsPoint(Point2d p)
        {
            return p.x >= m_pMin.x && p.y >= m_pMin.y &&
                    p.x <= m_pMax.x && p.y <= m_pMax.y;
        }

        public bool ClipRay(
            Point2d rayOrigin,
            Vector2d rayDirection,
            out float clipMinT,
            out float clipMaxT)
        {
            bool intersects = false;
            Vector2d tMin = new Vector2d();
            Vector2d tMax = new Vector2d();

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

            // Ray only intersects AABB if minT is before maxT and maxT is non-negative
            clipMinT = tMin.MaxComponent();
            clipMaxT = tMax.MinComponent();
            intersects = clipMinT < clipMaxT && clipMaxT >= 0;

            return intersects;
        }

        public Point2d ClipPoint(Point2d p)
        {
            return new Point2d(
                Math.Min(Math.Max(p.x, m_pMin.x), m_pMax.x),
                Math.Min(Math.Max(p.y, m_pMin.y), m_pMax.y));
        }

        public void Move(Vector2d v)
        {
            m_pMin += v;
            m_pMax += v;
        }

        public AABB2d ScaleAboutCenter(float scale)
        {
            float u = Math.Max(scale * 0.5f + 0.5f, 0.5f);
            Point2d newP1 = Point2d.Interpolate(m_pMax, m_pMin, u);
            Point2d newP0 = Point2d.Interpolate(m_pMin, m_pMax, u);

            return new AABB2d(newP0, newP1);
        }
    }
}
