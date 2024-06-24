using System.Collections.Generic;
using Lidgren.Network;
using Newtonsoft.Json;
using SWFServer.Data.Net;

namespace SWFServer.Data
{
    public enum UserRole
    {
        user = 0,
        moderator = 1,
        master = 2,
        admin = 3
    }

    public class User
    {
        public uint Id;
        public string Name;
        public UserRole Role = UserRole.user;
        public uint UnitId;

        public uint LocId = 1;

        public int Money;

        public uint TimeGame = 0;
        public double TimeCollDownEnterLoc = 0;

        [JsonIgnore] public uint TimeGameSession = 0;
        
        [JsonIgnore] public NetConnection address;
        [JsonIgnore] public bool isConnected;

        [JsonIgnore] public IGameUser Game;
        [JsonIgnore] public INetUser Net;
        
        [JsonIgnore] public Vector2w PosMapGrid;
        [JsonIgnore] public List<Vector2w> ViewGridMap = new List<Vector2w>();
        [JsonIgnore] public WRect ViewRect;


        public void SendMsg(MsgServer msg)
        {
            Net.SendMsg(msg);
        }
    }
}
