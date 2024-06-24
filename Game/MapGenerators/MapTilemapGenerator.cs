using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Game.MapGenerators
{
    class MapTilemapGenerator : IMapGenerator
    {
        public ITileMap TileMap { get; set; }
        public Vector2w Size
        {
            get => TileMap.Size;
            set => Size = value;
        }

        public void Generation(Rnd rnd, Map map, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            //Size = TileMap.Size;

            map.InitSize(Size);

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);
                    MapCell c = new MapCell();
                    map.SetCell(p, c);
                    int r = rnd.Range(0, 90);
                    c.Ground = Entity.Create("ground1");
                    if (TileMap.Floor[x, y] != 0)
                        c.Floor = Entity.Create(TileMap.Floor[x, y]);

                    if (TileMap.Block[x, y] != 0)
                    {
                        var entity = Entity.Create(TileMap.Block[x, y]);
                        entity.AddComponent(new Component(ComponentClass.valByte, ComponentType.rotate));
                        entity.Rotate.Value = 0;
                        map.SetEntity(p, entity);
                        if (Info.EntityInfo[entity.Id].isExit)
                        {
                            entity.AddComponent(new Component(ComponentClass.valUint, ComponentType.exitLocId));
                            entity.ExitLocId.Value = exit.Item1;
                            entity.AddComponent(new Component(ComponentClass.position, ComponentType.exitPos));
                            entity.ExitPos.Pos = exit.Item2;
                            map.Exit = p;
                        }
                    }
                }
            }
        }
    }
}
