using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Data;
using SWFServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMiningWater: GameModule
    {
        private ModuleUnits moduleUnits;
        private ModuleMap moduleMap;
        private ModuleLocation moduleLocation;

        public ModuleMiningWater(ModuleUnits moduleUnits, ModuleMap moduleMap, ModuleLocation moduleLocation)
        {
            this.moduleUnits = moduleUnits;
            this.moduleMap = moduleMap;
            this.moduleLocation = moduleLocation;   
        }

        public override void Update(float dt)
        {
            foreach (var unit in moduleUnits.Units)
            {
                var u = unit.Value.Unit;
                if (u.State == UnitState.miningWater)
                {
                    if (u.Action == null)
                    {
                        StopMining(unit.Value);
                        continue;
                    }
                    User user = moduleUnits.GetUser(u.UserId);
                    var loc = moduleLocation.Locations[user.LocId];
                    if (loc.LocationType != LocationType.world && loc.OwnerId != user.Id)
                    {
                        StopMining(unit.Value);
                        continue;
                    }

                    if (u.HandItem == null)
                    {
                        StopMining(unit.Value);
                        continue;
                    }

                    ushort waterId = Info.EntityInfo["water"].id;

                    Entity source = null;

                    if (CheckGroundLiquid(u, unit))
                    {
                        if (CheckBlockLiquid(u, unit, ref source))
                        {
                            StopMining(unit.Value);
                            continue;
                        }
                        waterId = source.Id;
                    }

                    var item = u.HandItem;

                    if (!Info.EntityInfo[item.Id].isLiquidContainer)
                    {
                        StopMining(unit.Value);
                        continue;
                    }

                    int need = 0;
                    if (item.Entities.Entities.Count > 0)
                    {
                        if (item.Entities.Entities[0].Id != waterId)//нельзя налить другую жидкость
                        {
                            user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                            StopMining(unit.Value);
                            continue;
                        }

                        need = Info.EntityInfo[item.Id].liquidSize - item.Entities.Entities[0].Count.Value;

                        if (need <= 0)
                        {
                            user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                            StopMining(unit.Value);
                            continue;
                        }
                    }
                    else
                    {
                        need = Info.EntityInfo[item.Id].liquidSize;
                    }

                    if (!u.Action.IsStart)
                    {
                        float t = 5f;
                        u.Action.TimeAction = ServerData.serverTime + t;
                        user?.SendMsg(new MsgServer(user.Id, MsgServerType.unitAction, new MsgServerUnitAction(t)));
                        u.Action.IsStart = true;
                    }
                    else
                    {
                        if (ServerData.serverTime >= u.Action.TimeAction)
                        {
                            //complete
                            if (source != null)
                            {
                                if(need > source.Count.Value)
                                    need = source.Count.Value;

                                source.Count.Value -= need;
                                if (source.Count.Value <= 0)
                                {
                                    var block = moduleMap.Map[u.Action.ActionPos].Block;
                                    block.Entities.Entities.RemoveAt(0);
                                }
                            }


                            if (item.Entities.Entities.Count > 0)
                            {
                                item.Entities.Entities[0].Count.Value += need;
                            }
                            else
                            {
                                Entity water = Entity.Create(waterId);
                                water.Count.Value = need;
                                item.Entities.Entities.Add(water);
                            }

                            user?.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit.Value)));
                            StopMining(unit.Value);
                        }
                    }
                }
            }
        }

        private bool CheckBlockLiquid(ComponentUnit u, KeyValuePair<uint, Entity> unit, ref Entity source)
        {
            var block = moduleMap.Map.GetBlock(u.Action.ActionPos);
            if (block.Item1 == null)
                return true;

            var info = Info.EntityInfo[block.Item1.Id];
            if (!info.isLiquidContainer || block.Item1.Entities.Entities.Count == 0)
                return true;

            source = block.Item1.Entities.Entities[0];

            if (source.Count.Value <= 0)
                return true;

            return false;
        }

        private bool CheckGroundLiquid(ComponentUnit u, KeyValuePair<uint, Entity> unit)
        {
            var ground = moduleMap.Map[u.Action.ActionPos].Ground;
            if (ground == null)
                return true;

            var info = Info.EntityInfo[ground.Id];
            if (!info.isWater)
                return true;

            return false;
        }

        private void StopMining(Entity unit)
        {
            unit.Unit.State = UnitState.stand;
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }

        public override void Terminate()
        {
            
        }
    }
}
