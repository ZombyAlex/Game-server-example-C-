using System;
using System.Collections.Generic;
using System.IO;

namespace SWFServer.Data.Entities
{
    public enum UnitAttrType
    {
        satiety,
        saturation
    }


    public class UnitAttributes
    {
        private Dictionary<UnitAttrType, float> attr = new Dictionary<UnitAttrType, float>();

        public float this[UnitAttrType t]
        {
            get
            {
                Check(t);

                return attr[t];
            }
        }

        private void Check(UnitAttrType t)
        {
            if (!attr.ContainsKey(t))
            {
                attr.Add(t, UnitAttributes.GetDefaultAttr(t));
            }
        }

        public void Change(UnitAttrType t, float v)
        {
            Check(t);
            attr[t] = Clamp(t, attr[t] + v);
        }

        public void Set(UnitAttrType t, float v)
        {
            Check(t);
            attr[t] = v;
        }

        public float Need(UnitAttrType t)
        {
            float v = this[t];
            float m = Max(t);
            return m - v;
        }

        private float Clamp(UnitAttrType t, float v)
        {
            float min = Min(t);
            float max = Max(t);

            return Util.Clamp(v, min, max);
        }

        private float Min(UnitAttrType t)
        {
            switch (t)
            {
                case UnitAttrType.satiety:
                    return 0;
                case UnitAttrType.saturation:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        private float Max(UnitAttrType t)
        {
            switch (t)
            {
                case UnitAttrType.satiety:
                    return 300f;
                case UnitAttrType.saturation:
                    return 100f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        private static float GetDefaultAttr(UnitAttrType t)
        {
            switch (t)
            {
                case UnitAttrType.satiety:
                    return 100f;
                case UnitAttrType.saturation:
                    return 100f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(attr.Count);
            foreach (var it in attr)
            {
                writer.Write((byte)it.Key);
                writer.Write(it.Value);
            }
        }

        public void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                UnitAttrType t = (UnitAttrType)reader.ReadByte();
                float val = reader.ReadSingle();
                attr.Add(t, val);
            }
        }
    }
}