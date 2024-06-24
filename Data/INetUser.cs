using Lidgren.Network;
using SWFServer.Data.Net;

namespace SWFServer.Data
{
    public interface INetUser
    {
        void SendMsg(MsgServer msg);
        void SendMsgAddress(MsgServer msg, NetConnection address);

        void LoadUser(User user);
        void UnloadUser(uint userId);
        int GetPort();

        //void GameMsg(MsgGame msg);
    }
}
