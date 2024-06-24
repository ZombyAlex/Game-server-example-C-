using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Game.GameModules;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Game.Units.Modifiers;


namespace SWFServer.Game.Units.Controllers
{
    public class UnitController: IUnitController
    {
        private Entity unit;

        public Entity Unit => unit;

        private Vector2f keyDirection = Vector2f.Zero;
        public Vector2f KeyDirection => keyDirection;

        private List<UnitModifier> modifiers = new List<UnitModifier>();

        public UnitController(Entity unit, ModuleMap moduleMap, ModuleUnits moduleUnits, ModuleMapGrid moduleMapGrid, ModuleMapUpdate moduleMapUpdate)
        {
            this.unit = unit;

            modifiers.Add(new UnitModifierMovePlayer(this, moduleMapGrid, moduleUnits, moduleMapUpdate, moduleMap));
            modifiers.Add(new UnitModifierSatiety(this, moduleUnits));
            modifiers.Add(new UnitModifierThirst(this, moduleUnits));
        }

        public virtual void Update(float dt)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].Update(dt);
            }
        }

        public void InputKey(Vector2f dir)
        {
            keyDirection = dir;
        }
    }
}
