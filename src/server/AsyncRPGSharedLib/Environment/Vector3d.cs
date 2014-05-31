using System;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Environment
{
    public struct Vector3d
    {
        public static Vector3d ZERO_VECTOR = new Vector3d();

		public static Vector3d WORLD_RIGHT = new Vector3d(1.0f, 0.0f, 0.0f);
		public static Vector3d WORLD_LEFT = new Vector3d(-1.0f, 0.0f, 0.0f);
		public static Vector3d WORLD_UP = new Vector3d(0.0f, -1.0f, 0.0f);
		public static Vector3d WORLD_DOWN = new Vector3d(0.0f, 1.0f, 0.0f);

        public static Vector3d I = new Vector3d(1F, 0F, 0F);
        public static Vector3d J = new Vector3d(0F, 1F, 0F);
        public static Vector3d K = new Vector3d(0F, 0F, 1F);

        public float i;
        public float j;
        public float k;

        public Vector3d(Vector3d v)
        {
            this.i = v.i;
            this.j = v.j;
            this.k = v.k;
        }
		
        public Vector3d(float i, float j, float k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public void Set(float i, float j, float k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public void Copy(Vector3d v)
        {
            this.i = v.i;
            this.j = v.j;
            this.k = v.k;
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.i + b.i, a.j + b.j, a.k + b.k);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.i - b.i, a.j - b.j, a.k - b.k);
        }

        public static Vector3d operator -(Vector3d v)
        {
            return new Vector3d(-v.i, -v.j, -v.k);
        }

        public static Vector3d operator *(Vector3d v, float s)
        {
            return new Vector3d(v.i * s, v.j * s, v.k * s);
        }

        public Vector3d ScaleBy(float s)
        {
            i *= s;
            j *= s;
            k *= s;

            return this;
        }

        public float Dot(Vector3d v)
        {
            return i * v.i + j * v.j + k * v.k;
        }

        public Vector3d Cross(Vector3d v)
        {
            return new Vector3d(j * v.k - v.j * k, v.i * k - i * v.k, i * v.j - v.i * j);
        }

        public float Cross2d(Vector3d v)
        {
            return i * v.j - v.i * j;
        }

        public float MagnitudeSquared()
        {
            return i * i + j * j + k * k;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(i * i + j * j + k * k);
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
            return NormalizeWithDefault(Vector3d.ZERO_VECTOR);
        }

        public float NormalizeWithDefault(Vector3d defaultVector)
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

        public static Vector3d FromAngle(float radians)
        {
            return new Vector3d((float)Math.Cos(radians), (float)Math.Sin(radians), 0.0f);
        }

        public Vector2d ToVector2d()
        {
            return new Vector2d(i, j);
        }

        public float MinComponent()
        {
            return Math.Min(i, Math.Min(j, k));
        }

        public float MaxComponent()
        {
            return Math.Max(i, Math.Max(j, k));
        }
    }
}
