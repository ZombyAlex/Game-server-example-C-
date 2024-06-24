using System;
using System.Collections.Generic;
using System.IO;
using Lidgren.Network;
using Newtonsoft.Json;
using SWFServer.Data;
using SWFServer.Data.Net;
using SWFServer.Server;
using SWFServer.Server.Net;

namespace SWFServer.Game
{
    public class UserInfo
    {
        public uint Id;
        public string Name;
        public string Pass;
    }

    public class GameState
    {
        public uint CurUnitId = 0;
        public uint CurLocId = 0;
        public double ServerTime = 0;
    }

    public class UsersConfig
    {
        public uint CurUserId = 0;
    }

    public class WorldUserManager
    {
        private List<UserInfo> users = new List<UserInfo>();
        private Dictionary<uint, UserInfo> usersDic = new Dictionary<uint, UserInfo>();
        private GameState gameState = new GameState();
        private UsersConfig usersConfig = new UsersConfig();

        private Dictionary<uint, int> userMonies = new Dictionary<uint, int>();

        private object locker = new object();
        private object locker2 = new object();

        private string dataPathUsers = "users/users.json";
        private string dataPathUsersCfg = "users/usersConfig.json";
        private string dataPathState = "data/game/state.json";


        public WorldUserManager()
        {
            LoadUsersCfg();
            LoadUsers();
            LoadGameState();
        }

        public uint GetLoginId(string login, string pass)
        {
            lock (locker)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Name == login)
                    {
                        if (users[i].Pass == pass)
                            return users[i].Id;
                    }
                }

                return 0;
            }
        }


        public bool IsLogin(string login)
        {
            lock (locker)
            {
                var s = login.ToLower();
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Name.ToLower() == s)
                        return true;
                }

                return false;
            }
        }

        public User RegisterUser(string login, string pass)
        {
            lock (locker)
            {
                User user = new User();
                usersConfig.CurUserId++;
                user.Id = usersConfig.CurUserId;
                user.Name = login;
                var u = new UserInfo() { Id = user.Id, Name = user.Name, Pass = pass };
                users.Add(u);
                usersDic.Add(u.Id, u);
                SaveUsers();
                SaveUsersCfg();
                Tools.SaveAnalytics("register " + user.Id);
                return user;
            }
        }

        public uint GetUnitId()
        {
            lock (locker)
            {
                gameState.CurUnitId++;
                SaveGameState();
                return gameState.CurUnitId;
            }
        }

        public uint GetLocId()
        {
            lock (locker)
            {
                gameState.CurLocId++;
                SaveGameState();
                return gameState.CurLocId;
            }
        }

        public void RequestUserName(User user, uint userId)
        {
            lock (locker)
            {
                if (usersDic.ContainsKey(userId))
                {
                    user?.SendMsg(new MsgServer(user.Id, MsgServerType.userName, new MsgServerUserName(userId, usersDic[userId].Name)));
                }
            }
        }

        public string GetUserName(uint userId)
        {
            lock (locker)
            {
                if (usersDic.ContainsKey(userId))
                    return usersDic[userId].Name;
                return String.Empty;
            }
        }

        public void AddUserMoney(uint userId, int money)
        {
            lock (locker)
            {
                if (!userMonies.ContainsKey(userId))
                {
                    userMonies.Add(userId, 0);
                }

                userMonies[userId] += money;
            }
        }

        public void LoginUser(UserLoginInfo info)
        {
            lock (locker)
            {
                uint userId = GetLoginId(info.msg.login, info.msg.pass);
                if (userId != 0)
                {
                    //load user
                    LoadUser(userId, info.net, info.address);
                }
                else
                {
                    if (IsLogin(info.msg.login))
                    {
                        //message exists user
                        info.net.SendMsgAddress(new MsgServer(0, MsgServerType.info,  new MsgServerInfo("login_busy")), info.address);
                        return;
                    }
                    else
                    {
                        //register user
                        User user = RegisterUser(info.msg.login, info.msg.pass);

                        SaveUser(user);

                        var game = WorldManager.GetGame(user);
                        game.Game.UserEnter(user, info.net, info.address, true);
                    }
                }
            }
        }

        private void LoadUser(uint userId, INetUser net, NetConnection address)
        {
            string filename = "users/user" + userId + ".json";

            StreamReader reader = new StreamReader(filename);
            string json = reader.ReadToEnd();
            reader.Close();
            User user = JsonConvert.DeserializeObject<User>(json);


            var game = WorldManager.GetGame(user);
            game.Game.UserEnter(user, net, address, true);

            Tools.SaveAnalytics("enter " + user.Id);
        }

        public void SaveUser(User user)
        {
            lock (locker2)
            {
                string filename = "users/user" + user.Id + ".json";

                string json = JsonConvert.SerializeObject(user);
                StreamWriter writer = new StreamWriter(filename, false);
                writer.Write(json);
                writer.Close();
            }
        }

        private void LoadUsersCfg()
        {
            if (!File.Exists(dataPathUsersCfg))
            {
                SaveUsersCfg();
                return;
            }

            StreamReader reader = new StreamReader(dataPathUsersCfg);
            string json = reader.ReadToEnd();
            reader.Close();
            usersConfig = JsonConvert.DeserializeObject<UsersConfig>(json);

            foreach (var user in users)
            {
                usersDic.Add(user.Id, user);
            }
        }

        private void SaveUsersCfg()
        {
            StreamWriter writer = new StreamWriter(dataPathUsersCfg, false);
            string json = JsonConvert.SerializeObject(usersConfig);
            writer.Write(json);
            writer.Close();
        }

        private void LoadUsers()
        {
            if (!File.Exists(dataPathUsers))
            {
                SaveUsers();
                return;
            }

            StreamReader reader = new StreamReader(dataPathUsers);
            string json = reader.ReadToEnd();
            reader.Close();
            users = JsonConvert.DeserializeObject<List<UserInfo>>(json);

            foreach (var user in users)
            {
                usersDic.Add(user.Id, user);
            }
        }

        private void SaveUsers()
        {
            StreamWriter writer = new StreamWriter(dataPathUsers, false);
            string json = JsonConvert.SerializeObject(users);
            writer.Write(json);
            writer.Close();
        }
        private void LoadGameState()
        {
            if (!File.Exists(dataPathState))
            {
                SaveGameState();
                return;
            }

            StreamReader reader = new StreamReader(dataPathState);
            string json = reader.ReadToEnd();
            reader.Close();
            gameState = JsonConvert.DeserializeObject<GameState>(json);
            ServerData.serverTime = gameState.ServerTime;
        }

        public void SaveGameState()
        {
            gameState.ServerTime = ServerData.serverTime;
            StreamWriter writer = new StreamWriter(dataPathState, false);
            string json = JsonConvert.SerializeObject(gameState);
            writer.Write(json);
            writer.Close();
        }
    }
}