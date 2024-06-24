using System.Collections.Generic;
using System.IO;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleTask : GameModule
    {
        private string path = "data/game/tasks.dat";
        private List<GameTask> tasks = new List<GameTask>();

        private ModuleLocation moduleLocation;
        private ModuleUsers moduleUsers;

        private ScheduleHandler scheduleHandler;

        private uint curId = 1;

        private readonly List<string> buyingRes = new List<string>()
            { "wood", "stone_item", "berry", "coal", "iron", "bar_iron", "wooden_plank", "stone_block" };

        public ModuleTask()
        {
        }

        public void Init(ModuleLocation moduleLocation, ModuleUsers moduleUsers)
        {
            this.moduleLocation = moduleLocation;
            this.moduleUsers = moduleUsers;

            Load();

            scheduleHandler = new ScheduleHandler("module_task");
            scheduleHandler.AddScheduleCall(30f, UpdateWorkTasks);
            scheduleHandler.AddScheduleCall(1f, UpdateDeadTasks);
            scheduleHandler.AddScheduleCall(30f, UpdateUnitDeadTasks);
        }

        private uint GetId()
        {
            var id = curId;
            curId++;
            return id;
        }

        private void Load()
        {
            if (!File.Exists(path))
                return;

            using (var stream = File.Open(path, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    curId = reader.ReadUInt32();
                    int cnt = reader.ReadInt32();
                    for (int i = 0; i < cnt; i++)
                    {
                        tasks.Add(GameTask.Read(reader));
                    }
                }
            }
        }

        private void Save()
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(curId);
                    writer.Write(tasks.Count);

                    foreach (var t in tasks)
                    {
                        //Util.JsonWrite(writer, t);
                        t.Write(writer);
                    }
                }
            }
        }

        public override void Update(float dt)
        {
            scheduleHandler.Update(dt);
        }

        private void UpdateWorkTasks()
        {
            bool isUpdate = false;
            int indexWork1 = 0;
            foreach (var loc in moduleLocation.Locations)
            {
                if (loc.Value.LocationType == LocationType.work1)
                {
                    uint locId = loc.Key;

                    var list = tasks.FindAll(f => f.LocId == locId);
                    int count = 5;
                    if (list.Count < count)
                    {
                        int need = count - list.Count;
                        for (int i = 0; i < need; i++)
                        {
                            CreateLocationTask(locId, indexWork1);
                            isUpdate = true;
                        }
                    }

                    indexWork1++;
                }
                else if (loc.Value.LocationType == LocationType.buying)
                {
                    uint locId = loc.Key;

                    var list = tasks.FindAll(f => f.LocId == locId);
                    int count = 10;
                    if (list.Count < count)
                    {
                        int need = count - list.Count;
                        for (int i = 0; i < need; i++)
                        {
                            CreateLocationTaskBuying(locId);
                            isUpdate = true;
                        }
                    }
                }

            }

            if (isUpdate)
                SendAllTasks();
        }

        private void CreateLocationTaskBuying(uint locId)
        {

            ushort itemId = Info.EntityInfo[buyingRes[Rnd.Range(0, buyingRes.Count)]].id;

            int cost = Info.EntityInfo[itemId].costSell;
            int count = Rnd.Range(5, 61);


            moduleLocation.GetLocation(locId);

            Vector2w pos = moduleLocation.Locations[locId].ModuleSystem.ModuleMap.Map.FindExit(0);

            int reward = count * cost;

            GameTask task = new GameTask()
            {
                Id = GetId(),
                Type = GameTaskType.purchase,
                LocId = locId,
                Cost = cost,
                Pos = pos,
                Count = count,
                ItemId = itemId,
                ExecutionTime = ServerData.serverTime + 86400,
                ReserveMoney = reward
            };

            tasks.Add(task);
        }

        private void CreateLocationTask(uint locId, int indexWork1)
        {
            ushort itemId = indexWork1 == 0 ? Info.EntityInfo["axe_stone"].id : Info.EntityInfo["pick_stone"].id;

            int cost = 5;
            int count = 5;
            List<Entity> list = new List<Entity>();

            foreach (var item in Info.EntityInfo[itemId].craft.items)
            {
                Entity entity = Entity.Create(item.id);
                entity.Count.Value = item.count;
                list.Add(entity);
            }

            moduleLocation.GetLocation(locId);

            Vector2w pos = moduleLocation.Locations[locId].ModuleSystem.ModuleMap.Map.FindExit(0);

            int reward = count * cost;
            GameTask task = new GameTask()
            {
                Id = GetId(), Type = GameTaskType.craft, LocId = locId, Cost = cost, Pos = pos, Count = count, ItemId = itemId, ExecutionTime = ServerData.serverTime + 31536000,
                ReserveMoney = reward, ReserveItems = list
            };

            tasks.Add(task);
        }

        public void SendAllTasks()
        {
            List<GameTask> list = new List<GameTask>(tasks.FindAll(f => f.ExecutionTime > ServerData.serverTime));
            foreach (var user in moduleUsers.UserList)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.tasks, new MsgServerTasks(list)));
            }
        }

        public void SendModuleTasks(User user)
        {
            List<GameTask> list = new List<GameTask>(tasks);
            user.SendMsg(new MsgServer(user.Id, MsgServerType.tasks, new MsgServerTasks(list)));
        }

        public override void Terminate()
        {
            Save();
        }

        public void CreateTask(User user, GameTask task)
        {
            if (task.Count < 1 || task.Cost < 1)
                return;

            var loc = moduleLocation.Locations[user.LocId];

            if (task.LocId != user.LocId || loc.OwnerId != user.Id)
                return;

            if (user.Money < task.Count * task.Cost)
                return;

            var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit == null)
                return;

            var info = Info.EntityInfo[task.ItemId];

            if(info.craft == null) return;

            foreach (var it in info.craft.items)
            {
                int cnt = it.count * task.Count;
                if (!UtilInventory.IsItem(it.id, cnt, unit.Entities.Entities))
                    return;
            }

            task.Id = GetId();
            task.Pos = loc.ModuleSystem.ModuleMap.Map.FindExit(0);
            task.ExecutionTime += ServerData.serverTime;
            task.ReserveMoney = task.Count * task.Cost;
            task.UserId = user.Id;
            
            user.Money -= task.Count * task.Cost;

            foreach (var it in info.craft.items)
            {
                int cnt = it.count * task.Count;
                UtilInventory.MoveItem(Info.EntityInfo[it.id].id, cnt, unit.Entities.Entities, task.ReserveItems);
            }


            user.SendMsg(new MsgServer(user.Id, MsgServerType.money, new MsgServerMoney(user.Money)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));

            tasks.Add(task);

            SendAllTasks();
        }

        public void GetTask(User user, uint taskId)
        {
            var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit == null)
                return;

            if (unit.Unit.TaskId != 0) return;

            var task = tasks.Find(f => f.Id == taskId);
            if(task == null) return;

            if (task.ExecutorId != 0)
                return;

            if(task.UserId == user.Id) return;

            task.ExecutorId = user.Id;
            unit.Unit.TaskId = taskId;


            SendAllTasks();
            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
        }

        private void UpdateDeadTasks()
        {
            List<GameTask> deleteList = new List<GameTask>();
            foreach (var task in tasks)
            {
                if (task.ExecutionTime < ServerData.serverTime)
                {
                    if (task.ItemId == 0)
                        deleteList.Add(task);
                    else
                    {
                        User user = moduleUsers.GetUser(task.UserId);
                        if (user != null)
                        {
                            user.Money += task.ReserveMoney;
                            user.SendMsg(new MsgServer() { UserId = user.Id, Data = new MsgServerMoney(user.Money), Type = MsgServerType.money });
                            var loc = moduleLocation.GetLocation(task.LocId);
                            var moduleMap = moduleLocation.Locations[task.LocId].ModuleSystem.ModuleMap;
                            var moduleMapUpdate = moduleLocation.Locations[task.LocId].ModuleSystem.ModuleMapUpdate;

                            foreach (var it in task.ReserveItems)
                            {
                                var container = moduleMap.Map.FindContainer(it.Id, it.Count.Value);
                                UtilInventory.AddItem(it, it.Count.Value, null, container.Item1.Entities.Entities);
                                moduleMapUpdate.SendAllCell(container.Item2);
                            }

                            deleteList.Add(task);
                        }
                    }
                }
            }

            if (deleteList.Count > 0)
            {
                foreach (var task in deleteList)
                {
                    tasks.Remove(task);
                }

                SendAllTasks();
            }
        }

        private void UpdateUnitDeadTasks()
        {
            foreach (var user in moduleUsers.UserList)
            {
                var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
                if (unit != null)
                {
                    if (unit.Unit.TaskId != 0)
                    {
                        if (tasks.Find(f => f.Id == unit.Unit.TaskId) == null)
                        {
                            unit.Unit.TaskId = 0;
                            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
                        }
                    }
                }
            }
        }

        public GameTask FindTask(uint taskId)
        {
            return tasks.Find(f => f.Id == taskId);
        }

        public void CompleteTask(GameTask task)
        {
            tasks.Remove(task);
            SendAllTasks();
        }

        public void CancelTask(User user)
        {
            var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit == null)
                return;

            if (unit.Unit.TaskId == 0) return;

            var task = tasks.Find(f => f.Id == unit.Unit.TaskId);
            if (task == null) return;

            if (task.ExecutorId != user.Id)
                return;

            task.ExecutorId = 0;
            unit.Unit.TaskId = 0;
            SendAllTasks();
            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
        }

        public void RemoveTask(User user, uint id)
        {
            var task = FindTask(id);

            if(task == null) return;

            if (task.UserId != user.Id) return;

            if(task.ExecutorId != 0) return;

            var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit == null)
                return;

            user.Money += task.ReserveMoney;

            var list = new List<Entity>(task.ReserveItems);

            foreach (var it in list)
            {
                if (UtilInventory.IsAddItem(it.Id, unit.Unit.InventorySize,it.Count.Value, unit.Entities.Entities))
                {
                    UtilInventory.MoveItem(it.Id, it.Count.Value, task.ReserveItems, unit.Entities.Entities);
                }
            }

            user.SendMsg(new MsgServer(user.Id, MsgServerType.money, new MsgServerMoney(user.Money)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));

            tasks.Remove(task);

            SendAllTasks();

        }

        public void PerformTask(User user, uint id)
        {
            var unit = moduleLocation.Locations[user.LocId].ModuleSystem.ModuleUnits.GetUnit(user);
            if (unit == null)
                return;

            if (unit.Unit.TaskId == 0) return;

            var task = tasks.Find(f => f.Id == unit.Unit.TaskId);
            if (task == null) return;

            if (task.ExecutorId != user.Id)
                return;

            if(task.Id!= id) return;

            if(task.Type != GameTaskType.purchase) return;

            if(user.LocId!= task.LocId) return;

            if (!UtilInventory.IsItem(task.ItemId, task.Count, unit.Entities.Entities))
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("not_items")));
                return;
            }


            if (task.UserId != 0)//add items
            {
                //TODO
                //check container and size
            }

            UtilInventory.RemoveItem(task.ItemId, task.Count, unit.Entities.Entities);
            user.Money += task.ReserveMoney;
            user.SendMsg(new MsgServer(user.Id, MsgServerType.money, new MsgServerMoney(user.Money)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));

            CompleteTask(task);

        }
    }
}
