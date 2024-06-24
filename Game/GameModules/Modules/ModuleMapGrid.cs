using System;
using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMapGrid: GameModule
    {
        private class MapUnitGrid
        {
            public List<Entity> Units = new List<Entity>();
        }


        private MapUnitGrid[,] unitGrid;

        private ModuleMap moduleMap;


        public ModuleMapGrid(ModuleMap moduleMap)
        {
            this.moduleMap = moduleMap;
            InitUnitGrid();
        }


        private void InitUnitGrid()
        {
            var s = Util.SizeMapUnitGrid(moduleMap.Map.Size);
            unitGrid = new MapUnitGrid[s.x, s.y];
            for (int x = 0; x < s.x; x++)
            {
                for (int y = 0; y < s.y; y++)
                {
                    unitGrid[x, y] = new MapUnitGrid();
                }
            }
        }

        public void AddUnitGrid(Entity unit)
        {
            var p = Util.ToMapUnitGrid(unit.Unit.CellPos);
            unitGrid[p.x, p.y].Units.Add(unit);
        }

        public void RemoveUnitGrid(Entity unit)
        {
            var p = Util.ToMapUnitGrid(unit.Unit.CellPos);
            unitGrid[p.x, p.y].Units.Remove(unit);
        }

        public void MoveUnitGrid(Entity unit, Vector2w oldPos)
        {
            var pOld = Util.ToMapUnitGrid(oldPos);
            unitGrid[pOld.x, pOld.y].Units.Remove(unit);
            AddUnitGrid(unit);
        }

        public bool IsUnit(Vector2w pos)
        {
            var p = Util.ToMapUnitGrid(pos);
            var grid = unitGrid[p.x, p.y];
            for (int i = 0; i < grid.Units.Count; i++)
            {
                if (grid.Units[i].Unit.CellPos == pos)
                    return true;
            }

            return false;
        }

        public bool IsUnit(Vector2w pos, Vector2w size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var p = pos + new Vector2w(x, y);
                    if (moduleMap.Map.IsMap(p) && IsUnit(p))
                        return true;
                }
            }
            return false;
        }

        public override void Update(float dt)
        {
            throw new NotImplementedException();
        }

        public override void Terminate()
        {
        }

        
    }
}
