using System;

namespace SWFServer.Data
{
    public class Rnd
    {
        private Random rnd = new Random();

        public float Range(float minValue, float maxValue)
        {
            return (float)((rnd.NextDouble() * (maxValue - minValue)) + minValue);
        }

        public int Range(int minValue, int maxValue)
        {
            return rnd.Next(minValue, maxValue);
        }

        public Vector2f Range(float minX, float minY, float maxX, float maxY)
        {
            return new Vector2f(Range(minX, maxX), Range(minY, maxY));
        }

        public Vector2f RangeVector2(float min, float max)
        {
            return new Vector2f(Range(min, max), Range(min, max));
        }

        public Vector2w Range(Vector2w min, Vector2w max)
        {
            return new Vector2w(Range(min.x, max.x), Range(min.y, max.y));
        }

        public Vector3w Range(Vector3w min, Vector3w max)
        {
            return new Vector3w(Range(min.x, max.x), Range(min.y, max.y), Range(min.z, max.z));
        }

        public Vector2f RangeOrbit(float min, float max)
        {
            float v = Range(min, max);
            double angle = Range(0f,(float) (Math.PI * 2));
            double x = v * Math.Cos(angle);
            double y = v * Math.Sin(angle);
            return new Vector2f((float) x, (float) y);
        }

        public int RndCount(int percent)
        {
            int c = 0;

            while (Range(0, 100) < percent)
            {
                c++;
            }

            return c;
        }
    }
}
