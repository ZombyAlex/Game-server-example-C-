using System;
using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Game.MapGenerators
{
    public class MapWorldGenerator : IMapGenerator
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
                    int r = rnd.Range(0, 90);
                    if (r < 50)
                        c.Ground = Entity.Create("ground1");
                    else
                        c.Ground = Entity.Create("ground2");
                }
            }

            //floor

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    Vector2w p = new Vector2w(x + 112, y);
                    var entity = Entity.Create("floor_stone");
                    map.SetEntity(p, entity);
                }
            }

            {
                var entity = Entity.Create("monument_sigma");
                entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                map.SetEntity(new Vector2w(126, 46), entity);
            }
            {
                var entity = Entity.Create("monument_space");
                entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                map.SetEntity(new Vector2w(120, 40), entity);
            }
            {
                var entity = Entity.Create("monument_gueng");
                entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                map.SetEntity(new Vector2w(134, 40), entity);
            }


            //locations
            CreateLocation(map, createLocation, "location_respawn", new Vector2w(125, 8), 0, LocationType.respawn);
            CreateLocation(map, createLocation, "location_work1", new Vector2w(121, 18), 1, LocationType.work1);
            CreateLocation(map, createLocation, "location_work1", new Vector2w(131, 18), 3, LocationType.work1);
            CreateLocation(map, createLocation, "location_rent1", new Vector2w(121, 24), 1, LocationType.rent1);
            CreateLocation(map, createLocation, "location_buying", new Vector2w(131, 24), 3, LocationType.buying);
            CreateLocation(map, createLocation, "location_shop", new Vector2w(131, 30), 3, LocationType.shop);


            //caves

            for (int i = 0; i < 50; i++)
            {
                if (CreateCave(rnd, map, createLocation)) return;
            }

            //water
            GenerateWater(rnd, map);


            //blocks
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);

                    if (map[p].Floor == null && Info.EntityInfo[map[p].Ground.Id].isMove)
                    {
                        string block = GenerateBlock(rnd);
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

        private static bool CreateCave(Rnd rnd, Map map, FuncCreateLocation createLocation)
        {
            var info = Info.EntityInfo["cave"];
            byte rotate = (byte)rnd.Range(0, 4);

            Vector2w pos;

            do
            {
                pos = map.GetFreeRndPos(rnd, info.size, rotate, 1);
            } while (GameConst.cityRect.Contains(pos));

            if (pos == Vector2w.Empty)
            {
                Util.Log("logs/error_create_loc.txt", "no find pos exit");
                return true;
            }

            Entity exitCave = Entity.Create(info.id);
            exitCave.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
            exitCave.Rotate.Value = rotate;
            map.SetEntity(pos, exitCave);

            var loc = createLocation(map.Id, LocationType.cave, 0, pos, 0);

            exitCave.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
            exitCave.ExitLocId.Value = loc.Item1;
            exitCave.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
            exitCave.ExitPos.Pos = loc.Item2;
            return false;
        }

        private void GenerateWater(Rnd rnd, Map map)
        {
            int s = map.Size.x * Size.y - GameConst.cityRect.w * GameConst.cityRect.h;

            int n = s / 512;

            ushort waterId = Info.EntityInfo["ground_water"].id;

            for (int i = 0; i < n; i++)
            {
                Vector2w pos = GetFreeWaterPos(rnd, map);
                map[pos].Ground = Entity.Create(waterId);
            }

            bool[,] m = new bool[Size.x, Size.y];

            for (int i = 0; i < 6; i++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    for (int y = 0; y < Size.y; y++)
                        m[x, y] = false;
                }


                for (int x = 0; x < Size.x; x++)
                {
                    for (int y = 0; y < Size.y; y++)
                    {
                        Vector2w p = new Vector2w(x, y);
                        if (map[p].Ground.Id == waterId)
                        {
                            var pos = p + Util.offset4[rnd.Range(0, 4)];
                            if (map.IsMap(pos) && !GameConst.cityRect.Contains(pos))
                            {
                                m[pos.x, pos.y] = true;
                            }
                        }
                    }
                }

                for (int x = 0; x < Size.x; x++)
                {
                    for (int y = 0; y < Size.y; y++)
                    {
                        if (m[x, y])
                        {
                            var pos = new Vector2w(x, y);
                            map[pos].Ground = Entity.Create(waterId);
                        }
                    }
                }
            }
        }


        private Vector2w GetFreeWaterPos(Rnd rnd, Map map)
        {
            Vector2w pos;
            do
            {
                pos = rnd.Range(Vector2w.Zero, Size);
            } while (GameConst.cityRect.Contains(pos) || !Info.EntityInfo[map[pos].Ground.Id].isMove);

            return pos;
        }

        private void CreateLocation(Map map, FuncCreateLocation createLocation, string name, Vector2w pos, byte rotate, LocationType locationType)
        {
            var entity = Entity.Create(name);
            var p = pos;
            entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
            entity.Rotate.Value = rotate;
            map.SetEntity(p, entity);
            var loc = createLocation(map.Id, locationType, 0, p, 0);

            entity.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
            entity.ExitLocId.Value = loc.Item1;
            entity.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
            entity.ExitPos.Pos = loc.Item2;
        }

        private string GenerateBlock(Rnd rnd)
        {
            string block = string.Empty;

            if (rnd.Range(0, 100) < 20)
            {
                if (rnd.Range(0, 100) < 20)
                    block = "stone_small";
                else if (rnd.Range(0, 100) < 20)
                    block = "wood_small";
                else if (rnd.Range(0, 100) < 20)
                    block = "bush";
                else if (rnd.Range(0, 100) < 20)
                    block = "bush2";
                else if (rnd.Range(0, 100) < 20)
                    block = "fir";
                else if (rnd.Range(0, 100) < 20)
                    block = "stone";
                else if (rnd.Range(0, 100) < 20)
                    block = "fir2";
                else if (rnd.Range(0, 100) < 20)
                    block = "stone2";
                else if (rnd.Range(0, 100) < 20)
                    block = "fir3";
            }

            return block;
        }
    }
}
