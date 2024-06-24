using Lidgren.Network;
using System.IO;
using System;

namespace SWFServer.Data
{
	public struct Vector2w
	{
        public short x;
        public short y;

        public static Vector2w Empty = new Vector2w(-1, -1);
        public static Vector2w One = new Vector2w(1, 1);
        public static Vector2w Zero = new Vector2w(0, 0);

        public Vector2w(short x, short y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2w(int x, int y)
		{
			this.x = (short)x;
			this.y = (short)y;
		}
		public Vector2w(Vector2w pos)
		{
			x = pos.x;
			y = pos.y;
		}

		public static Vector2w operator +(Vector2w v1, Vector2w v2)
		{
			return new Vector2w((short)(v1.x + v2.x), (short)(v1.y + v2.y));
		}
		public static Vector2w operator -(Vector2w v1, Vector2w v2)
		{
			return new Vector2w((short)(v1.x - v2.x), (short)(v1.y - v2.y));
		}
		public static bool operator ==(Vector2w v1, Vector2w v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }
		public static bool operator !=(Vector2w v1, Vector2w v2)
        {
            return v1.x != v2.x || v1.y != v2.y;
        }
		
		public int GetR(Vector2w vec)
		{
			if (Math.Abs(x - vec.x) > Math.Abs(y - vec.y))
			{
				return Math.Abs(x - vec.x);
			}
			return Math.Abs(y - vec.y);
		}
        
		public override int GetHashCode()
		{
			unchecked
			{
				return (x.GetHashCode()*497) ^ y.GetHashCode();
			}
		}

	    public bool Equals(Vector2w vec)
		{
			return vec.x == x && vec.y == y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(Vector2w)) return false;
			return Equals((Vector2w)obj);
		}
        
		//public override string ToString() { return "{\"x\": " + x + ",\"y\": " + y + " }"; }
        //public override string ToString() { return x + "," + y; }

        public void MoveTo1(Vector2w inTarget)
		{
			if (x < inTarget.x)
				x++;
			else if (x > inTarget.x)
				x--;
			if (y < inTarget.y)
				y++;
			else if (y > inTarget.y)
				y--;
		}

        public bool IsZero()
        {
            if (x != 0 || y != 0)
                return false;
            return true;
        }

        public bool IsBeside(Vector2w p)
		{
			if (p.x != x && p.y != y)
				return false;
			if (GetR(p) != 1)
				return false;
			return true;
		}

        public void Normalize()
        {
            if (x != 0)
                x /= Math.Abs(x);
            if (y != 0)
                y /= Math.Abs(y);
        }

        public static Vector2w Read(BinaryReader reader)
        {
            return new Vector2w(reader.ReadInt16(), reader.ReadInt16());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
        }

        public static Vector2w Read(NetIncomingMessage reader)
        {
            return new Vector2w(reader.ReadInt16(), reader.ReadInt16());
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(x);
            writer.Write(y);
        }

        public Vector2w GetDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.up:
                    return new Vector2w(x, y + 1);
                case Direction.down:
                    return new Vector2w(x, y - 1);
                case Direction.right:
                    return new Vector2w(x + 1, y);
                case Direction.left:
                    return new Vector2w(x - 1, y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        public void Swap()
        {
            (x, y) = (y, x);
        }
    }
}
