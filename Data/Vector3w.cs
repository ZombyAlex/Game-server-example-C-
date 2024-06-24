using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace SWFServer.Data
{
    public struct  Vector3w
    {
		public short x;
		public short y;
		public short z;


		public static Vector3w Empty = new Vector3w(-1, -1, -1);
		public static Vector3w One = new Vector3w(1, 1, 1);
		public static Vector3w Zero = new Vector3w(0, 0, 0);
		public static Vector3w Up = new Vector3w(0, 0, 1);
		public static Vector3w Down = new Vector3w(0, 0, -1);

		public Vector3w(short x, short y, short z)
		{
			this.x = x;
			this.y = y;
            this.z = z;
        }

		public Vector3w(int x, int y, int z)
		{
			this.x = (short)x;
			this.y = (short)y;
			this.z = (short)z;
		}
		public Vector3w(Vector3w pos)
		{
			x = pos.x;
			y = pos.y;
			z = pos.z;
		}

		public static Vector3w operator +(Vector3w v1, Vector3w v2)
        {
            return new Vector3w((short) (v1.x + v2.x), (short) (v1.y + v2.y), (short) (v1.z + v2.z));
        }
		public static Vector3w operator -(Vector3w v1, Vector3w v2)
        {
            return new Vector3w((short) (v1.x - v2.x), (short) (v1.y - v2.y), (short) (v1.z - v2.z));
        }
		public static bool operator ==(Vector3w v1, Vector3w v2)
		{
			return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
		}
		public static bool operator !=(Vector3w v1, Vector3w v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

		public int GetR(Vector3w vec)
		{
			if (Math.Abs(x - vec.x) > Math.Abs(y - vec.y))
			{
				return Math.Abs(x - vec.x);
			}
			return Math.Abs(y - vec.y);
		}

        public int GetR3D(Vector3w vec)
        {
            var v = this - vec;
            return Math.Max(Math.Abs(v.x), Math.Max(Math.Abs(v.y), Math.Abs(v.z)));
        }

		public override string ToString()
        {
            return x + "," + y + "," + z;
        }

        public bool IsZero()
        {
            if (x != 0 || y != 0 || z != 0)
                return false;
            return true;
        }


		public void Normalize()
		{
			if (x != 0)
				x /= Math.Abs(x);
			if (y != 0)
				y /= Math.Abs(y);
            if (z != 0)
                z /= Math.Abs(z);
		}

        public static Vector3w Read(BinaryReader reader)
        {
            return new Vector3w(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }

        public static Vector3w Read(NetIncomingMessage reader)
        {
            return new Vector3w(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }

        public Vector2w ToVector2w()
        {
            return new Vector2w(x, y);
        }
    }
}
