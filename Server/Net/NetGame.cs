using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Lidgren.Network;
using SWFServer.Data;
using SWFServer.Data.Net;
using SWFServer.Game;

namespace SWFServer.Server.Net
{
    public class UserLoginInfo
    {
        public MsgClientLogin msg;
        public INetUser net;
        public NetConnection address;

        public UserLoginInfo(MsgClientLogin msg, INetUser net, NetConnection address)
        {
            this.msg = msg;
            this.net = net;
            this.address = address;
        }
    }
    

    public class NetGame: GameThread, INetUser
    {
        private NetPeerConfiguration config;
        private NetServer server;
        //private Server.Net.Net net = null;
        private int port;

        private float timeEmpty = 0;

        private Dictionary<IPAddress, bool> blocks = new Dictionary<IPAddress, bool>();
        private Dictionary<IPAddress, bool> blocksHand = new Dictionary<IPAddress, bool>();

        private bool isBlockIp = false;

        private Dictionary<uint, User> userData = new Dictionary<uint, User>();
        private VectorLock netMsg = new VectorLock();
        //private VectorLock specialMsg = new VectorLock();
        private object locker = new object();

        

        public int Port
        {
            get { return port; }
        }

        public NetGame(int sleepWait, string threadName, int port) : base(sleepWait, threadName, (uint)(port + GameConst.netIndex))
        {
            this.port = port;
        }

        protected override void Init()
        {
            InitServer();

            AddScheduleCall(0.02f, UpdateOutMsg);
            AddScheduleCall(10.0f, UpdateStatusUsers);

            AddScheduleCall(10.0f, UpdateBlockList);
        }

        private void UpdateBlockList()
        {
            string path = "data/block_list.txt";

            if (!File.Exists(path))
                return;

            blocksHand.Clear();
            string line;

            StreamReader file = new StreamReader(path);

            while ((line = file.ReadLine()) != null)
            {
                IPAddress ad = IPAddress.Parse(line);
                if (!blocksHand.ContainsKey(ad))
                    blocksHand.Add(ad, true);
            }

            file.Close();

            path = "data/block_ip.txt";

            isBlockIp = File.Exists(path);
        }

        private void InitServer()
        {
            string nameServer = GameConst.serverName;
            config = new NetPeerConfiguration(nameServer) {Port = port};
            config.MaximumConnections = GameConst.countNetThreadConnect;
            server = new NetServer(config);
            server.Start();

            
            //Console.WriteLine("MaxConnections=" + config.MaximumConnections);
        }

        protected override void Update(float dt)
        {
            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                if (msg.SenderConnection == null)
                {
                    server.Recycle(msg);
                    continue;
                }

                if (msg.SenderEndPoint == null)
                {
                    msg.SenderConnection.Disconnect("");
                    server.Recycle(msg);
                    continue;
                }

                if (msg.SenderEndPoint.Address == null)
                {
                    msg.SenderConnection.Disconnect("");
                    server.Recycle(msg);
                    continue;
                }

                if (blocks.ContainsKey(msg.SenderEndPoint.Address) || blocksHand.ContainsKey(msg.SenderEndPoint.Address))
                {
                    msg.SenderConnection.Disconnect("");
                    server.Recycle(msg);
                    continue;
                }

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        WriteLog("Error net message=" + msg.ReadString());

                        break;
                    case NetIncomingMessageType.Data:
                        
                        ProcMsg(msg);

                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        string reason = msg.ReadString();
                        WriteLog("Status Changed ip = " + msg.SenderConnection.RemoteUniqueIdentifier + " status=" + status + " reason:" + reason);

                        if (status == NetConnectionStatus.Connected)
                        {
                            WriteLog("connect ip = " + msg.SenderConnection.RemoteUniqueIdentifier);

                            ChangeConnectStatusClient(msg.SenderConnection, true);
                        }
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            ChangeConnectStatusClient(msg.SenderConnection, false);
                        }

