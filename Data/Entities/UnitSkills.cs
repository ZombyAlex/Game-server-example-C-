using System;
using System.Collections.Generic;
using System.IO;

namespace SWFServer.Data.Entities
{
    public class UnitSkills
    {
        private Dictionary<ushort, float> craft = new Dictionary<ushort, float>();
        private Dictionary<MiningType, float> mining = new Dictionary<MiningType, float>();

        public float this[ushort t]
        {
            get
            {
                Check(t);

                return craft[t];
            }
        }

        public float this[MiningType t]
        {
            get
            {
                Check(t);

                return mining[t];
            }
        }

        public Dictionary<ushort, float> Craft => craft;
        public Dictionary<MiningType, float> Mining => mining;

        private void Check(ushort t)
        {
            if (!craft.ContainsKey(t))
            {
                craft.Add(t, 0);
            }
        }

        private void Check(MiningType t)
        {
            if (!mining.ContainsKey(t))
            {
                mining.Add(t, 0);
            }
        }

        public void Change(ushort t, float v)
        {
            Check(t);
            craft[t] = Clamp(craft[t] + v);
        }

        public void Change(MiningType t, float v)
        {
            Check(t);
            mining[t] = Clamp(mining[t] + v);
        }

        public void Set(ushort t, float v)
        {
            Check(t);
            craft[t] = v;
        }

        private float Clamp(float v)
        {
            return Util.Clamp(v, 0, float.MaxValue);
        }

        public void Regression(ushort t)
        {
            foreach (var it in craft)
            {
                if (it.Key != t)
                {
                    Change(it.Key, -0.1f);
                }
            }
        }

        public void Regression(MiningType t)
        {
            foreach (var it in mining)
            {
                if (it.Key != t)
                {
                    Change(it.Key, -0.1f);
                }
            }
        }

        public float Factor(ushort t)
        {
            float val = this[t];
            return GetFactor(val);
        }

        private static float GetFactor(float val)
        {
            if (val < 10)
                return 0.5f;
            if (val < 50)
                return 0.75f;
            if (val < 250)
                return 1f;
            if (val < 1000)
                return 1.5f;
            if (val < 5000)
                return 2.0f;
            if (val < 25000)
                return 2.5f;
            if (val < 100000)
                return 3;
            return 4f;
        }

        public float Factor(MiningType t)
        {
            float val = this[t];
            return GetFactor(val);
        }

        public float FactorAdd(ushort t)
        {
            int val = Level(t);
            return GetFactorAdd(val);
        }

        public float FactorAdd(MiningType t)
        {
            int val = Level(t);
            return GetFactorAdd(val);
        }

        private static float GetFactorAdd(int lvl)
        {
            switch (lvl)
            {
                case 0: return 0;
                case 1: return 2;
                case 2: return 5;
                case 3: return 10;
                case 4: return 20;
                case 5: return 50;
                case 6: return 100;
                case 7: return 100;
                case 8: return 100;
            }
            return 0;
        }


        public int Level(ushort t)
        {
            float val = this[t];
            return GetLevel(val);
        }

        private static int GetLevel(float val)
        {
            if (val < 10)
                return 0;
            if (val < 50)
                return 1;
            if (val < 250)
                return 2;
            if (val < 1000)
                return 3;
            if (val < 5000)
                return 4;
            if (val < 25000)
                return 5;
            if (val < 100000)
                return 6;
            return 7;
        }

        public int Level(MiningType t)
        {
            float val = this[t];
            return GetLevel(val);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(craft.Count);
            foreach (var it in craft)
            {
                writer.Write(it.Key);
                writer.Write(it.Value);
            }

            writer.Write(mining.Count);

            foreach (var it in mining)
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
                ushort t = reader.ReadUInt16();
                float val = reader.ReadSingle();
                craft.Add(t, val);
            }

            cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                MiningType t = (MiningType)reader.ReadByte();
                float val = reader.ReadSingle();
                mining.Add(t, val);
            }
        }
    }
}
