using SWFServer.Data.Entities;
using SWFServer.Game.Units.Controllers;

namespace SWFServer.Game.Units.Modifiers
{
    public class UnitModifier
    {
        protected UnitController controller;

        protected Entity unit => controller.Unit;

        public UnitModifier(UnitController controller)
        {
            this.controller = controller;
        }

        public virtual void Update(float dt)
        {

        }
    }
}
