using SWFServer.Data.Entities;
using System.Collections.Generic;
using System.IO;

namespace SWFServer.Data
{
    public enum GameTaskType
    {
        craft,
        purchase
    }

    public class GameTask
    {
        public uint Id;
        public GameTaskType Type;
        public uint UserId;
        public uint LocId;
        public ushort ItemId;
        public int Count;
        public int Cost;
        public Vector2w Pos;

        public uint ExecutorId;
        public double ExecutionTime;

        public int ReserveMoney;
        public List<Entity> ReserveItems = new List<Entity>();

        public void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write((byte)Type);
            writer.Write(UserId);
            writer.Write(LocId);
            writer.Write(ItemId);
            writer.Write(Count);
            writer.Write(Cost);
            Pos.Write(writer);
            writer.Write(ExecutorId);
            writer.Write(ExecutionTime);
            writer.Write(ReserveMoney);

            writer.Write(ReserveItems.Count);
            foreach (var it in ReserveItems)
            {
                it.Write(writer);
            }
        }

        public static GameTask Read(BinaryReader reader)
        {
            GameTask t = new GameTask();
            t.Id = reader.ReadUInt32();
            t.Type = (GameTaskType)reader.ReadByte();
            t.UserId = reader.ReadUInt32();
            t.LocId = reader.ReadUInt32();
            t.ItemId = reader.ReadUInt16();
            t.Count = reader.ReadInt32();
            t.Cost = reader.ReadInt32();
            t.Pos = Vector2w.Read(reader);
            t.ExecutorId = reader.ReadUInt32();
            t.ExecutionTime = reader.ReadDouble();
            t.ReserveMoney = reader.ReadInt32();

            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                t.ReserveItems.Add(Entity.Read(reader));
            }

            return t;
        }
    }
}
