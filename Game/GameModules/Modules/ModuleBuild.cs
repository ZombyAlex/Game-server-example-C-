using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleBuild: GameModule
    {
        private ModuleUnits moduleUnits;
        private ModuleLocation moduleLocation;

        private FuncCreateLocation funcCreateLocation;

        public ModuleBuild(ModuleUnits moduleUnits, ModuleLocation moduleLocation, FuncCreateLocation funcCreateLocation)
        {
            this.moduleUnits = moduleUnits;
            this.moduleLocation = moduleLocation;
            this.funcCreateLocation = funcCreateLocation;
        }

        public override void Update(float dt)
        {
            
        }

        public override void Terminate()
        {
            
        }

        public void Build(User user, int inventoryPos, Vector2w pos, int rotate)
        {
            var unit = moduleUnits.GetUnit(user);

            if (unit == null)
                return;

            if(inventoryPos>= unit.Entities.Entities.Count)
                return;

            var item = unit.Entities.Entities[inventoryPos];

            var info = Info.EntityInfo[item.Id];
            if (info.build == null)
                return;

            if (info.build.cost > user.Money)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("no_money")));
                return;
            }

            //check location

            var loc = moduleLocation.Locations[user.LocId];


            if (!info.build.IsLocation(loc.LocationType))
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("not_build_location")));
                return;
            }

            var p = pos;
            var size = info.size;
            var s = size;
            if (info.build.location != LocationType.world)
            {
                p += Vector2w.Empty;
                size += new Vector2w(2, 2);
            }

            if (rotate == 1 || rotate == 3) s.Swap();


            //check location pos
            if (loc.LocationType == LocationType.world)
            {
                if (CheckBuildZone(user, pos)) return;
                if (CheckBuildZone(user, pos + s + Vector2w.Empty)) return;
            }


            //check free place
            var map = loc.ModuleSystem.ModuleMap.Map;

            if (!map.IsFree(p, size, (byte)rotate))
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("build_place_occupied")));
                return;
            }

            if (loc.ModuleSystem.ModuleMapGrid.IsUnit(p, s))
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("build_place_occupied")));
                return;
            }

            if (info.layer == EntityMapLayer.floor)
            {
                if (map[pos].Floor != null)
                {
                    user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("build_place_occupied")));
                    return;
                }
            }

            //complete
            user.Money -= info.build.cost;
            user.SendMsg(new MsgServer() { UserId = user.Id, Data = new MsgServerMoney(user.Money), Type = MsgServerType.money });
            Entity build = item.Count.Value > 1 ? Entity.Create(info.id) : item;
            if (build.Rotate == null)
                build.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
            build.Rotate.Value = (byte)rotate;

            

            map.SetEntity(pos, build);
            loc.ModuleSystem.ModuleMapUpdate.SendAllCell(pos);

            if (item.Count.Value > 1)
            {
                item.Count.Value--;
                if (item.Count.Value <= 0)
                    unit.Entities.Entities.RemoveAt(inventoryPos);
            }
            else
                unit.Entities.Entities.RemoveAt(inventoryPos);

            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));

            if (info.build.location != LocationType.world)
            {
                var res = funcCreateLocation(user.LocId, info.build.location, user.Id, pos, 0);

                build.AddComponent(new Component(ComponentClass.valUint, ComponentType.id));
                build.UserId.Value = user.Id;

                build.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
                build.ExitLocId.Value = res.Item1;
                build.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
                build.ExitPos.Pos = res.Item2;
            }
        }

        private bool CheckBuildZone(User user, Vector2w pos)
        {
            bool isPlace = false;

            GameConst.buildPlaces.ForEach(f =>
            {
                if (f.Contains(pos))
                    isPlace = true;
            });

            if (!isPlace)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("not_build_place")));
                return true;
            }

            return false;
        }
    }
}
