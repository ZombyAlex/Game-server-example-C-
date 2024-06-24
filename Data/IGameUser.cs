using Lidgren.Network;
using SWFServer.Data.Net;

namespace SWFServer.Data
{
    public interface IGameUser
    {
        void SetConnected(User user, bool isConnected);
        void NetMsg(User user, MsgClient msg);
    }
}
