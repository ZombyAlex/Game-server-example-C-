using System;
using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMining: GameModule
    {
        private ModuleUnits moduleUnits;
        private ModuleMap moduleMap;
        private ModuleMapUpdate moduleMapUpdate;


        public ModuleMining(ModuleUnits moduleUnits, ModuleMap moduleMap, ModuleMapUpdate moduleMapUpdate)
        {
            this.moduleUnits = moduleUnits;
            this.moduleMap = moduleMap;
            this.moduleMapUpdate = moduleMapUpdate;
        }


        public override void Update(float dt)
        {
            foreach (var unit in moduleUnits.Units)
            {
                var u = unit.Value.Unit;
                if (u.State == UnitState.mining)
                {
                    if (u.Action == null)
                    {
                        StopMining(unit.Value);
                        continue;
                    }
                    var block = moduleMap.Map.GetBlock(u.Action.ActionPos);
                    if (block.Item1 == null)
                    {
                        StopMining(unit.Value);
                        continue;
                    }
                    User user = moduleUnits.GetUser(u.UserId);

                    var info = Info.EntityInfo[block.Item1.Id];
                    float mFactor = 1f;
                    if (info.toolMining != null)
                    {
                        bool isAvailable = false;
                        if (u.HandItem != null)
                        {
                            var infoEquip = Info.EntityInfo[u.HandItem.Id];
                            if (infoEquip.tool != null && infoEquip.tool.type == info.toolMining.type && infoEquip.tool.level >= info.toolMining.level)
                            {
                                isAvailable = true;
                                mFactor = infoEquip.timeMining;
                            }
                        }

                        if (!isAvailable)
                        {
                            user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("tool_required")));
                            StopMining(unit.Value);
                            continue;
                        }
                    }

                    int count = 1;

                    var item = block.Item1.Entities.Entities.Find(f => f.Count.Value > 0);

                   

                    if (!UtilInventory.IsAddItem(item.Id, u.InventorySize, count, unit.Value.Entities.Entities))
                    {
                        user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                        StopMining(unit.Value);
                        continue;
                    }

                    var miningInfo = Info.EntityInfo[item.Id];

                    if (!u.Action.IsStart)
                    {
                        float t = miningInfo.timeMining * mFactor / u.Skills.Factor(miningInfo.miningType);
                        u.Action.TimeAction = ServerData.serverTime + t;
                        user?.SendMsg(new MsgServer(user.Id, MsgServerType.unitAction, new MsgServerUnitAction(t)));
                        u.Action.IsStart = true;
                    }
                    else
                    {
                        if (ServerData.serverTime >= u.Action.TimeAction)
                        {
                            //complete
                            UtilInventory.AddItem(item, count, block.Item1.Entities.Entities, unit.Value.Entities.Entities);
                            user?.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Value.Entities.Entities, Vector2w.Empty)));

                            u.Skills.Change(miningInfo.miningType, 1f);
                            u.Skills.Regression(miningInfo.miningType);

                            if (block.Item1.Entities.Entities.Count == 0)
                            {
                                moduleMap.Map.RemoveEntity(u.Action.ActionPos, EntityMapLayer.block);
                                moduleMapUpdate.SendAllCell(block.Item2);
                                StopMining(unit.Value);
                            }
                            else
                            {
                                float t = miningInfo.timeMining * mFactor / u.Skills.Factor(miningInfo.miningType);
                                u.Action.TimeAction = ServerData.serverTime + t ;
                                user?.SendMsg(new MsgServer(user.Id, MsgServerType.unitAction, new MsgServerUnitAction(t)));
                            }

                            if (info.toolMining != null)
                            {
                                u.HandItem.Durability.Value -= 1.0f / u.Skills.Factor(miningInfo.miningType);
                                if (u.HandItem.Durability.Value <= 0)
                                {
                                    u.HandItem = null;
                                    user?.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit.Value)));
                                    moduleUnits.SendAllUnitAvatar(u.GetAvatar());
                                }
                                else
                                {
                                    user?.SendMsg(new MsgServer(user.Id, MsgServerType.entity, new MsgServerEntity(u.HandItem, -1)));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void StopMining(Entity unit)
        {
            unit.Unit.State = UnitState.stand;
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }

        public override void Terminate()
        { }
    }
}
