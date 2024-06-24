using System;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Game.GameModules;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Game.Units.Controllers;

namespace SWFServer.Game.Units.Modifiers
{
    public class UnitModifierMovePlayer: UnitModifier
    {
        private ModuleMapGrid moduleMapGrid;
        private ModuleUnits moduleUnits;
        private ModuleMapUpdate moduleMapUpdate;
        private ModuleMap moduleMap;

        public UnitModifierMovePlayer(UnitController controller, ModuleMapGrid moduleMapGrid, ModuleUnits moduleUnits, ModuleMapUpdate moduleMapUpdate, ModuleMap moduleMap) : base(controller)
        {
            this.moduleMapGrid = moduleMapGrid;
            this.moduleUnits = moduleUnits;
            this.moduleMapUpdate = moduleMapUpdate;
            this.moduleMap = moduleMap;
        }

        public override void Update(float dt)
        {
            Vector2f dir = controller.KeyDirection;

            var oldVel = unit.Unit.Velocity;

            var newVel = Velocity(dir);

            Vector2f newPos = NewPos(newVel, dt);


            if (newVel.sqrMagnitude > 0 && !TryMove(newPos, newVel))
            {

                var angle = Vector2f.SignedAngle(new Vector2f(0, 1), newVel);

                var ang = (float)(Math.Round(angle / 90f) * 90f);

                newVel = new Vector2f(0, 1);
                newVel.Rotate(ang);
                newVel = Velocity(newVel);
                var p = NewPos(Velocity(newVel), dt);
                if (!TryMove(p, newVel))
                {
                    float a = angle - ang;

                    if (a > 0)
                        ang = (float)(Math.Ceiling(angle / 90f) * 90f);
                    else
                        ang = (float)(Math.Floor(angle / 90f) * 90f);

                    newVel = new Vector2f(0, 1);
                    newVel.Rotate(ang);
                    newVel = Velocity(newVel);
                    p = NewPos(Velocity(newVel), dt);
                    if (!TryMove(p, newVel))
                        newVel = Vector2f.Zero;
                }
            }

            unit.Unit.Velocity = newVel;
            newPos = NewPos(newVel, dt);

            Vector2w oldPosGrid = Util.ToMapGrid(unit.Unit.CellPos);
            Vector2w oldPosCell = unit.Unit.CellPos;

            unit.Unit.Pos = newPos;

            if (unit.Unit.CellPos != oldPosCell)
                moduleMapGrid.MoveUnitGrid(unit, oldPosCell);

            Vector2w newPosGrid = Util.ToMapGrid(unit.Unit.CellPos);
            if (oldPosGrid != newPosGrid)
            {
                moduleMapUpdate.UpdateUnitGridPos(unit);
            }

            if ((oldVel - newVel).sqrMagnitude > 0)
            {
                if (unit.Unit.State != UnitState.stand)
                {
                    unit.Unit.State = UnitState.stand;
                }

                moduleUnits.SendAllUnitAvatar(unit.Unit.GetAvatar());
            }
        }

        Vector2f Velocity(Vector2f direction)
        {
            direction.Normalize();
            float speed = moduleMap.Map.GetSpeedMove(unit.Unit.CellPos);
            return direction * speed;
        }

        Vector2f NewPos(Vector2f velocity, float dt)
        {
            return unit.Unit.Pos + velocity * dt;
        }

        private bool TryMove(Vector2f pos, Vector2f direction)
        {
            direction.Normalize();

            pos += direction * 0.5f;
            var p = Util.ToVector2W(pos);
            if (!moduleMap.Map.IsMap(p))
                return false;
            return moduleMapUpdate.IsMove(p, unit.Unit.UserId, p != unit.Unit.CellPos);
        }
    }
}
