using System.Collections.Generic;
using Lidgren.Network;
using SWFServer.Data;
using SWFServer.Data.Net;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Server;

namespace SWFServer.Game
{

    public class Game: GameThread, IGameUser
    {
        private object locker = new object();
        private List<MsgClient> messages = new List<MsgClient>();


        private ModuleLocation moduleLocation;
        private ModuleUsers moduleUsers;
        private ModuleTask moduleTask;

        public Game(int sleepWait, string threadName, uint gameIndex) : base(sleepWait, threadName, gameIndex)
        {
            moduleUsers = new ModuleUsers();

            moduleTask = new ModuleTask();
            moduleLocation = new ModuleLocation(moduleUsers, moduleTask);

            moduleTask.Init(moduleLocation, moduleUsers);
        }

        protected override void Init()
        {
            AddScheduleCall(0.02f, UpdateMsg);
            AddScheduleUpdate(1f, UpdateUsers);
            AddScheduleCall(30f, SaveStatistic);
            AddScheduleCall(30f, UpdateRating);

            /*
            
            AddScheduleCall(1f, d.GameBots.UpdateBots);
            AddScheduleCall(1f, UpdateMapSwitchBlock);
            AddScheduleCall(1f, UpdateUsers);
            AddScheduleCall(5f, UpdateUnitToRespawn);


            AddPeriod(new GamePeriod(ServerData.serverTime, (int) GameConst.month, UpdateMonth));
            */
        }
        
        private void UpdateUsers(float dt)
        {
            moduleUsers.Update(dt);
            moduleTask.Update(dt);
        }
        /*
        private void UpdateMapSwitchBlock()
        {
            for (int i = 0; i < d.Map.SwitchBlocks.Count; i++)
            {
                var block = d.Map.SwitchBlocks[i];
                if (block.time < ServerData.serverTime)
                {
                    if (!d.Map.IsUnit(block.pos))
                    {
                        var cell = d.Map[block.pos];
                        var info = Info.EntityInfo[Info.EntityInfo[cell.Block.Value].switchItem];
                        cell.Block.Value = info.valUint;
                        d.Net.SendAllCell(block.pos, cell);
                        d.Map.SwitchBlocks.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        */

        
        private void UpdateMonth()
        {
            //GodManager.UpdateBuffs(d.Rnd);
        }

        private void UpdateRating()
        {
            var list = Rating.GetRatings();
            moduleUsers.SendAllRating(list);
        }


        private void UpdateMsg()
        {
            lock (locker)
            {
                if (messages.Count > 0)
                {
                    var msg = messages[0];
                    messages.RemoveAt(0);
                    
                    User user = moduleUsers.GetUser(msg.UserId);
                    if (user == null)
                        return;
                    moduleLocation.UpdateMsg(msg, user);
                }
            }
        }

        protected override void OnTerminate()
        {
            SaveAll();
        }

        public void SaveAll()
        {
            moduleUsers.Terminate();
            moduleTask.Terminate();
            moduleLocation.Terminate();

            
        }

        protected override void Update(float dt)
        {
            if (gameIndex == 0)
            {
                ServerData.serverTime += dt;
            }

            moduleLocation.Update(dt);
        }

        public void SetConnected(User user, bool isConnected)
        {
            lock (locker)
            {
                user.isConnected = isConnected;
                if (!isConnected)
                {
                    if (moduleUsers.IsUser(user.Id))
                    {
                        WorldManager.UserManager.SaveUser(user);
                        moduleUsers.RemoveUser(user);

                        moduleLocation.UnloadUnit(user);
                        
                        Tools.SaveAnalytics("exit " + user.Id + " " + user.TimeGameSession);
                        Tools.LogData("logs/users.txt", "exit " + user.Name + " [" + user.Id + "]");
                    }
                }
            }
        }

        public void NetMsg(User user, MsgClient msg)
        {
            lock (locker)
            {
                messages.Add(msg);
            }
        }

        private void SaveStatistic()
        {
            moduleUsers.SaveStatistic(gameIndex);
        }

        public void UserEnter(User user, INetUser net, NetConnection address, bool isLoadGame)
        {
            lock (locker)
            {
                moduleLocation.UserEnterGame(user, net, address, isLoadGame, this);
            }
        }

        public bool GetLocation(uint locId)
        {
            return moduleLocation.GetLocation(locId);
        }

        public void SendModuleTasks(User user)
        {
            moduleTask.SendModuleTasks(user);
        }

        public void RegenWorld()
        {
            moduleLocation.RegenWorld();
        }
    }
}
