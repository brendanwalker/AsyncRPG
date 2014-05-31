using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Environment
{
    public struct Vector2d
    {
        public static Vector2d ZERO_VECTOR = new Vector2d();

        public static Vector2d WORLD_RIGHT = new Vector2d(1.0f, 0.0f);
        public static Vector2d WORLD_LEFT = new Vector2d(-1.0f, 0.0f);
        public static Vector2d WORLD_UP = new Vector2d(0.0f, -1.0f);
        public static Vector2d WORLD_DOWN = new Vector2d(0.0f, 1.0f);

        public static Vector2d I = new Vector2d(1F, 0F);
        public static Vector2d J = new Vector2d(0F, 1F);

        public float i;
        public float j;

        public Vector2d(Vector2d v)
        {
            this.i = v.i;
            this.j = v.j;
        }

        public Vector2d(float i, float j)
        {
            this.i = i;
            this.j = j;
        }

        public void Set(float i, float j)
        {
            this.i = i;
            this.j = j;
        }

        public void Copy(Vector2d v)
        {
            this.i = v.i;
            this.j = v.j;
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.i + b.i, a.j + b.j);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.i - b.i, a.j - b.j);
        }

        public static Vector2d operator -(Vector2d v)
        {
            return new Vector2d(-v.i, -v.j);
        }

        public static Vector2d operator *(Vector2d v, float s)
        {
            return new Vector2d(v.i * s, v.j * s);
        }

        public Vector2d ScaleBy(float s)
        {
            i *= s;
            j *= s;

            return this;
        }

        public float Dot(Vector2d v)
        {
            return i * v.i + j * v.j;
        }

        public float Cross(Vector2d v)
        {
            return i * v.j - v.i * j;
        }

        public float MagnitudeSquared()
        {
            return i * i + j * j;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(i * i + j * j);
        }

        public bool IsNonZero
        {
            get { return MagnitudeSquared() > MathConstants.EPSILON_SQUARED; }
        }

        public bool IsAlmostZero
        {
            get { return MagnitudeSquared() <= MathConstants.EPSILON_SQUARED; }
        }

        public float Normalize()
        {
            return NormalizeWithDefault(Vector2d.ZERO_VECTOR);
        }

        public float NormalizeWithDefault(Vector2d defaultVector)
        {
            float length = Magnitude();

            if (length > MathConstants.EPSILON)
            {
                ScaleBy(1.0f / length);
            }
            else
            {
                Copy(defaultVector);
            }

            return length;
        }

        public static Vector2d FromAngle(float radians)
        {
            return new Vector2d((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public float MinComponent()
        {
            return Math.Min(i, j);
        }

        public float MaxComponent()
        {
            return Math.Max(i, j);
        }
    }
}
