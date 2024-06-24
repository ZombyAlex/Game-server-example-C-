using System;
using System.Collections.Generic;
using System.IO;
using Lidgren.Network;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleLocation: GameModule
    {
        private object locker = new object();
        private Dictionary<uint, Location> locations = new Dictionary<uint, Location>();
        private ModuleUsers moduleUsers;
        private ModuleTask moduleTask;

        public Dictionary<uint, Location> Locations => locations;

        public ModuleLocation(ModuleUsers moduleUsers, ModuleTask moduleTask)
        {
            this.moduleUsers = moduleUsers;
            this.moduleTask = moduleTask;

            Location location = new Location(this, 0, moduleUsers, moduleTask, LocationType.world, 0, CreateLocation, (0, Vector2w.Zero), 0);
            SaveLocationInfo(0, LocationType.world, 0, 0);
            locations.Add(0, location);

            GetLocation(1);
            GetLocation(2);
            GetLocation(3);
            GetLocation(4);
            GetLocation(5);
            GetLocation(6);
        }

        private (uint, Vector2w) CreateLocation(uint locId, LocationType locationType, uint ownerId, Vector2w exitPos, int level)
        {
            var id = WorldManager.UserManager.GetLocId();
            Location loc = new Location(this, id, moduleUsers, moduleTask, locationType, ownerId, CreateLocation, (locId, exitPos), level);
            SaveLocationInfo(id, locationType, ownerId, level);
            locations.Add(id, loc);
            return (id, loc.ModuleSystem.ModuleMap.GetExit());
        }

        private void SaveLocationInfo(uint locId, LocationType type, uint ownerId, int level)
        {
            string path = "data/locations/loc" + locId + ".dat";

            if (File.Exists(path))
                return;

            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((int)type);
                    writer.Write(ownerId);
                    writer.Write(level);
                }
            }
        }

        public bool GetLocation(uint locId)
        {
            lock (locker)
            {
                if (locations.ContainsKey(locId))
                    return true;

                Console.WriteLine("Load location " + locId);

                LocationType t;
                uint ownerId;
                int level;

                string path = "data/locations/loc" + locId + ".dat";
                if (!File.Exists(path))
                    return false;

                using (var stream = File.Open(path, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        t = (LocationType)reader.ReadInt32();
                        ownerId = reader.ReadUInt32();
                        level = reader.ReadInt32();
                    }
                }

                Location location = new Location(this, locId, moduleUsers, moduleTask, t, ownerId, CreateLocation, (0, Vector2w.Zero), level);
                locations.Add(locId, location);
                return true;
            }
        }

        public override void Update(float dt)
        {
            List<uint> unloadList = new List<uint>();

            foreach (var location in locations)
            {
                location.Value.Update(dt);
                if(location.Value.IsUnload())
                    unloadList.Add(location.Key);
            }

            for (int i = 0; i < unloadList.Count; i++)
            {
                var loc = locations[unloadList[i]];
                loc.Terminate();
                locations.Remove(unloadList[i]);
                Console.WriteLine("Unload location = " + unloadList[i]);
            }
        }

        public override void Terminate()
        {
            foreach (var location in locations)
            {
                location.Value.Terminate();
            }
        }

        public void UpdateMsg(MsgClient msg, User user)
        {
            locations[user.LocId].NetMsg(msg, user);
        }

        public void UnloadUnit(User user)
        {
            locations[user.LocId].ModuleSystem.ModuleUnits.UnloadUnit(user);
        }

        public void UserEnterGame(User user, INetUser net, NetConnection address, bool isLoadGame, Game game)
        {
            Console.WriteLine("User enter game " + user.Name + " map=" + user.LocId);
            user.Net = net;
            user.Game = game;
            user.address = address;


            if (isLoadGame)
            {
                net.LoadUser(user);
                moduleUsers.AddUser(user);
                Tools.LogData("logs/users.txt", "enter " + user.Name + " [" + user.Id + "]");
            }
            InitEnterGame(user);
        }

        private void InitEnterGame(User user)
        {
            var loc = locations[user.LocId];
            loc.ModuleSystem.ModuleUnits.CheckLoadUnit(user);
            loc = locations[user.LocId];

            user.SendMsg(new MsgServer(user.Id, MsgServerType.user, new MsgServerUser(user.Id, user.LocId, loc.LocationType, loc.OwnerId)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.time, new MsgServerTime(ServerData.serverTime)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.money, new MsgServerMoney(user.Money)));

            loc.ModuleSystem.ModuleMapUpdate.SendMap(user);
            loc.ModuleSystem.ModuleMapUpdate.UpdateAroundUnits(user);
            moduleTask.SendModuleTasks(user);


            var unit = loc.ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit != null)
                user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
        }

        public void TransitionLocation(User user, Entity unit, uint locId, Vector2w pos)
        {
            GetLocation(locId);
            user.LocId = locId;
            var loc = locations[user.LocId];

            Console.WriteLine("Transition location " + user.Name + " map=" + user.LocId);
            var block = loc.ModuleSystem.ModuleMap.Map.GetBlock(pos);

            Vector2w posExit;
            if (block.Item1 == null)
                posExit = pos;
            else
            {
                var size = Util.ToVector2F(Info.EntityInfo[block.Item1.Id].size) / 2;
                var pCenter = Util.ToVector2F(block.Item2) + size;
                var offset = Util.ToVector2F(Info.EntityInfo[block.Item1.Id].exit) - size + new Vector2f(GameConst.cellSize, GameConst.cellSize)/2;
                offset.Rotate(-90f * block.Item1.Rotate.Value);
                posExit = Util.ToVector2W(pCenter + offset);
            }
            
            unit.Unit.Pos = Util.ToVector2F(posExit) + new Vector2f(GameConst.cellSize / 2, GameConst.cellSize / 2);
            unit.Unit.Controller = null;
            loc.ModuleSystem.ModuleUnits.AddUnit(unit);
            user.TimeCollDownEnterLoc = 5f;
            user.ViewGridMap.Clear();
            InitEnterGame(user);
        }

        public void RegenWorld()
        {
            
        }
    }
}
