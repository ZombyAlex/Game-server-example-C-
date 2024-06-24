using System;

namespace SWFServer.Data
{
    public interface ITileMap
    {
        Vector2w Size { get;}
        ushort[,] Ground { get; }
        ushort[,] Floor { get; }
        ushort[,] Block { get;}
    }

    public interface IMapGenerator
    {
        ITileMap TileMap { get; set; }
        Vector2w Size { get; }
        void Generation(Rnd rnd, Map map, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level);
    }
}
