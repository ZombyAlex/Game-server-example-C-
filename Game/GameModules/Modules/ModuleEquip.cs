using SWFServer.Data;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleEquip: GameModule
    {
        private ModuleUnits moduleUnits;


        public ModuleEquip(ModuleUnits moduleUnits)
        {
            this.moduleUnits = moduleUnits;
        }

        public override void Update(float dt)
        {
        }

        public override void Terminate()
        {
        }

        public void Equip(User user, int index)
        {
            var unit = moduleUnits.GetUnit(user);
            if(unit == null)
                return;

            if (index >= unit.Entities.Entities.Count)
                return;

            if(unit.Unit.HandItem!= null) return;

            var item = unit.Entities.Entities[index];

            unit.Unit.HandItem = item;
            unit.Entities.Entities.Remove(item);

            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }

        public void UnEquip(User user, int index)
        {
            var unit = moduleUnits.GetUnit(user);
            if (unit == null)
                return;

            if (unit.Entities.Entities.Count >= unit.Unit.InventorySize)
            {
                user?.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                return;
            }

            unit.Entities.Entities.Add(unit.Unit.HandItem);
            unit.Unit.HandItem = null;

            user.SendMsg(new MsgServer(user.Id, MsgServerType.unit, new MsgServerUnit(unit)));
            moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
        }
    }
}
