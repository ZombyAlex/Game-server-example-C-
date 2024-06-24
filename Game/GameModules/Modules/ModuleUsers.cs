using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleUsers: GameModule
    {
        private Dictionary<uint, User> users = new Dictionary<uint, User>();
        private List<User> userList = new List<User>();

        public Dictionary<uint, User> Users => users;

        public List<User> UserList => userList;

        public override void Update(float dt)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                userList[i].TimeGame++;
                userList[i].TimeGameSession++;
                if (userList[i].TimeCollDownEnterLoc > 0)
                    userList[i].TimeCollDownEnterLoc -= dt;
            }
        }

        public override void Terminate()
        {
            for (int i = 0; i < userList.Count; i++)
            {
                WorldManager.UserManager.SaveUser(userList[i]);
            }
        }

        public void AddUser(User user)
        {
            if (!users.ContainsKey(user.Id))
            {
                users.Add(user.Id, user);
                userList.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            users.Remove(user.Id);
            userList.Remove(user);
        }

        public User GetUser(uint userId)
        {
            if (users.ContainsKey(userId))
                return users[userId];
            return null;
        }

        public bool IsUser(uint userId)
        {
            return users.ContainsKey(userId);
        }

        public void SaveStatistic(uint gameIndex)
        {
            Tools.LogData("logs/stat" + gameIndex + ".txt", "user count = " + userList.Count);
        }

        public void SendAllRating(List<UserRating> ratings)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                userList[i].SendMsg(new MsgServer(userList[i].Id, MsgServerType.rating, new MsgServerRating(ratings)));
            }
        }
    }
}
