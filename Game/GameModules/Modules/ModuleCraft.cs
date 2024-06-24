
using System.ComponentModel;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleCraft: GameModule
    {
        private ModuleUnits moduleUnits;
        private ModuleMap moduleMap;
        private ModuleLocation moduleLocation;
        private ModuleTask moduleTask;
        private ModuleMapUpdate moduleMapUpdate;

        public ModuleCraft(ModuleUnits moduleUnits, ModuleMap moduleMap, ModuleLocation moduleLocation, ModuleTask moduleTask, ModuleMapUpdate moduleMapUpdate)
        {
            this.moduleUnits = moduleUnits;
            this.moduleMap = moduleMap;
            this.moduleLocation = moduleLocation;
            this.moduleTask = moduleTask;
            this.moduleMapUpdate = moduleMapUpdate;
        }

        public override void Update(float dt)
        {
            foreach (var unit in moduleUnits.Units)
            {
                var u = unit.Value.Unit;
                if (u.State == UnitState.craft)
                {
                    if (u.Action == null)
                    {
                        StopCraft(unit.Value);
                        return;
                    }
                    /*
                    var block = moduleMap.Map.GetBlock(u.Action.ActionPos);
                    if (block.Item1 == null)
                    {
                        StopCraft(unit.Value);
                        return;
                    }
                    */

                    ushort itemId = u.Action.ItemId;

                    int count = 1;

                    //var item = block.Item1.Entities.Entities.Find(f => f.Count.Value > 0);

                    User user = moduleUnits.GetUser(u.UserId);

                    


                    if (!u.Action.IsStart)
                    {
                        if (!IsReadyCraft(user, unit.Value, itemId, count, u.Action.ActionPos))
                            return;

                        float t = Info.EntityInfo[itemId].craft.time;
                        u.Action.TimeAction = ServerData.serverTime + t;
                        user?.SendMsg(new MsgServer(user.Id, MsgServerType.unitAction, new MsgServerUnitAction(t)));
                        u.Action.IsStart = true;
                    }
                    else
                    {
                        if (ServerData.serverTime >= u.Action.TimeAction)
                        {
                            if (!IsReadyCraft(user, unit.Value, itemId, count, u.Action.ActionPos))
                                return;
                            //complete

                            var info = Info.EntityInfo[itemId];

                            var task = GetTask(user, unit.Value, itemId);

                            if (task != null)
                            {
                                UtilInventory.RemoveItem(info.craft.items, task.ReserveItems);
                                task.Count--;
                                if (task.Count <= 0)
                                {
                                    u.UnitStat.CompleteTask++;
                                    u.TaskId = 0;
                                    moduleTask.CompleteTask(task);
                                }
                                else
                                    moduleTask.SendAllTasks();
                            }
                            else
                            {
                                UtilInventory.RemoveItem(info.craft.items, unit.Value.Entities.Entities);

                            }

                            bool isRent = moduleLocation.Locations[user.LocId].LocationType == LocationType.rent1;
                            if (isRent)
                            {
                                user.Money -= info.costCraft;
                                user.SendMsg(new MsgServer() { UserId = user.Id, Data = new MsgServerMoney(user.Money), Type = MsgServerType.money });
                            }

                            if (task != null)
                            {
                                if (task.UserId != 0)
                                {
                                    //find container
                                    //add item container

                                    var container = moduleMap.Map.FindContainer(itemId, count);
                                    Entity item = Entity.Create(u.Action.ItemId);
                                    if (item.Durability != null)
                                        item.Durability.Value *= u.Skills.Factor(itemId);
                                    UtilInventory.AddItem(item, count, null, container.Item1.Entities.Entities);
                                    moduleMapUpdate.SendAllCell(container.Item2);

                                }

                                //add money
                                task.ReserveMoney -= task.Cost;
                                user.Money += task.Cost;
                                u.UnitStat.MoneyTask += task.Cost;

                                user.SendMsg(new MsgServer() { UserId = user.Id, Data = new MsgServerMoney(user.Money), Type = MsgServerType.money });
                            }
                            else
                            {
                            
                                Entity item = Entity.Create(u.Action.ItemId);
                                if (item.Durability != null)
                                    item.Durability.Value *= u.Skills.Factor(itemId);
                                UtilInventory.AddItem(item, count, null, unit.Value.Entities.Entities);
                                user?.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Value.Entities.Entities, Vector2w.Empty)));
                            }


                            if (info.workbenchCraft != WorkbenchType.not && user!= null && moduleLocation.Locations[user.LocId].OwnerId != 0)//check workbench
                            {
                                var block = moduleMap.Map.GetBlock(u.Action.ActionPos);
                                if (block.Item1 != null)
                                {
                                    //Damage Workbench
                                    block.Item1.Durability.Value -= 1f;
                                    if (block.Item1.Durability.Value <= 0)
                                    {
                                        moduleMap.Map.RemoveEntity(u.Action.ActionPos, EntityMapLayer.block);
                                        StopCraft(unit.Value);
                                    }
                                    moduleMapUpdate.SendAllCell(block.Item2);
                                }
                            }

                            u.Skills.Change(itemId, 1f);
                            u.Skills.Regression(itemId);

                            u.Action.Count--;

                            if (u.Action.Count <= 0)
                            {
                                StopCraft(unit.Value);
                            }
                            else
                            {
                                float t = info.craft.time;
                                u.Action.TimeAction = ServerData.serverTime + t;
                                user?.SendMsg(new MsgServer(user.Id, MsgServerType.unitAction, new MsgServerUnitAction(t)));
                            }
                        }
                    }
                }
            }
        }

        private bool IsReadyCraft(User user, Entity unit, ushort itemId, int count, Vector2w pos)
        {
            var task = GetTask(user, unit, itemId);
            if (task == null)
            {
                if (!UtilInventory.IsAddItem(itemId, unit.Unit.InventorySize, count, unit.Entities.Entities))
                {
                    user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                    StopCraft(unit);
                    return false;
                }
            }
            else
            {
                //check inventory location
                if (task.UserId != 0)
                {
                    var container = moduleMap.Map.FindContainer(itemId, count);
                    if (container.Item1 == null)
                    {
                        StopCraft(unit);
                        return false;
                    }
                }
            }

            var info = Info.EntityInfo[itemId];

            bool isRent = moduleLocation.Locations[user.LocId].LocationType == LocationType.rent1;

            if (isRent) //check money
            {
                if (user.Money < info.costCraft)
                {
                    user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("no_money")));
                    StopCraft(unit);
                    return false;
                }
            }



            if (info.workbenchCraft != WorkbenchType.not)//check workbench
            {
                var block = moduleMap.Map.GetBlock(pos);
                if (block.Item1 == null)
                {
                    StopCraft(unit);
                    return false;
                }

                if (Info.EntityInfo[block.Item1.Id].workbenchType != info.workbenchCraft)
                {
                    StopCraft(unit);
                    return false;
                }
            }

            //check materials
            bool isOwner = moduleLocation.Locations[user.LocId].OwnerId == user.Id;

            if (!isOwner)
            {
                if (task != null)
                    return true;

                if (!isRent && info.workbenchCraft != WorkbenchType.not)
                {
                    StopCraft(unit);
                    return false;
                }

                if (!UtilInventory.IsItems(info.craft.items, unit.Entities.Entities))
                {
                    StopCraft(unit);
                    return false;
                }
            }
            else
            {
                if (!UtilInventory.IsItems(info.craft.items, unit.Entities.Entities))
                {
                    StopCraft(unit);
                    return false;
                }
            }

            

            return true;
        }

        private GameTask GetTask(User user, Entity unit, ushort itemId)
        {
            var taskId = unit.Unit.TaskId;
            if (taskId != 0)
            {
                var task = moduleTask.FindTask(taskId);
                if (task != null && task.ExecutorId == user.Id && task.ItemId == itemId && task.LocId == user.LocId && task.Type == GameTaskType.craft)
                {
                    return task;
                }
            }

            return null;
        }

        private void StopCraft(Entity unit)
        {
            unit.Unit.State = UnitState.stand;
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }

        public override void Terminate()
        {
            
        }

        public void Craft(User user, ushort itemId, int count, Vector2w posCraft)
        {
            var unit = moduleUnits.GetUnit(user);

            if (unit == null)
                return;

            var info = Info.EntityInfo[itemId];
            if (info.craft == null || info.craft.items.Count == 0)
                return;

            WorkbenchType workbenchType = WorkbenchType.not;

            if (posCraft != Vector2w.Empty)
            {
                var block = moduleMap.Map.GetBlock(posCraft);

                if (block.Item1 != null && Info.EntityInfo[block.Item1.Id].workbenchType != WorkbenchType.not)
                {
                    workbenchType = Info.EntityInfo[block.Item1.Id].workbenchType;
                    posCraft = block.Item2;
                }
            }
            else
            {
                posCraft = unit.Unit.CellPos;
            }

            bool isOwner = moduleLocation.Locations[user.LocId].OwnerId == user.Id;

            bool isTask = false;

            if (!isOwner)
            {
                var taskId = unit.Unit.TaskId;
                if (taskId != 0)
                {
                    var task = moduleTask.FindTask(taskId);
                    if (task != null && task.ExecutorId == user.Id && task.ItemId == itemId)
                    {
                        isTask = true;
                    }
                }

                if (!isTask && workbenchType == WorkbenchType.not)
                {
                    if(info.workbenchCraft != workbenchType)
                        return;
                }
            }

            unit.Unit.State = UnitState.craft;
            unit.Unit.Action = new UnitAction() { ActionPos = posCraft, ItemId = itemId, Count = count};
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }
    }
}
