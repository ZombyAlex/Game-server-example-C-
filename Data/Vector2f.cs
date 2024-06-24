using Lidgren.Network;
using System.IO;
using System;

namespace SWFServer.Data
{
    public struct Vector2f
    {
        public float x;
        public float y;

        public static Vector2f Empty = new Vector2f(-1, -1);
        public static Vector2f One = new Vector2f(1, 1);
        public static Vector2f Zero = new Vector2f(0, 0);
        public static Vector2f Up = new Vector2f(0, 1);
        public static Vector2f Down = new Vector2f(0, -1);
        public static Vector2f Left = new Vector2f(-1, 0);
        public static Vector2f Right = new Vector2f(1, 0);

        public Vector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2f(Vector2f pos)
        {
            this.x = pos.x;
            this.y = pos.y;
        }

        public static Vector2f operator +(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2f operator -(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2f operator *(Vector2f v1, float v)
        {
            return new Vector2f(v1.x * v, v1.y * v);
        }

        public static Vector2f operator /(Vector2f v1, float v)
        {
            return new Vector2f(v1.x / v, v1.y / v);
        }

        public static bool operator ==(Vector2f v1, Vector2f v2)
        {
            if (v1.x == v2.x && v1.y == v2.y)
                return true;
            return false;
        }
        public static bool operator !=(Vector2f v1, Vector2f v2)
        {
            if (v1.x == v2.x && v1.y == v2.y)
                return false;
            return true;
        }

        public static Vector2f Read(NetIncomingMessage reader)
        {
            return new Vector2f(reader.ReadSingle(), reader.ReadSingle());
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(x);
            writer.Write(y);
        }

        public static Vector2f Read(BinaryReader reader)
        {
            return new Vector2f(reader.ReadSingle(), reader.ReadSingle());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
        }

        public float Length()
        {
            return (float) Math.Sqrt(x * x + y * y);
        }

        public void Normalize()
        {
            float l = Length();

            if (l > 0)
            {
                x /= l;
                y /= l;
            }
        }

        public void Rotate(float degrees)
        {
            float rad = (float) ((Math.PI / 180f) * degrees);

            float sin = (float) Math.Sin(rad);
            float cos = (float) Math.Cos(rad);

            float tx = x;
            float ty = y;
            x = (cos * tx) - (sin * ty);
            y = (sin * tx) + (cos * ty);
        }

        public bool IsDistance(Vector2f p, double r)
        {
            Vector2f n = this - p;
            double v = n.x * n.x + n.y * n.y;
            return v < r * r;
        }

        public static float Angle(Vector2f from, Vector2f to)
        {
            float num = (float)Math.Sqrt((double)from.sqrMagnitude * (double)to.sqrMagnitude);
            if ((double)num < 1.00000000362749E-15)
                return 0.0f;
            return (float)Math.Acos((double)Util.Clamp(Vector2f.Dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }

        public float sqrMagnitude
        {
            get { return (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y); }
        }
        public static float Dot(Vector2f lhs, Vector2f rhs)
        {
            return (float)((double)lhs.x * (double)rhs.x + (double)lhs.y * (double)rhs.y);
        }

        public static float SignedAngle(Vector2f from, Vector2f to)
        {
            return Vector2f.Angle(from, to) * Math.Sign((float)((double)from.x * (double)to.y - (double)from.y * (double)to.x));
        }

        public static float Distance(Vector2f p1, Vector2f p2)
        {
            Vector2f l = p1 - p2;
            return l.Length();
        }
    }
}