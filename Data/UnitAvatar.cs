using System.IO;
using SWFServer.Data.Entities;

namespace SWFServer.Data
{
    public class UnitAvatar
    {
        public uint Id;
        public Vector2f Pos;
        public Vector2f Velocity;
        public uint UserId;
        public UnitState State;
        public int Level;
        public Vector2w ActionPos;
        public int itemHand = -1;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            Pos.Write(writer);
            Velocity.Write(writer);
            writer.Write(UserId);
            writer.Write((byte)State);
            writer.Write(Level);
            ActionPos.Write(writer);
            writer.Write(itemHand);
        }

        public static UnitAvatar Read(BinaryReader reader)
        {
            UnitAvatar avatar = new UnitAvatar();

            avatar.Id = reader.ReadUInt32();
            avatar.Pos = Vector2f.Read(reader);
            avatar.Velocity = Vector2f.Read(reader);
            avatar.UserId = reader.ReadUInt32();
            avatar.State = (UnitState)reader.ReadByte();
            avatar.Level = reader.ReadInt32();
            avatar.ActionPos = Vector2w.Read(reader);
            avatar.itemHand = reader.ReadInt32();
            return avatar;
        }
    }
}
