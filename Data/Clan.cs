
using System.Collections.Generic;
using Lidgren.Network;

namespace SWFServer.Data
{
    public enum ClanUserStatus
    {
        leader,
        user
    }

    public class ClanUser
    {
        public uint Id;
        public ClanUserStatus Status;

        public void Read(NetIncomingMessage reader)
        {
            Id = reader.ReadUInt32();
            Status = (ClanUserStatus)reader.ReadByte();
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(Id);
            writer.Write((byte)Status);
        }
    }

    public class Clan
    {
        public uint Id;
        public string Name;

        public List<ClanUser> Users = new List<ClanUser>();

        public void Read(NetIncomingMessage reader)
        {
            Id = reader.ReadUInt32();
            Name = Util.ReadString(reader);

            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                ClanUser user = new ClanUser();
                user.Read(reader);
                Users.Add(user);
            }
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(Id);
            Util.WriteString(writer, Name);

            writer.Write(Users.Count);

            foreach (var user in Users)
            {
                user.Write(writer);
            }
        }
    }
}
