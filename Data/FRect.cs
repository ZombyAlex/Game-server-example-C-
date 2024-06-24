using System;
using System.IO;
using Lidgren.Network;

namespace SWFServer.Data
{
    public struct FRect
    {
		public float x, y, w, h;

		
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is WRect && Equals((WRect)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = x.GetHashCode();
				hashCode = (hashCode * 397) ^ y.GetHashCode();
				hashCode = (hashCode * 397) ^ w.GetHashCode();
				hashCode = (hashCode * 397) ^ h.GetHashCode();
				return hashCode;
			}
		}

        public FRect(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

		public FRect(FRect rect)
		{
			x = rect.x;
			y = rect.y;
			w = rect.w;
			h = rect.h;
		}

		public float MinX
		{
			get { return x; }
		}

		public float MinY
		{
			get { return y; }
		}

		public float MaxX
		{
			get { return x + w; }
		}

		public float MaxY
		{
			get { return y + h; }
		}

		public bool Contains(float x, float y)
		{
			if (x >= this.x && x < this.x + w && y >= this.y && y < this.y + h)
				return true;
			return false;
		}

		public bool Contains(Vector2f pos)
		{
			if (pos.x >= x && pos.x < x + w && pos.y >= y && pos.y < y + h)
				return true;
			return false;
		}

		public bool Contains(FRect rect)
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
				short aVal = (short)(rect.x - x);
				x = rect.x;
				w -= aVal;
			}
			if (y < rect.y)
			{
				short aVal = (short)(rect.y - y);
				y = rect.y;
				h -= aVal;
			}
			if (x + w > rect.x + rect.w)
			{
				short aVal = (short)((x + w) - (rect.x + rect.w));
				w -= aVal;
			}
			if (y + h > rect.y + rect.h)
			{
				short aVal = (short)((y + h) - (rect.y + rect.h));
				h -= aVal;
			}
		}

		public static bool operator ==(FRect rect1, FRect rect2)
		{
			if (rect1.x == rect2.x && rect1.y == rect2.y && rect1.w == rect2.w && rect1.h == rect2.h)
				return true;
			return false;
		}

		public static bool operator !=(FRect rect1, FRect rect2)
		{
			if (rect1.x == rect2.x && rect1.y == rect2.y && rect1.w == rect2.w && rect1.h == rect2.h)
				return false;
			return true;
		}

		public static FRect OffsetRectToFree(FRect rectOffset, FRect rectBlock, byte dir)
		{
            FRect aRect = rectOffset;
			switch (dir)
			{
				case 0:
					aRect.y = rectBlock.y - rectOffset.h;
					break;
				case 1:
					aRect.x = rectBlock.x - rectOffset.w;
					break;
				case 2:
					aRect.y = rectBlock.y + rectBlock.h;
					break;
				case 3:
					aRect.x = rectBlock.x + rectBlock.w;
					break;
			}
			return aRect;
		}

		public bool IncludeRect(FRect rect)
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

		public Vector2f GetCenter()
		{
			return new Vector2f(x + w / 2, y + h / 2);
		}

		public Vector2f Clip(Vector2f pos)
		{
            Vector2f aPos = new Vector2f(pos);
			if (x > aPos.x)
				aPos.x = x;
			if (x + w - 1 < aPos.x)
				aPos.x = (short)(x + w - 1);
			if (y > aPos.y)
				aPos.y = y;
			if (y + h - 1 < aPos.y)
				aPos.x = (short)(y + h - 1);
			return aPos;
		}

		public float GetDistanceToEdge(Vector2f pos)
		{
			return Math.Min(GetDistanceToEdgeX(pos.x), GetDistanceToEdgeY(pos.y));
		}

		public float GetDistanceToEdgeX(float posX)
		{
			float r = Math.Abs(MinX - posX);
			r = Math.Min(r, Math.Abs(MaxX - posX));
			return r;
		}

		public float GetDistanceToEdgeY(float posY)
		{
            float r = Math.Abs(MinY - posY);
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

		public float Square()
		{
			return w * h;
		}

        public Vector2f IntersectLineRect(Vector2f a, Vector2f b)
        {
            // Получим параметрическое уравнение линии AB
            float dx = b.x - a.x;
            float dy = b.y - a.y;
			
            // Проверка и вычисление пересечения с вертикальными сторонами
            if (dx != 0)
            {
                float x1 = (MinX - a.x) / dx;
                float x2 = (MaxX - a.x) / dx;
                float y1 = a.y + x1 * dy;
                float y2 = a.y + x2 * dy;
                if (y1 >= MinY && y1 <= MaxY && x1 >= 0 && x1 <= 1)
                    return new Vector2f(MinX, y1);
                if (y2 >= MinY && y2 <= MaxY && x2 >= 0 && x2 <= 1)
                    return new Vector2f(MaxX, y2);
            }

            // Проверка и вычисление пересечения с горизонтальными сторонами
            if (dy != 0)
            {
                float y1 = (MinY - a.y) / dy;
                float y2 = (MaxY - a.y) / dy;
                float x1 = a.x + y1 * dx;
                float x2 = a.x + y2 * dx;
                if (x1 >= MinX && x1 <= MaxX && y1 >= 0 && y1 <= 1)
                    return new Vector2f(x1, MinY);
                if (x2 >= MinX && x2 <= MaxX && y2 >= 0 && y2 <= 1)
                    return new Vector2f(x2, MaxY);
            }

            // Точка пересечения не найдена
            return b;
        }

        public FRect ShrinkRect(FRect rect, float amount)
        {
            FRect newRect = new FRect(rect);
            newRect.x = rect.x + amount;
            newRect.y = rect.y + amount;
            newRect.w -= 2 * amount;
            newRect.h -= 2 * amount;
            return newRect;
        }
	} 
}
