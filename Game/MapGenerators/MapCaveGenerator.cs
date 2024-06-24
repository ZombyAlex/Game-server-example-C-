using System;
using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Game.MapGenerators
{
    public class MapCaveGenerator : IMapGenerator
    {
        public ITileMap TileMap { get; set; }
        public Vector2w Size { get; set; }
        public void Generation(Rnd rnd, Map map, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            map.InitSize(Size);

            //ground
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);
                    MapCell c = new MapCell();
                    map.SetCell(p, c);
                    c.Ground = Entity.Create("ground3");
                }
            }


            { //create enter
                var info = Info.EntityInfo["cave2"];
                byte rotate = (byte)rnd.Range(0, 4);

                var pos = map.GetFreeRndPos(rnd, info.size, rotate, 1);
                if (pos == Vector2w.Empty)
                {
                    Util.Log("logs/error_create_loc.txt", "no find pos enter");
                    return;
                }

                Entity enter = Entity.Create(info.id);
                enter.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                enter.Rotate.Value = rotate;
                map.SetEntity(pos, enter);

                enter.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
                enter.ExitLocId.Value = exit.Item1;
                enter.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
                enter.ExitPos.Pos = exit.Item2;
                map.Exit = pos;
            }

            int rndN = 50;
            if (level == 0)
                rndN = 50;
            else if (level == 1)
                rndN = 40;
            else if (level == 2)
                rndN = 30;
            else if (level == 3)
                rndN = 20;
            else if (level == 4)
                rndN = 10;
            else
                rndN = 0;


            if (rnd.Range(0, 100) < rndN)
            {
                if (CreateCave(rnd, map, createLocation, level + 1))
                {
                    if (rnd.Range(0, 100) < rndN-10)
                    {
                        if (CreateCave(rnd, map, createLocation, level + 1))
                        {
                            if (rnd.Range(0, 100) < rndN-20)
                            {
                                CreateCave(rnd, map, createLocation, level + 1);
                            }
                        }
                    }
                }
            }

            //blocks
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);

                    if (Info.EntityInfo[map[p].Ground.Id].isMove)
                    {
                        string block = GenerateBlock(rnd, level);
                        var rotate = (byte)rnd.Range(0, 4);
                        if (!string.IsNullOrEmpty(block) && map.IsFree(p, Info.EntityInfo[block].size, rotate))
                        {
                            var entity = Entity.Create(block);
                            entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                            entity.Rotate.Value = rotate;
                            map.SetEntity(p, entity);
                        }
                    }
                }
            }

        }

        private static bool CreateCave(Rnd rnd, Map map, FuncCreateLocation createLocation, int level)
        {
            var info = Info.EntityInfo["cave2"];
            byte rotate = (byte)rnd.Range(0, 4);

            var pos = map.GetFreeRndPos(rnd, info.size, rotate, 1);
            if (pos == Vector2w.Empty)
            {
                Util.Log("logs/error_create_loc.txt", "no find pos exit");
                return false;
            }

            Entity exitCave = Entity.Create(info.id);
            exitCave.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
            exitCave.Rotate.Value = rotate;
            map.SetEntity(pos, exitCave);

            var loc = createLocation(map.Id, LocationType.cave, 0, pos, level);

            exitCave.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
            exitCave.ExitLocId.Value = loc.Item1;
            exitCave.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
            exitCave.ExitPos.Pos = loc.Item2;
            return true;
        }

        private string GenerateBlock(Rnd rnd, int level)//TODO
        {
            string block = string.Empty;


            if(rnd.Range(0, 100)>=30) return String.Empty;

            int r = rnd.Range(0, 100);


            if (level == 0)
            {
                if (r < 20)
                    block = "stone_small";
                else if (r < 40)
                    block = "stone";
                else if (r < 60)
                    block = "stone2";
                else if (r < 80)
                    block = "ore_coal";
                else
                    block = "ore_coal2";
            }
            else if (level == 1)
            {
                if (r < 20)
                    block = "stone_small";
                else if (r < 40)
                    block = "stone";
                else if (r < 50)
                    block = "stone2";
                else if (r < 70)
                    block = "ore_coal";
                else if (r < 80)
                    block = "ore_coal2";
                else if (r < 90)
                    block = "ore_iron";
                else
                    block = "ore_iron2";
            }
            else if (level == 1)
            {
                if (r < 20)
                    block = "stone_small";
                else if (r < 30)
                    block = "stone";
                else if (r < 40)
                    block = "stone2";
                else if (r < 80)
                    block = "ore_iron";
                else
                    block = "ore_iron2";
            }
            else
            {
                if (r < 20)
                    block = "stone_small";
                else if (r < 30)
                    block = "stone";
                else if (r < 40)
                    block = "stone2";
                else if (r < 80)
                    block = "ore_iron";
                else
                    block = "ore_iron2";
            }


            return block;
        }

    }
}
