using System.Collections.Generic;
using System.IO;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleUnits: GameModule
    {
        private Dictionary<uint, Entity> units = new Dictionary<uint, Entity>();
        private ModuleMap moduleMap;
        private ModuleUsers moduleUsers;
        private ModuleMapGrid moduleMapGrid;
        private ModuleLocation moduleLocation;

        private string path => "data/maps/map_units" + moduleMap.Map.Id + ".dat";
        public Dictionary<uint, Entity> Units => units;

        public ModuleUnits(ModuleMap moduleMap, ModuleUsers moduleUsers, ModuleMapGrid moduleMapGrid, ModuleLocation moduleLocation)
        {
            this.moduleMap = moduleMap;
            this.moduleUsers = moduleUsers;
            this.moduleMapGrid = moduleMapGrid;
            this.moduleLocation = moduleLocation;

            Load();
        }

        private bool Load()
        {
            if (!File.Exists(path))
                return false;

            using (var stream = File.Open(path, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    Read(reader);
                }
            }

            return true;
        }

        public void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                Entity unit = Entity.Read(reader);
                
                if (unit.Unit.UserId == 0)
                {
                    Units.Add(unit.Unit.Id, unit);
                    moduleMapGrid.AddUnitGrid(unit);
                }
            }
        }

        private void Save()
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    int count = 0;
                    foreach (var unit in Units)
                    {
                        if (unit.Value.Unit.UserId == 0)
                            count++;
                    }

                    writer.Write(count);

                    foreach (var unit in Units)
                    {
                        if (unit.Value.Unit.UserId == 0)
                            unit.Value.Write(writer);
                        else
                            SaveUnit(unit.Value);
                    }
                }
            }
        }

        private Entity LoadUnit(uint unitId)
        {
            string filename = "data/units/unit" + unitId+ ".dat";

            if (!File.Exists(filename))
                return null;
            using (var stream = File.Open(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    Entity unit = Entity.Read(reader);
                    return unit;
                }
            }
        }

        public void CheckLoadUnit(User user)
        {
            uint id = user.UnitId;
            if(units.ContainsKey(id))
                return;

            var unit = LoadUnit(id);
            if (unit != null)
            {
                units.Add(unit.Unit.Id, unit);
                moduleMapGrid.AddUnitGrid(unit);
            }
            else
            {
                user.LocId = 1;
                user.UnitId = 0;
            }
        }

        private void SaveUnit(Entity unit)
        {
            string filename = "data/units/unit" + unit.Unit.Id + ".dat";

            using (var stream = File.Open(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    unit.Write(writer);
                }
            }
        }

        public void AddUnit(Entity unit)
        {
            if(units.ContainsKey(unit.Unit.Id))
                return;

            units.Add(unit.Unit.Id, unit);
            moduleMapGrid.AddUnitGrid(unit);
        }

        public void UnloadUnit(User user)
        {
            var unit = GetUnit(user);
            if (unit != null)
            {
                SaveUnit(unit);
                units.Remove(unit.Unit.Id);
                moduleMapGrid.RemoveUnitGrid(unit);
                SendAllHideUnit(unit.Unit.Id, unit.Unit.CellPos);
            }
        }

        public override void Update(float dt)
        {
           
        }

        public override void Terminate()
        {
            Save();
        }

        public Entity GetUnit(User user)
        {
            if (units.ContainsKey(user.UnitId))
                return units[user.UnitId];
            return null;
        }

        public void CreateUnit(User user)
        {
            if (user.UnitId != 0)
            {
                if (units.ContainsKey(user.UnitId))
                    return;
            }

            Vector2w pos = moduleMap.GetFreeRespawn();
            if (pos == Vector2w.Empty)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("respawns_busy")));
                return;
            }

            Entity unit = Entity.Create("unit");
            //unit.AddComponent(new Component(ComponentClass.unit, ComponentType.unit));
            unit.Unit.Id = WorldManager.UserManager.GetUnitId();
            unit.Unit.Pos = Util.ToVector2F(pos) + new Vector2f(GameConst.cellSize / 2, GameConst.cellSize / 2);
            unit.Unit.UserId = user.Id;
            unit.Unit.State = UnitState.stand;

            user.UnitId = unit.Unit.Id;

            //d.Map[pos].Block.UserId.Value = user.Value; клетка не принадлежит игроку
            //moduleMap.SendAllCell(pos);
            SendAllUnitAvatar(unit.Unit.GetAvatar());
            units.Add(unit.Unit.Id, unit);
            moduleMapGrid.AddUnitGrid(unit);

            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
            SaveUnit(unit);
        }

        public void SendAllUnitAvatar(UnitAvatar avatar)
        {
            var userList = moduleUsers.UserList;
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].ViewRect.Contains(Util.ToVector2W(avatar.Pos)))
                    userList[i].SendMsg(new MsgServer(userList[i].Id, MsgServerType.unitAvatar, new MsgServerUnitAvatar(avatar)));
            }
        }

        public void SendAllHideUnit(uint unitId, Vector2w pos)
        {
            var userList = moduleUsers.UserList;
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].ViewRect.Contains(pos))
                    userList[i].SendMsg(new MsgServer(userList[i].Id, MsgServerType.hideUnit, new MsgServerHideUnit(unitId)));
            }
        }

        public void SendUserAttr(Entity unit, UnitAttrType t, float val)
        {
            if (unit.Unit.UserId != 0)
            {
                User user = moduleUsers.GetUser(unit.Unit.UserId);
                if (user != null)
                    user.SendMsg(new MsgServer(user.Id, MsgServerType.unitAttr, new MsgServerUnitAttr(t, val)));
            }
        }

        private void UnitCenterCell(ComponentUnit u, Vector2w pos)
        {
            u.Velocity = Vector2f.Zero;
            u.Pos = Util.ToVector2F(pos) + new Vector2f(GameConst.cellSize / 2, GameConst.cellSize / 2);
            SendAllUnitAvatar(u.GetAvatar());
        }

        public void Sleep(User user)
        {
            if (user.UnitId == 0) return;
            var unit = GetUnit(user);
            if (unit == null) return;

            if (moduleLocation.Locations[user.LocId].OwnerId != user.Id) return;

            var list = moduleMap.Map.GetBlocks(unit.Unit.CellPos, 1);

            var block = list.Find(f => f != null && Info.EntityInfo[f.Id].isSleep);
            if (block == null) return;

            var pos = moduleMap.Map.GetBlockPos(unit.Unit.CellPos, 1, block);

            var u = unit.Unit;
            u.State = UnitState.lie;
            UnitCenterCell(u, pos);
        }

        public void Sit(User user)
        {
            if (user.UnitId == 0)
                return;

            /*
            var unit = d.Map.Units[user.UnitId];
            var cell = d.Map[unit.Unit.CellPos];
            if (cell.Block == null)
                return;
            if (!Info.EntityInfo[cell.Block.Value].isSit)
                return;

            var u = unit.Unit;
            u.State = UnitState.sit;
            UnitCenterCell(u, u.CellPos);
            */
        }

        public void Shower(User user)
        {
            if (user.UnitId == 0)
                return;
            /*
            var unit = d.Map.Units[user.UnitId];
            var cell = d.Map[unit.Unit.CellPos];
            if (cell.Block == null)
                return;
            if (!Info.EntityInfo[cell.Block.Value].isShower)
                return;

            var u = unit.Unit;
            u.State = UnitState.shower;
            UnitCenterCell(u, u.CellPos);
            */
        }


        public User GetUser(uint userId)
        {
            return moduleUsers.GetUser(userId);
        }

        public void RequestSkills(User user)
        {
            var unit = GetUnit(user);
            if (unit != null)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
            }
        }
    }
}
