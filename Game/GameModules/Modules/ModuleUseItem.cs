using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleUseItem: GameModule
    {
        private ModuleUnits moduleUnits;


        public ModuleUseItem(ModuleUnits moduleUnits)
        {
            this.moduleUnits = moduleUnits;
        }

        public override void Update(float dt)
        {
            
        }

        public override void Terminate()
        {
            
        }


        public void UseItem(User user, int index)
        {
            var unit = moduleUnits.GetUnit(user);
            if (unit == null) return;

            if(index >= unit.Entities.Entities.Count) return;

            var item = unit.Entities.Entities[index];

            var info = Info.EntityInfo[item.Id];
            if(!info.isUse) return;

            if (info.containerSize > 0)
            {
                if (item.Entities.Entities.Count > 0)
                {
                    var it = item.Entities.Entities[0];
                    if (Info.EntityInfo[it.Id].isWater && it.Count.Value > 0)
                    {
                        int cnt = Drink(unit, it.Count.Value);

                        if (cnt > 0)
                        {
                            it.Count.Value -= cnt;
                            if (it.Count.Value <= 0)
                                item.Entities.Entities.RemoveAt(0);

                            item.Durability.Value -= 0.1f;

                            if (item.Durability.Value <= 0)
                                unit.Entities.Entities.Remove(item);

                            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
                            moduleUnits.SendUserAttr(unit, UnitAttrType.saturation, unit.Unit.Attr[UnitAttrType.saturation]);
                        }

                        return;
                    }
                }
            }
        }

        public int Drink(Entity unit, int maxCount)
        {
            var need = unit.Unit.Attr.Need(UnitAttrType.saturation);
            if (need > 0)
            {
                int n = (int)(need / 50f);
                if (n > maxCount)
                {
                    n = maxCount;
                }

                unit.Unit.Attr.Change(UnitAttrType.saturation, n * 50f);

                return n;
            }

            return 0;
        }
    }
}
