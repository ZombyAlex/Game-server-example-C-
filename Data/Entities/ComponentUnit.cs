using System.IO;
using Lidgren.Network;
using Newtonsoft.Json;

namespace SWFServer.Data.Entities
{
    public enum UnitState
    {
        stand,
        lie,
        sit,
        shower,
        mining,
        craft,
        miningWater
    }

    public class UnitAction
    {
        public Vector2w ActionPos { get; set; }
        public double TimeAction;
        public ushort ItemId;
        public int Count;
        public bool IsStart = false;
    }

    
    public  class UnitStat
    {
        public int CompleteTask;
        public int MoneyTask;

        public void Write(BinaryWriter writer)
        {
            writer.Write(CompleteTask);
            writer.Write(MoneyTask);
        }

        public void Read(BinaryReader reader)
        {
            CompleteTask = reader.ReadInt32();
            MoneyTask = reader.ReadInt32();
        }
    }

    
    public  class ComponentUnit : ComponentData
    {
        public uint Id;
        public uint UserId;
        public Vector2f Pos;
        public Vector2f Velocity;
        public UnitState State = UnitState.stand;
        public UnitAttributes Attr = new UnitAttributes();
        public UnitSkills Skills = new UnitSkills();
        public int Level = 0;
        public int InventorySize = 8;
        public uint TaskId = 0;
        public UnitStat UnitStat = new UnitStat();
        public Entity HandItem = null;

        [JsonIgnore] public Vector2w CellPos => new Vector2w((int)(Pos.x / GameConst.cellSize), (int)(Pos.y / GameConst.cellSize));

        //temp
        [JsonIgnore] public IUnitController Controller;
        [JsonIgnore] public UnitAction Action = null;

        public UnitAvatar GetAvatar()
        {
            return new UnitAvatar()
            {
                Id = Id, Pos = Pos, Velocity = Velocity, UserId = UserId, State = State, Level = Level,
                ActionPos = Action?.ActionPos ?? Vector2w.Zero, itemHand = HandItem == null ? -1 : HandItem.Id
            };
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(UserId);
            Pos.Write(writer);
            Velocity.Write(writer);
            writer.Write((byte)State);
            Attr.Write(writer);
            writer.Write(Level);
            writer.Write(InventorySize);
            writer.Write(TaskId);
            UnitStat.Write(writer);

            writer.Write(HandItem != null);
            HandItem?.Write(writer);

            Skills.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            Id = reader.ReadUInt32();
            UserId = reader.ReadUInt32();
            Pos = Vector2f.Read(reader);
            Velocity = Vector2f.Read(reader);
            State = (UnitState)reader.ReadByte();
            Attr.Read(reader);
            Level = reader.ReadInt32();
            InventorySize = reader.ReadInt32();
            TaskId = reader.ReadUInt32();
            UnitStat.Read(reader);

            if(reader.ReadBoolean())
                HandItem = Entity.Read(reader);

            Skills.Read(reader);
        }
    }
}