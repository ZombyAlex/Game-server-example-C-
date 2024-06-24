using System;
using SWFServer.Data;
using SWFServer.Game.MapGenerators;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMap: GameModule
    {
        public Data.Map Map { get; } = null;

        public ModuleMap(uint id, LocationType locationType, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            IMapGenerator g;

            switch (locationType)
            {
                case LocationType.world:
                    g = new MapWorldGenerator() { Size = new Vector2w(256, 256) };
                    break;
                case LocationType.respawn:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/location_respawn.tmj") };
                    break;
                case LocationType.work1:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/location_work1.tmj") };
                    break;
                case LocationType.rent1:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/location_rent1.tmj") };
                    break;
                case LocationType.house_wood:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/house_wood.tmj") };
                    break;
                case LocationType.factory1:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/factory1.tmj") };
                    break;
                case LocationType.cave:
                    g = new MapCaveGenerator() { Size = Rnd.Range(new Vector2w(6, 6), new Vector2w(25, 25)) };
                    break;
                case LocationType.buying:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/location_buying.tmj") };
                    break;
                case LocationType.shop:
                    g = new MapTilemapGenerator() { TileMap = new TileMap("maps/location_shop.tmj") };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null);
            }

            Map = new Map(id, Rnd, g, createLocation, exit, level);
        }

        public override void Update(float dt)
        {
        }

        public override void Terminate()
        {
            Map.Save();
        }

        public Vector2w GetFreeRespawn()
        {
            return Rnd.Range(Vector2w.One, new Vector2w(15, 15));
        }

        public Vector2w GetExit()
        {
            return Map.GetExit();
        }
    }
}
