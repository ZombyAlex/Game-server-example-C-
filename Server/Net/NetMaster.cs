using System;
using System.Collections.Generic;
using System.Threading;
using Lidgren.Network;
using SWFServer.Data;
using SWFServer.Data.Net;
using SWFServer.Game;

namespace SWFServer.Server.Net
{
    class NetMasterInfo
    {
        public NetGame net;
        public Thread netThread;

        public NetMasterInfo(NetGame net, Thread netThread)
        {
            this.net = net;
            this.netThread = netThread;
        }

        public bool IsAvailable()
        {
            return net.IsAvailable();
        }

        public bool IsEmpty()
        {
            return net.IsEmpty();
        }

        public int GetUserCount()
        {
            return net.GetUserCount();
        }
    }

    public class NetMaster: GameThread
    {
        public static NetMaster instance;

        private NetPeerConfiguration config;
        private NetServer server;

        private List<NetMasterInfo> nets = new List<NetMasterInfo>();

        public bool IsUpdateUserList { get; set; } = false;

        public NetMaster(int sleepWait, string threadName) : base(sleepWait, threadName, GameConst.netMasterIndex)
        {
            instance = this;
        }

        protected override void Init()
        {
            InitServer();

            for (int i = 0; i < GameConst.netServerCount; i++)
            {
                CreateNet();
            }

            //AddScheduleCall(GameConst.timeRemoveEmptyNetThread, UpdateFreeNet);
            AddScheduleCall(2, UpdateUserList);
        }

        private void UpdateFreeNet()
        {
            for (int i = 0; i < nets.Count; i++)
            {
                if (nets[i].IsEmpty())
                {
                    RemoveNet(nets[i]);
                    i--;
                }
            }
        }

        private void InitServer()
        {
            string nameServer = GameConst.serverName;
            int port = GameConst.port;
            config = new NetPeerConfiguration(nameServer) { Port = port };
            config.MaximumConnections = 2048;
            server = new NetServer(config);
            server.Start();
            Console.WriteLine("MaxConnections=" + config.MaximumConnections);
        }

        protected override void OnTerminate()
        {
            while (nets.Count > 0)
            {
                RemoveNet(nets[0]);
            }
        }

        protected override void Update(float dt)
        {
            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        WriteLog("Error net message=" + msg.ReadString());

                        break;
                    case NetIncomingMessageType.Data:

                        //ProcMsg(msg);

                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        string reason = msg.ReadString();
                        WriteLog("Status Changed ip = " + msg.SenderConnection.RemoteUniqueIdentifier + " status=" + status + " reason:" + reason);

                        if (status == NetConnectionStatus.Connected)
                        {
                            WriteLog("connect ip = " + msg.SenderConnection.RemoteUniqueIdentifier);
                            
                            ConnectUser(msg.SenderConnection);
                        }
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            //net.ChangeConnectStatusClient(msg.SenderConnection, false);
                        }

                        WriteLog("num connect = " + server.Connections.Count);
                        //Console.WriteLine("connection = " + server.Connections.Count);
                        break;
                    default:
                        WriteLog("Unhandled @class = " + msg.MessageType);
                        break;
                }
                server.Recycle(msg);
            }
        }

        private void ConnectUser(NetConnection connection)
        {
            NetMasterInfo info = GetNet();

            SendGameData(connection);

            MsgServer m = new MsgServer(0, MsgServerType.connect, new MsgServerConnect(info.net.Port));
            //MsgServerConnect m = new MsgServerConnect(info.net.Port);
            
            //MsgServerConnect msg = new MsgServerConnect(info.net.Port, Data.gameConfig);
            NetOutgoingMessage om = server.CreateMessage();

           
            m.Write(om);
            server.SendMessage(om, connection, NetDeliveryMethod.ReliableOrdered);
            connection.Disconnect("reconnect");
        }

        private void SendGameData(NetConnection connection)
        {
            /*
            MsgServerGameData msg = new MsgServerGameData(GameData.craftInfo, GameData.blockInfo, GameData.itemInfo, GameData.machineInfo, GameData.abilityInfo, GameData.techInfo);
            NetOutgoingMessage om = server.CreateMessage();
            msg.Write(om);
            server.SendMessage(om, connection, NetDeliveryMethod.ReliableOrdered);
            */
        }

        private NetMasterInfo CreateNet()
        {
            int port = GetFreePort();
            Console.WriteLine("Create net " + port);
            NetGame netGame = new NetGame(10, "Net" + port, port);
            Thread netGameThread = new Thread(netGame.Run) {Name = "Net" + port};
            NetMasterInfo info = new NetMasterInfo(netGame, netGameThread);
            nets.Add(info);
            netGameThread.Start();
            return info;
        }

        private int GetFreePort()
        {
            int port = GameConst.port + 1;

            for (int i = 0; i < 10000; i++)
            {
                if (!IsPortBusy(port))
                    return port;
                port++;
            }
            return port;
        }

        private bool IsPortBusy(int port)
        {
            for (int i = 0; i < nets.Count; i++)
            {
                if (nets[i].net.Port == port)
                    return true;
            }

            return false;
        }

        private void RemoveNet(NetMasterInfo info)
        {
            Console.WriteLine("Remove net "+info.net.Port);
            info.net.Terminate();
            info.netThread.Join();
            nets.Remove(info);
        }

        private NetMasterInfo GetNet()
        {
            int n = Int32.MaxValue;

            NetMasterInfo nmi = null;

            for (int i = 0; i < nets.Count; i++)
            {
                if (nets[i].IsAvailable())
                {
                    if (nets[i].GetUserCount() < n)
                    {
                        n = nets[i].GetUserCount();
                        nmi = nets[i];
                    }
                }
            }

            if (nmi != null)
                return nmi;
            
            return CreateNet();
        }

        private void UpdateUserList()
        {
            if(!IsUpdateUserList)
                return;

            IsUpdateUserList = false;

            List<uint> list = new List<uint>();

            for (int i = 0; i < nets.Count; i++)
            {
                list.AddRange(nets[i].net.GetUsersList());
            }

            for (int i = 0; i < nets.Count; i++)
            {
                nets[i].net.SendAllUserList(list);
            }
        }

        public void ChatMsg(MsgClient msg)
        {
            string userName = WorldManager.UserManager.GetUserName(msg.UserId);

            MsgClientChat m = (MsgClientChat)msg.Data;

            if (m.isChannel)
                Tools.LogData("logs/chat_" + m.channelId + ".txt", userName + ": " + m.text);
            else
                Tools.LogData("logs/chat_private.txt", userName + ": " + m.text);


            for (int i = 0; i < nets.Count; i++)
            {
                nets[i].net.ChatMsg(msg, userName);
            }
        }
    }
}
