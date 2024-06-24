using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Game
{
    public class TileMapLayer
    {
        public List<int> data;
        public int height;
        public int id;
        public string name;
        public int opacity;
        public string type;
        public bool visible;
        public int width;
        public int x;
        public int y;
    }

    public class TileMapRoot
    {
        public int compressionlevel;
        public int height;
        public bool infinite;
        public List<TileMapLayer> layers;
        public int nextlayerid;
        public int nextobjectid;
        public string orientation;
        public string renderorder;
        public string tiledversion;
        public int tileheight;
        public List<TileMapTileset> tilesets;
        public int tilewidth;
        public string type;
        public string version;
        public int width;
    }

    public class TileMapTileset
    {
        public int firstgid;
        public string source;
    }

    public class TileMap : ITileMap
    {
        private Dictionary<int, string> adapter = new Dictionary<int, string>();

        public Vector2w Size { get; private set; }
        public ushort[,] Ground { get; }
        public ushort[,] Floor { get; }
        public ushort[,] Block { get; }


        public TileMap(string path)
        {
            adapter.Add(1, "floor_wood");
            adapter.Add(2, "floor_stone");
            adapter.Add(3, "wall_stone");
            adapter.Add(4, "exit_stone");
            adapter.Add(5, "wall_wood");
            adapter.Add(6, "exit_wood");
            adapter.Add(7, "workbench");
            adapter.Add(8, "container_wood");

            adapter.Add(10, "table_wood");
            adapter.Add(11, "sawmill_wood");
            adapter.Add(12, "masons_workshop");
            adapter.Add(13, "furnance_stone");



            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();

            TileMapRoot map = JsonConvert.DeserializeObject<TileMapRoot>(json);

            Size = new Vector2w(map.width, map.height);
            Floor = new ushort[Size.x, Size.y];
            Block = new ushort[Size.x, Size.y];

            FillMap(map, "floor", Floor);
            FillMap(map, "block", Block);
        }

        private void FillMap(TileMapRoot map, string layerName, ushort[,] data)
        {
            TileMapLayer layer = map.layers.Find(f => f.name == layerName);

            for (int i = 0; i < layer.data.Count; i++)
            {
                int x = i % Size.x;
                int y = Size.y - i / Size.x - 1;

                if (layer.data[i] != 0)
                    data[x, y] = Info.EntityInfo[adapter[layer.data[i]]].id;
            }
        }
    }
}
