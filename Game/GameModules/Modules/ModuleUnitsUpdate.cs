using SWFServer.Data;
using SWFServer.Game.Units.Controllers;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleUnitsUpdate: GameModule
    {

        private ModuleUnits moduleUnits;
        private ModuleMap moduleMap;
        private ModuleMapGrid moduleMapGrid;
        private ModuleMapUpdate moduleMapUpdate;


        public ModuleUnitsUpdate(ModuleUnits moduleUnits, ModuleMap moduleMap, ModuleMapGrid moduleMapGrid, ModuleMapUpdate moduleMapUpdate)
        {
            this.moduleUnits = moduleUnits;
            this.moduleMap = moduleMap;
            this.moduleMapGrid = moduleMapGrid;
            this.moduleMapUpdate = moduleMapUpdate;
        }

        public override void Update(float dt)
        {
            foreach (var unit in moduleUnits.Units)
            {
                if (unit.Value.Unit.Controller == null)
                    unit.Value.Unit.Controller = new UnitController(unit.Value, moduleMap, moduleUnits, moduleMapGrid, moduleMapUpdate);
                unit.Value.Unit.Controller.Update(dt);
            }
        }

        public override void Terminate()
        {
            
        }

        public void InputKey(User user, Vector2f dir)
        {
            if (user.UnitId == 0)
                return;

            var unit = moduleUnits.Units[user.UnitId];

            if (unit.Unit.Controller == null)
                unit.Unit.Controller = new UnitController(unit, moduleMap, moduleUnits, moduleMapGrid, moduleMapUpdate);

            unit.Unit.Controller.InputKey(dir);
        }
    }
}
