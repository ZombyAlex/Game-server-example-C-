
namespace SWFServer.Data
{
    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3f(Vector3f pos)
        {
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }

        public static Vector3f operator +(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static Vector3f operator -(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3f operator *(Vector3f v1, float v)
        {
            return new Vector3f(v1.x * v, v1.y * v, v1.z * v);
        }

        public static bool operator ==(Vector3f v1, Vector3f v2)
        {
            if (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z)
                return true;
            return false;
        }
        public static bool operator !=(Vector3f v1, Vector3f v2)
        {
            if (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z)
                return false;
            return true;
        }
    }
}