                        WriteLog("num connect = " + server.Connections.Count);
                        Console.WriteLine("connect status = " + status);
                        Console.WriteLine("connection = " + server.Connections.Count + " net=" + port);
                        break;
                    default:
                        WriteLog("Unhandled @class = " + msg.MessageType);
                        break;
                }
                server.Recycle(msg);
            }

            if (server.Connections.Count == 0)
                timeEmpty += dt;
            else
                timeEmpty = 0;
        }


        public bool IsAvailable()
        {
            if (server == null)
                return false;
            return server.Connections.Count < GameConst.countNetThreadConnect;
        }

        public bool IsEmpty()
        {
            return server.Connections.Count == 0 && timeEmpty > GameConst.timeRemoveEmptyNetThread;
        }

        protected override void OnTerminate()
        {
            server.Shutdown("stop server");
        }

        public int GetUserCount()
        {
            return server.Connections.Count;
        }

        public void ProcMsg(NetIncomingMessage msg)
        {
            MsgClient m = new MsgClient();
            //m.address = msg.SenderConnection;
            m.Read(msg);

            User user = GetUser(msg.SenderConnection);

            if (user == null)
            {
                if (m.Type == MsgClintType.login)
                {
                    MsgClientLogin ms = (MsgClientLogin)m.Data;
                    var l = new UserLoginInfo(ms, this, msg.SenderConnection);
                    WorldManager.UserManager.LoginUser(l);
                }
                return;
            }

            //MsgClient message = CreateMsg(m.Class);

            //message.OnRead(msg);
            m.UserId = user.Id;
            user.Game.NetMsg(user, m);
        }
        /*
        private MsgClient CreateMsg(MsgClintType @class)
        {
            switch (@class)
            {
                case MsgClintType.login:
                    return new MsgClientLogin();
                case MsgClintType.requestUserName:
                    return new MsgClientRequestUserName();
                case MsgClintType.chat:
                    return new MsgClientChat();
                case MsgClintType.signal:
                    return new MsgClientSignal();
                case MsgClintType.undocking:
                    return new MsgClientUndocking();
                case MsgClintType.inputKey:
                    return new MsgClientInputKey();
                case MsgClintType.docking:
                    return new MsgClientDocking();
                case MsgClintType.setTarget:
                    return new MsgClientSetTarget();
                case MsgClintType.moduleActivate:
                    return new MsgClientModuleActivate();
                case MsgClintType.selectUnit:
                    return new MsgClientSelectUnit();
                case MsgClintType.moduleAction:
                    return new MsgClientModuleAction();
                case MsgClintType.moveItems:
                    return new MsgClientMoveItems();
                case MsgClintType.getModuleCost:
                    return new MsgClientGetModuleCost();
                case MsgClintType.sellModule:
                    return new MsgClientSellModule();
                case MsgClintType.buy:
                    return new MsgClientBuy();
                case MsgClintType.repair:
                    return new MsgClientRepair();
                case MsgClintType.getShipCost:
                    return new MsgClientGetShipCost();
                case MsgClintType.sellShip:
                    return new MsgClientSellShip();
                case MsgClintType.upSkill:
                    return new MsgClientUpSkill();
                case MsgClintType.buySkill:
                    return new MsgClientBuySkill();
                case MsgClintType.buyLimit:
                    return new MsgClientBuyLimit();
                case MsgClintType.buyClan:
                    return new MsgClientBuyClan();
                case MsgClintType.inviteClan:
                    return new MsgClientInviteClan();
                case MsgClintType.acceptInviteClan:
                    return new MsgClientAcceptInviteClan();
                case MsgClintType.removeUserInClan:
                    return new MsgClientRemoveUserInClan();
                case MsgClintType.addProduct:
                    return new MsgClientAddProduct();
                case MsgClintType.removeProduct:
                    return new MsgClientRemoveProduct();
                case MsgClintType.requestResource:
                    return new MsgClientRequestResource();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        */

        private User GetUser(NetConnection inAddress)
        {
            foreach (var user in userData)
            {
                if (user.Value.address == inAddress)
                    return user.Value;
            }
            return null;
        }

        public void ChangeConnectStatusClient(NetConnection senderConnection, bool isConnected)
        {
            User user = GetUser(senderConnection);
            if (user != null)
            {
                user.isConnected = isConnected;

                user.Game.SetConnected(user, isConnected);
                //Console.WriteLine("Net user =" + user.valUint + " status connect = " + isConnected);
                //Data.t.dataBaseToGame.Add(new MsgGameUserStatusConnect(user.valUint, user.mapId, isConnected, this));
            }

        }

        public void UpdateOutMsg()
        {

            //сетевые сообщения

            if (netMsg.ToWorkFast())
            {
                var list = netMsg.GetWork();
                for (int i = 0; i < list.Count; i++)
                {
                    MsgServer msg = (MsgServer)list[i];

                    if (userData.ContainsKey(msg.UserId))
                    {
                        //Console.WriteLine("send msg = " + msg.@class);


                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        NetOutgoingMessage om = server.CreateMessage();
                        msg.Write(om);
                        server.SendMessage(om, userData[msg.UserId].address, NetDeliveryMethod.ReliableOrdered);

                        sw.Stop();
                        WriteLogTime(server.Port, sw.ElapsedMilliseconds, msg.Type.ToString());
                    }
                }
            }
            /*
            if (specialMsg.ToWorkFast())
            {
                var list = specialMsg.GetWork();
                for (int i = 0; i < list.Count; i++)
                {
                    SpecialMsg msg = (SpecialMsg)list[i];

                    NetOutgoingMessage om = server.CreateMessage();
                    msg.msg.Write(om);
                    server.SendMessage(om, msg.address, NetDeliveryMethod.ReliableOrdered);
                }
            }
            */
        }

        private void WriteLogTime(int port, long timeMilliseconds, string funcName)
        {
            if (timeMilliseconds >= 1000)
                LogShedule("logs/time_sheduleNetH" + port + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
            else if (timeMilliseconds >= 100)
                LogShedule("logs/time_sheduleNetM" + port + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
            //else if (timeMilliseconds >= 50)
            //	Util.WriteLog("time_sheduleL.txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
        }

        public static void LogShedule(string filename, string text)
        {
            using (StreamWriter writer = File.AppendText(filename))
            {
                writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
            }

        }

        public void SendMsg(MsgServer msg)
        {
            netMsg.Add(msg);
        }

        public void SendMsgAddress(MsgServer msg, NetConnection address)
        {
            lock (locker)
            {
                NetOutgoingMessage om = server.CreateMessage();
                msg.Write(om);
                server.SendMessage(om, address, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void LoadUser(User user)
        {
            lock (locker)
            {
                if (!userData.ContainsKey(user.Id))
                    userData.Add(user.Id, user);
                else
                    userData[user.Id] = user;
                NetMaster.instance.IsUpdateUserList = true;
            }
        }

        public void UnloadUser(uint userId)
        {
            lock (locker)
            {
                userData.Remove(userId);
                NetMaster.instance.IsUpdateUserList = true;
            }
        }

        public int GetPort()
        {
            return server.Port;
        }


        public void UpdateStatusUsers()
        {
            List<uint> removeList = new List<uint>();

            foreach (var user in userData)
            {
                if (user.Value.address.Status != NetConnectionStatus.Connected)
                {
                    user.Value.isConnected = false;
                    user.Value.Game.SetConnected(user.Value,false);

                    removeList.Add(user.Key);
                }
            }

            if (removeList.Count > 0)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    userData.Remove(removeList[i]);
                }

                NetMaster.instance.IsUpdateUserList = true;
            }
        }

        public List<uint> GetUsersList()
        {
            lock (locker)
            {
                List<uint> list = new List<uint>();
                foreach (var user in userData)
                {
                    list.Add(user.Key);
                }

                return list;
            }
        }

        public void SendAllUserList(List<uint> list)
        {
            lock (locker)
            {
                foreach (var user in userData)
                {
                    SendMsg(new MsgServer(user.Key, MsgServerType.userList, new MsgServerUserList(list)));
                }
            }
            
        }

        public void ChatMsg(MsgClient msg, string name)
        {
            lock (locker)
            {
                MsgClientChat m = (MsgClientChat)msg.Data;

                if (m.isChannel)
                {
                    foreach (var user in userData)
                    {
                        SendMsg(new MsgServer(user.Key, MsgServerType.chat, new MsgServerChat(m.isChannel, m.channelId, name, m.text)));
                    }
                }
                else
                {
                    if (userData.ContainsKey(m.channelId))
                        SendMsg(new MsgServer(m.channelId, MsgServerType.chat, new MsgServerChat(false, msg.UserId, name, m.text)));
                    if (userData.ContainsKey(msg.UserId))
                        SendMsg(new MsgServer(msg.UserId, MsgServerType.chat,  new MsgServerChat(false, m.channelId, name, m.text)));
                }
            }
        }
    }
}
