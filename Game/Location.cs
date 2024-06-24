using System;
using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Game.GameModules;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Server.Net;

namespace SWFServer.Game
{
    public class Location
    {
        private LocationType locationType;
        private uint ownerId;

        private ModuleSystem moduleSystem;

        public ModuleSystem ModuleSystem => moduleSystem;

        public LocationType LocationType => locationType;
        public uint OwnerId => ownerId;

        private ModuleLocation moduleLocation;

        private float timeUnload = 0;
        private ModuleUsers moduleUsers;
        private uint locId;

        public Location(ModuleLocation moduleLocation, uint locId, ModuleUsers moduleUsers, ModuleTask moduleTask, LocationType locationType, uint ownerId, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            this.locationType = locationType;
            this.ownerId = ownerId;
            this.moduleUsers = moduleUsers;
            this.locId = locId;

            this.moduleLocation = moduleLocation;
            moduleSystem = new ModuleSystem(locId, moduleUsers, moduleTask, moduleLocation, locationType, createLocation, exit, level);
            timeUnload = 30f;
        }
        
        public void NetMsg(MsgClient msg, User user)
        {
            switch (msg.Type)
            {
                case MsgClintType.login:
                    break;

                case MsgClintType.id:
                {
                    MsgClientId m = (MsgClientId)msg.Data;
                    switch (m.type)
                    {
                        case MsgClientTypeId.requestUserName:
                            WorldManager.UserManager.RequestUserName(user, m.val);
                                break;
                        case MsgClientTypeId.removeTask:
                            moduleSystem.ModuleTask.RemoveTask(user, m.val);
                            break;
                        case MsgClientTypeId.performTask:
                            moduleSystem.ModuleTask.PerformTask(user, m.val);
                                break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    
                }
                    break;

                case MsgClintType.chat:
                {
                    MsgClientChat m = (MsgClientChat)msg.Data;
                    if (!moduleSystem.ModuleCheat.TryCheat(user, m))
                        NetMaster.instance.ChatMsg(msg);
                }
                    break;
                case MsgClintType.signal:
                {
                    MsgClientSignal m = (MsgClientSignal)msg.Data;
                    switch (m.signal)
                    {
                        case MsgClintTypeSignal.createUnit:
                            moduleSystem.ModuleUnits.CreateUnit(user);
                            break;
                        case MsgClintTypeSignal.sleep:
                            moduleSystem.ModuleUnits.Sleep(user);
                            break;
                        case MsgClintTypeSignal.sit:
                            moduleSystem.ModuleUnits.Sit(user);
                            break;
                        case MsgClintTypeSignal.shower:
                            moduleSystem.ModuleUnits.Shower(user);
                            break;
                        case MsgClintTypeSignal.exitLocation:
                            ExitLocation(user);
                            break;
                        case MsgClintTypeSignal.cancelTask:
                            moduleSystem.ModuleTask.CancelTask(user);
                            break;
                        case MsgClintTypeSignal.requestSkills:
                            moduleSystem.ModuleUnits.RequestSkills(user);
                            break;
                            default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                case MsgClintType.pos:
                {
                    MsgClientPos m = (MsgClientPos)msg.Data;
                    switch (m.type)
                    {
                        case MsgClintTypePos.leftClick:
                            moduleSystem.ModuleMapUpdate.LeftClick(user, m.pos);
                            break;
                        case MsgClintTypePos.rightClick:
                            moduleSystem.ModuleMapUpdate.RightClick(user, m.pos);
                            break;
                        case MsgClintTypePos.takeBlock:
                            moduleSystem.ModuleMoveItems.TakeBlock(user, m.pos);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                case MsgClintType.inputKey:
                {
                    MsgClientInputKey m = (MsgClientInputKey)msg.Data;
                    moduleSystem.ModuleUnitsUpdate.InputKey(user, m.dir);
                }
                    break;

                case MsgClintType.getTask:
                {
                    MsgClientGetTask m = (MsgClientGetTask)msg.Data;
                    moduleSystem.ModuleTask.GetTask(user, m.taskId);
                }
                    break;
                case MsgClintType.craft:
                {
                    MsgClientCraft m = (MsgClientCraft)msg.Data;
                    moduleSystem.ModuleCraft.Craft(user, m.itemId, m.count, m.pos);
                }
                    break;
                case MsgClintType.index:
                {
                    MsgClientIndex m = (MsgClientIndex)msg.Data;
                    switch (m.type)
                    {
                        case MsgClientTypeIndex.equip:
                            moduleSystem.ModuleEquip.Equip(user, m.index);
                            break;
                        case MsgClientTypeIndex.unEquip:
                            moduleSystem.ModuleEquip.UnEquip(user, m.index);
                            break;
                        case MsgClientTypeIndex.useItem:
                            moduleSystem.ModuleUseItem.UseItem(user, m.index); 
                            break;
                            
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
                    break;
                case MsgClintType.build:
                {
                    MsgClientBuild m = (MsgClientBuild)msg.Data;
                    moduleSystem.ModuleBuild.Build(user, m.inventoryPos, m.pos, m.rotate);
                }
                    break;
                case MsgClintType.moveItems:
                {
                    MsgClientMoveItems m = (MsgClientMoveItems)msg.Data;
                    moduleSystem.ModuleMoveItems.MoveItems(user, m.pos, m.items, m.isGet);
                }
                    break;
                case MsgClintType.task:
                {
                    MsgClientTask m = (MsgClientTask)msg.Data;
                    moduleSystem.ModuleTask.CreateTask(user, m.task);
                }
                    break;
                case MsgClintType.buy:
                {
                    MsgClientBuy m = (MsgClientBuy)msg.Data;
                    moduleSystem.ModuleTrade.Buy(user, m.pos, m.index, m.itemId, m.count);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ExitLocation(User user)
        {
            if (user.UnitId == 0)
                return;

            if (user.TimeCollDownEnterLoc > 0)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("location_entry_break")));
                return;
            }

            var unit = moduleSystem.ModuleUnits.GetUnit(user);


            List<Entity> list = moduleSystem.ModuleMap.Map.GetBlocks(unit.Unit.CellPos, 1);

            var exit = list.Find(f => Info.EntityInfo[f.Id].isExit);
            if (exit == null)
                return;

            uint locId = exit.ExitLocId.Value;
            Vector2w pos = exit.ExitPos.Pos;

            ModuleSystem.ModuleUnits.UnloadUnit(user);

            moduleLocation.TransitionLocation(user, unit, locId, pos);
        }

        public void Terminate()
        {
            Save();
            moduleSystem.Terminate();
        }

        private void Save()
        {
            moduleSystem.ModuleMap.Map.Save();
        }

        public void Update(float dt)
        {
            moduleSystem.Update(dt);

            if (IsLocUnload() && !IsUserInLoc())
                timeUnload -= dt;
            else
                timeUnload = 30f;
        }

        private bool IsUserInLoc()
        {
            return moduleUsers.UserList.Find(f => f.LocId == locId) != null;
        }

        public bool IsUnload()
        {
            return timeUnload < 0;
        }

        private bool IsLocUnload()
        {
            switch (locationType)
            {
                case LocationType.world:
                    return false;
                case LocationType.respawn:
                    return false;
                case LocationType.work1:
                    return false;
                case LocationType.rent1:
                    return false;
                case LocationType.house_wood:
                    return true;
                case LocationType.factory1:
                    return true;
                case LocationType.cave:
                    return true;
                case LocationType.buying:
                    return false;
                case LocationType.shop:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
