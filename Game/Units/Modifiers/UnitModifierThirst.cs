using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWFServer.Data.Entities;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Game.Units.Controllers;

namespace SWFServer.Game.Units.Modifiers
{
    public class UnitModifierThirst: UnitModifier
    {
        private float time = 0;
        private ModuleUnits moduleUnits;

        public UnitModifierThirst(UnitController controller, ModuleUnits moduleUnits) : base(controller)
        {
            this.moduleUnits = moduleUnits;
        }

        public override void Update(float dt)
        {
            if (unit.Unit.State == UnitState.lie)
                return;

            time += dt;
            if (time > 60f)//20 minutes full
            {
                time -= 60f;
                unit.Unit.Attr.Change(UnitAttrType.saturation, -5);
                moduleUnits.SendUserAttr(unit, UnitAttrType.saturation, unit.Unit.Attr[UnitAttrType.saturation]);
            }
        }
    }
}
