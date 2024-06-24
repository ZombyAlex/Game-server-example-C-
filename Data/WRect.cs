using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace SWFServer.Data
{
	public struct WRect
	{
        public short x, y, w, h;
		
		public bool Equals(WRect other)
		{
			return x == other.x && y == other.y && w == other.w && h == other.h;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is WRect && Equals((WRect) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = x.GetHashCode();
				hashCode = (hashCode*397) ^ y.GetHashCode();
				hashCode = (hashCode*397) ^ w.GetHashCode();
				hashCode = (hashCode*397) ^ h.GetHashCode();
				return hashCode;
			}
		}

        public WRect(short x, short y, short w, short h)
		{
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;
		}

		public WRect(int x, int y, int w, int h)
		{
			this.x = (short) x;
			this.y = (short) y;
			this.w = (short) w;
			this.h = (short) h;
		}

		public WRect(WRect rect)
		{
			x = rect.x;
			y = rect.y;
			w = rect.w;
			h = rect.h;
		}

        public short MinX
        {
            get { return x; }
        }

        public short MinY
        {
            get { return y; }
        }

        public short MaxX
        {
            get { return (short) (x + w); }
        }

        public short MaxY
        {
            get { return (short) (y + h); }
        }

        public Vector2w Min
        {
            get { return new Vector2w(x, y); }
        }

        public Vector2w Max
        {
            get { return new Vector2w(x + w, y + h); }
        }

		public bool Contains(short x, short y)
		{
			if (x >= this.x && x < this.x + w && y >= this.y && y < this.y + h)
				return true;
			return false;
		}

		public bool Contains(int x, int y)
		{
			if (x >= this.x && x < this.x + w && y >= this.y && y < this.y + h)
				return true;
			return false;
		}

		public bool Contains(Vector2w pos)
		{
			if (pos.x >= x && pos.x < x + w && pos.y >= y && pos.y < y + h)
				return true;
			return false;
		}
        
		public bool Contains(WRect rect)
		{
			if (Contains(rect.x, rect.y))
				return true;
			if (Contains(rect.x + rect.w, rect.y))
				return true;
			if (Contains(rect.x, rect.y + rect.h))
				return true;
			if (Contains(rect.x + rect.w, rect.y + rect.h))
				return true;
			if (IncludeRect(rect) || rect.IncludeRect(this))
				return true;
			return false;
		}

		public void Clip(WRect rect)
		{
			if (x < rect.x)
			{
				short aVal = (short) (rect.x - x);
				x = rect.x;
				w -= aVal;
			}
			if (y < rect.y)
			{
				short aVal = (short) (rect.y - y);
				y = rect.y;
				h -= aVal;
			}
			if (x + w > rect.x + rect.w)
			{
				short aVal = (short) ((x + w) - (rect.x + rect.w));
				w -= aVal;
			}
			if (y + h > rect.y + rect.h)
			{
				short aVal = (short) ((y + h) - (rect.y + rect.h));
				h -= aVal;
			}
		}

		public static bool operator ==(WRect rect1, WRect rect2)
		{
			if (rect1.x == rect2.x && rect1.y == rect2.y && rect1.w == rect2.w && rect1.h == rect2.h)
				return true;
			return false;
		}

		public static bool operator !=(WRect rect1, WRect rect2)
		{
			if (rect1.x == rect2.x && rect1.y == rect2.y && rect1.w == rect2.w && rect1.h == rect2.h)
				return false;
			return true;
		}

		public static WRect OffsetRectToFree(WRect rectOffset, WRect rectBlock, byte dir)
		{
			WRect aRect = rectOffset;
			switch (dir)
			{
				case 0:
					aRect.y = (short)(rectBlock.y - rectOffset.h);
					break;
				case 1:
					aRect.x = (short)(rectBlock.x - rectOffset.w);
					break;
				case 2:
					aRect.y = (short) (rectBlock.y + rectBlock.h);
					break;
				case 3:
					aRect.x = (short) (rectBlock.x + rectBlock.w);
					break;
			}
			return aRect;
		}

		public bool IncludeRect(WRect rect)
		{
			if (!Contains(rect.x, rect.y))
				return false;
			if (!Contains(rect.x + rect.w - 1, rect.y + rect.h - 1))
				return false;
			return true;
		}

		public override string ToString()
		{
			return x.ToString() + "," + y + "," + w + "," + h;
		}

		public Vector2w GetCenter()
		{
			return new Vector2w(x + w/2, y + h/2);
		}

		public Vector2w Clip(Vector2w pos)
		{
			Vector2w aPos = new Vector2w(pos);
			if (x > aPos.x)
				aPos.x = x;
			if (x + w - 1 < aPos.x)
				aPos.x = (short) (x + w - 1);
			if (y > aPos.y)
				aPos.y = y;
			if (y + h - 1 < aPos.y)
				aPos.x = (short)(y + h - 1);
			return aPos;
		}

        public int GetDistanceToEdge(Vector2w pos)
        {
            return Math.Min(GetDistanceToEdgeX(pos.x), GetDistanceToEdgeY(pos.y));
        }

        public int GetDistanceToEdgeX(int posX)
        {
            int r = Math.Abs(MinX - posX);
            r = Math.Min(r, Math.Abs(MaxX - posX));
            return r;
        }

        public int GetDistanceToEdgeY(int posY)
        {
            int r = Math.Abs(MinY - posY);
            r = Math.Min(r, Math.Abs(MaxY - posY));
            return r;
        }

        public Direction GetNearDir(Vector2w pos)
        {
            if (GetDistanceToEdgeX(pos.x) < GetDistanceToEdgeY(pos.y))
            {
                if (pos.x < w / 2)
                    return Direction.left;
                return Direction.right;
            }

            if (pos.y < h / 2)
                return Direction.down;
            return Direction.up;
        }

        public static WRect Read(BinaryReader reader)
        {
            return new WRect(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(w);
            writer.Write(h);
        }

        public static WRect Read(NetIncomingMessage reader)
        {
			return new WRect(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
		}

        public void Write(NetOutgoingMessage writer)
        {
			writer.Write(x);
            writer.Write(y);
            writer.Write(w);
            writer.Write(h);
		}

		public int Square()
        {
			return w * h;
        }
    }
}
