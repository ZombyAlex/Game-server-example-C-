using System;
using System.Collections.Generic;
using System.Diagnostics;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using SWFServer.Server;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMapUpdate: GameModule
    {
        private ModuleUsers moduleUsers;
        private ModuleMap moduleMap;
        private ModuleMapGrid moduleMapGrid;
        private ModuleUnits moduleUnits;
        private ModuleUseItem moduleUseItem;

        private Data.Map map => moduleMap.Map;


        public ModuleMapUpdate(ModuleUsers moduleUsers, ModuleMap moduleMap, ModuleMapGrid moduleMapGrid, ModuleUnits moduleUnits, ModuleUseItem moduleUseItem)
        {
            this.moduleUsers = moduleUsers;
            this.moduleMap = moduleMap;
            this.moduleMapGrid = moduleMapGrid;
            this.moduleUnits = moduleUnits;
            this.moduleUseItem = moduleUseItem;
        }

        public void SendMap(User user)
        {
            Entity unit = moduleUnits.GetUnit(user);
            if (unit == null)
            {
                user.PosMapGrid = Vector2w.Zero;
            }
            else
            {
                var p = unit.Unit.CellPos;
                user.PosMapGrid = Util.ToMapGrid(p);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var list = UpdateMapGrid(user);

            user.SendMsg(new MsgServer(user.Id, MsgServerType.map, new MsgServerMap(map, list, user.PosMapGrid)));

            sw.Stop();
            Tools.LogShedule("logs/shedule/time_game_" + map.Id + ".txt", "f=SendMap" + " time = " + sw.ElapsedMilliseconds / 1000.0);
        }

        public List<Vector2w> UpdateMapGrid(User user)
        {
            var list = GetMapGrid(user.PosMapGrid);

            List<Vector2w> newList = new List<Vector2w>();

            for (int i = 0; i < list.Count; i++)
            {
                if (user.ViewGridMap.FindIndex(f => f == list[i]) == -1)
                {
                    newList.Add(list[i]);
                    user.ViewGridMap.Add(list[i]);
                }
            }

            for (int i = 0; i < user.ViewGridMap.Count; i++)
            {
                if (user.ViewGridMap[i].GetR(user.PosMapGrid) > 2)
                {
                    user.ViewGridMap.RemoveAt(i);
                    i--;
                }
            }

            Vector2w p = Util.GridToMapPos(user.PosMapGrid);
            user.ViewRect = new WRect(p.x - GameConst.mapGrid * 2, p.y - GameConst.mapGrid * 2, GameConst.mapGrid * 5, GameConst.mapGrid * 5);
            user.ViewRect.Clip(new WRect((short)0, (short)0, map.Size.x, map.Size.y));

            return newList;
        }

        private List<Vector2w> GetMapGrid(Vector2w pos)
        {
            List<Vector2w> list = new List<Vector2w>();

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var p = pos + new Vector2w(x - 2, y - 2);
                    if (map.IsMap(new Vector2w(p.x * GameConst.mapGrid, p.y * GameConst.mapGrid)))
                    {
                        list.Add(p);
                    }
                }
            }

            return list;
        }

        public void UpdateUnitGridPos(Entity unit)
        {
            if (unit.Unit.UserId != 0)
            {
                var user = moduleUsers.GetUser(unit.Unit.UserId);
                if (user != null)
                {
                    var p = unit.Unit.CellPos;
                    var pGrid = Util.ToMapGrid(p);
                    //user.PosMapGrid = pGrid;
                    if (pGrid.GetR(user.PosMapGrid) > 1)
                        SendMap(user);

                    UpdateAroundUnits(user);
                }
            }

            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }

        public void UpdateAroundUnits(User user)
        {
            foreach (var unit in moduleUnits.Units)
            {
                if (user.ViewRect.Contains(unit.Value.Unit.CellPos))
                    user.SendMsg(new MsgServer(user.Id, MsgServerType.unitAvatar, new MsgServerUnitAvatar(unit.Value.Unit.GetAvatar())));
            }
        }

        public bool IsMove(Vector2w pos, uint userId, bool isTransitionCell)
        {
            if (!Info.EntityInfo[map[pos].Ground.Id].isMove) return false;

            var block = map[pos].Block;
            if (block != null)
            {
                var info = Info.EntityInfo[block.Id];

                if (info.isBlockUnit && isTransitionCell)
                {
                    if (moduleMapGrid.IsUnit(pos))
                        return false;
                }

                //return info.isMove;
            }

            return map.IsMove(pos);
        }

        public void LeftClick(User user, Vector2w pos)
        {
            if (user.UnitId == 0)
                return;

            if (!map.IsMap(pos))
                return;

            if(!moduleUnits.Units.ContainsKey(user.UnitId))
                return;

            var unit = moduleUnits.Units[user.UnitId];

            var block = map.GetBlock(pos);

            int r = unit.Unit.CellPos.GetR(pos);

            var ground = map[pos].Ground;
            if (ground != null && Info.EntityInfo[ground.Id].isWater && r < 2)
            {
                if (unit.Unit.HandItem != null && Info.EntityInfo[unit.Unit.HandItem.Id].isLiquidContainer)
                {
                    unit.Unit.State = UnitState.miningWater;
                    unit.Unit.Action = new UnitAction() { ActionPos = pos };
                    moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
                }
                else
                {
                    moduleUseItem.Drink(unit, 1);
                    moduleUnits.SendUserAttr(unit, UnitAttrType.saturation, unit.Unit.Attr[UnitAttrType.saturation]);
                }

                return;
            }


            if (block.Item1 != null && r < 2)
            {
                var info = Info.EntityInfo[block.Item1.Id];
                if (info.isSwitch)
                {
                    var info1 = Info.EntityInfo[info.switchItem];
                    block.Item1.Id = info1.id;
                    SendAllCell(pos);
                    //map.SwitchBlocks.Add(new MapSwitchBlock() { pos = pos, time = ServerData.serverTime + 5f });
                    return;
                }

                if (info.res != null && info.res.Count > 0 && block.Item1.Entities.Entities.Count > 0) //mining
                {
                    unit.Unit.State = UnitState.mining;
                    unit.Unit.Action = new UnitAction() { ActionPos = pos };
                    moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
                    return;
                }

                if (info.isLiquidContainer)
                {
                    if (unit.Unit.HandItem != null && Info.EntityInfo[unit.Unit.HandItem.Id].isLiquidContainer)
                    {
                        unit.Unit.State = UnitState.miningWater;
                        unit.Unit.Action = new UnitAction() { ActionPos = pos };
                        moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
                        return;
                    }
                    else
                    {
                        //moduleUseItem.Drink(unit, 1);//TODO
                    }

                }

                if (info.containerSize > 0) //container open
                {
                    user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(block.Item1.Entities.Entities, block.Item2)));
                }
            }


            


        }

        public void SendAllCell(Vector2w pos)
        {
            MapCell cell = map[pos];

            var userList = moduleUsers.UserList;
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].ViewRect.Contains(pos))
                    userList[i].SendMsg(new MsgServer(userList[i].Id, MsgServerType.mapCell, new MsgServerMapCell(cell, pos)));
            }
        }

        public void RightClick(User user, Vector2w pos)
        {
            if (user.UnitId == 0)
                return;
            if (!map.IsMap(pos))
                return;
        }

        public override void Update(float dt)
        {
            throw new NotImplementedException();
        }

        public override void Terminate()
        {
        }
    }
}
