using System;

namespace SWFServer.Data
{
    public struct Vector2d
    {
        public double x;
        public double y;
        
        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2d(Vector2d pos)
        {
            this.x = pos.x;
            this.y = pos.y;
        }

        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2d operator *(Vector2d v1, double v)
        {
            return new Vector2d(v1.x * v, v1.y * v);
        }

        public static bool operator ==(Vector2d v1, Vector2d v2)
        {
            if (v1.x == v2.x && v1.y == v2.y)
                return true;
            return false;
        }
        public static bool operator !=(Vector2d v1, Vector2d v2)
        {
            if (v1.x == v2.x && v1.y == v2.y)
                return false;
            return true;
        }
        public void Set(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double GetR(Vector2d v)
        {
            if (Math.Abs(x - v.x) > Math.Abs(y - v.y))
            {
                return Math.Abs(x - v.x);
            }
            return Math.Abs(y - v.y);
        }
        public void Normalize()
        {
            double val = Math.Sqrt(x * x + y * y);
            x = x / val;
            y = y / val;
        }

        public bool IsDistance(Vector2d p, double r)
        {
            Vector2d n = this - p;
            double v = n.x * n.x + n.y * n.y;
            return v < r * r;
        }

        public bool IsRange(Vector2d p, double r)
        {
            return Math.Max(Math.Abs(x - p.x), Math.Abs(y - p.y)) < r;
        }

        public double Lenght()
        {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
