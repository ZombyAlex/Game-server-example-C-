using SWFServer.Data.Entities;
using SWFServer.Game.GameModules;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Game.Units.Controllers;

namespace SWFServer.Game.Units.Modifiers
{
    public class UnitModifierSatiety: UnitModifier
    {
        private float time = 0;
        private ModuleUnits moduleUnits;
        public UnitModifierSatiety(UnitController controller, ModuleUnits moduleUnits) : base(controller)
        {
            this.moduleUnits = moduleUnits;
        }

        public override void Update(float dt)
        {
            if (unit.Unit.State == UnitState.lie)
                return;

            time += dt;
            if (time > 60f)//100 minutes full
            {
                time -= 60f;
                unit.Unit.Attr.Change(UnitAttrType.satiety, -1);
                moduleUnits.SendUserAttr(unit, UnitAttrType.satiety, unit.Unit.Attr[UnitAttrType.satiety]);
            }
        }
    }
}
