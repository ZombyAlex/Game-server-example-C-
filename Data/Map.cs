using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Lidgren.Network;
using Newtonsoft.Json;
using SWFServer.Data.Entities;

namespace SWFServer.Data
{
    public enum LocationType
    {
        world,
        respawn,
        work1,
        rent1,
        house_wood,
        factory1,
        cave,
        buying,
        shop
    }

    public class MapCell
    {
        public Entity Ground;
        public Entity Floor;
        public Entity Block;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Ground!= null);
            Ground?.Write(writer);

            writer.Write(Floor != null);
            Floor?.Write(writer);

            writer.Write(Block != null);
            Block?.Write(writer);
        }

        public static MapCell Read(BinaryReader reader)
        {
            MapCell c = new MapCell();

            if(reader.ReadBoolean()) c.Ground = Entity.Read(reader);
            if(reader.ReadBoolean()) c.Floor = Entity.Read(reader);
            if(reader.ReadBoolean()) c.Block = Entity.Read(reader);

            return c;
        }
    }

    public class MapSwitchBlock
    {
        public double time;
        public Vector2w pos;

        public void Read(BinaryReader reader)
        {
            time = reader.ReadDouble();
            pos = Vector2w.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(time);
            pos.Write(writer);
        }
    }

    public delegate (uint, Vector2w) FuncCreateLocation(uint locId, LocationType locationType, uint ownerId, Vector2w exitPos, int level);

    public class Map
    {
        public uint Id;

        //private MapCell[,] map;
        private Dictionary<Vector2w, MapCell> cells = new Dictionary<Vector2w, MapCell>();

        private string path => "data/maps/map" + Id + ".dat";

        private Vector2w size;

        //public MapCell[,] Cells => this[Vector2w.Zero];
        public Vector2w Size => size;

        public MapCell this[Vector2w p] => cells.ContainsKey(p) ? cells[p] : null;


        //public List<MapSwitchBlock> SwitchBlocks = new List<MapSwitchBlock>();

        private ushort[,] free;

        public Vector2w Exit { get; set; }

        public Map()
        {
            InitSize(new Vector2w(256, 256));
            InitFree();
        }

        public Map(uint id, Rnd rnd, IMapGenerator generator, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            Id = id;
            size = generator.Size;
            InitFree();

            if (!Load())
            {
                Console.WriteLine("Generate map " + id + " lvl=" + level);
                generator.Generation(rnd, this, createLocation, exit, level);
                Save();
            }
        }

        public void InitSize(Vector2w size)
        {
            this.size = new Vector2w(size.x, size.y);
            //map = new MapCell[size.x, size.y];
        }

        private void InitFree()
        {
            free = new ushort[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    free[x, y] = 0;
                }
            }
        }

        public bool IsFree(Vector2w p, Vector2w size, byte rotate)
        {
            if (rotate == 1 || rotate == 3)
                size.Swap();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var pos = p + new Vector2w(x, y);
                    if (!IsMap(pos))
                        return false;
                    if (!Info.EntityInfo[this[pos].Ground.Id].isMove)
                        return false;
                    if (free[pos.x, pos.y] != 0)
                        return false;
                }
            }
            return true;
        }

        public void SetFree(Vector2w p, Vector2w size, ushort val, byte rotate)
        {
            if(rotate == 1 || rotate == 3)
                size.Swap();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var pos = p + new Vector2w(x, y);
                    free[pos.x, pos.y] = val;
                }
            }
        }

        private bool IsMap(Vector2w p, Vector2w size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var pos = p + new Vector2w(x, y);
                    if (!IsMap(p))
                        return false;
                }
            }

            return true;
        }

        private bool Load()
        {
            if (!File.Exists(path))
                return false;

            using (var stream = File.Open(path, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {

                    size = Vector2w.Read(reader);
                    InitSize(size);
                    InitFree();

                    int cnt = reader.ReadInt32();
                    for (int i = 0; i < cnt; i++)
                    {
                        Vector2w key = Vector2w.Read(reader);
                        MapCell cell = MapCell.Read(reader);
                        cells.Add(key, cell);

                        UpdateFree(key);
                    }


                    //string json = Util.ReadString(reader);
                    //size = JsonConvert.DeserializeObject<Vector2w>(json);
                    //string jsonMap = Util.ReadString(reader);
                    //cells = JsonConvert.DeserializeObject<Dictionary<Vector2w, MapCell>>(jsonMap);
                    //Read(reader);
                }
            }

            return true;
        }


        /*
        public void Read(BinaryReader reader)
        {
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            size = new Vector2w(x, y);

            map = new MapCell[x, y];



            for (int ix = 0; ix < x; ix++)
            {
                for (int iy = 0; iy < y; iy++)
                {
                    var c = Util.JsonRead<MapCell>(reader);

                    map[ix, iy] = c;
                    if (c.Block != null)
                    {
                        var info = Info.EntityInfo[c.Block.Id];
                        SetFree(new Vector2w(ix, iy), info.size, c.Block.Id);
                    }
                }
            }

            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                MapSwitchBlock b = new MapSwitchBlock();
                b.Read(reader);
                SwitchBlocks.Add(b);
            }

        }
        */


        public void Save()
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    //writer.Write(size.x);
                    // writer.Write(size.y);
                    size.Write(writer);

                    writer.Write(cells.Count);
                    foreach (var c in cells)
                    {
                        c.Key.Write(writer);
                        c.Value.Write(writer);
                    }


                    //Util.WriteString(writer, JsonConvert.SerializeObject(size));
                    //Util.WriteString(writer, JsonConvert.SerializeObject(cells));
                    /*
                    for (int ix = 0; ix < size.x; ix++)
                    {
                        for (int iy = 0; iy < size.y; iy++)
                        {
                            Util.JsonWrite(writer, map[ix, iy]);
                        }
                    }
                    */
                    /*
                    writer.Write(SwitchBlocks.Count);
                    foreach (var b in SwitchBlocks)
                    {
                        b.Write(writer);
                    }
                    */
                }
            }
        }

        public bool IsMap(Vector2w p)
        {
            return p.x >= 0 && p.y >= 0 && p.x < size.x && p.y < size.y;
        }

        public void SetEntity(Vector2w pos, Entity entity)
        {
            var info = Info.EntityInfo[entity.Id];
            var cell = this[pos];

            switch (info.layer)
            {
                case EntityMapLayer.ground:
                    cell.Ground = entity;
                    break;
                case EntityMapLayer.floor:
                    cell.Floor = entity;
                    break;
                case EntityMapLayer.block:
                    cell.Block = entity;
                    SetFree(pos, info.size, entity.Id, entity.Rotate?.Value ?? (byte)0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public WRect GetRect()
        {
            return new WRect((short)0, (short)0, size.x, size.y);
        }

        public float GetSpeedMove(Vector2w pos)
        {
            var c = this[pos];
            if (c.Block != null)
                return Info.EntityInfo[c.Block.Id].speedMove;
            if (c.Floor != null)
                return Info.EntityInfo[c.Floor.Id].speedMove;

            return Info.EntityInfo[c.Ground.Id].speedMove;
        }

        public void RemoveEntity(Vector2w pos, EntityMapLayer layer)
        {
            var cell = this[pos];

            switch (layer)
            {
                case EntityMapLayer.ground:
                    cell.Ground = null;
                    break;
                case EntityMapLayer.floor:
                    cell.Floor = null;
                    break;
                case EntityMapLayer.block:
                    var block = GetBlock(pos);
                    SetFree(block.Item2, Info.EntityInfo[block.Item1.Id].size, 0, block.Item1.Rotate?.Value ?? (byte)0);
                    this[block.Item2].Block = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsMove(Vector2w pos)
        {
            var f = free[pos.x, pos.y];
            if (f == 0)
                return true;
            return Info.EntityInfo[f].isMove;
        }

        public (Entity, Vector2w) GetBlock(Vector2w pos)
        {
            var f = free[pos.x, pos.y];
            if (f == 0)
                return (null, Vector2w.Zero);

            if (this[pos] != null && this[pos].Block != null)
                return (this[pos].Block, pos);

            Vector2w s = Info.EntityInfo[f].size;

            for (int x = 0; x < s.x; x++)
            {
                for (int y = 0; y < s.y; y++)
                {
                    Vector2w p = new Vector2w(x - s.x + 1, y - s.y + 1) + pos;
                    if (IsMap(p) && this[p] != null && this[p].Block != null && this[p].Block.Id == f)
                        return (this[p].Block, p);
                }
            }

            return (null, Vector2w.Zero);
        }

        public Vector2w GetExit()
        {
            return Exit;
        }

        public List<MapCell> GetCells(Vector2w pos, int radius)
        {
            List<MapCell> list = new List<MapCell>();
            int r = radius * 2 + 1;

            for (int x = 0; x < r; x++)
            {
                for (int y = 0; y < r; y++)
                {
                    Vector2w p = new Vector2w(x - radius, y - radius) + pos;
                    if(IsMap(p))
                        list.Add(this[p]);
                }
            }

            return list;
        }

        public List<Entity> GetBlocks(Vector2w pos, int radius)
        {
            List<Entity> list = new List<Entity>();
            int r = radius * 2 + 1;

            for (int x = 0; x < r; x++)
            {
                for (int y = 0; y < r; y++)
                {
                    Vector2w p = new Vector2w(x - radius, y - radius) + pos;
                    if (IsMap(p))
                    {
                        var b = GetBlock(p);
                        if (b.Item1 != null)
                            list.Add(b.Item1);
                    }
                }
            }

            return list;
        }

        public Vector2w GetBlockPos(Vector2w pos, int radius, Entity block)
        {
            int r = radius * 2 + 1;

            for (int x = 0; x < r; x++)
            {
                for (int y = 0; y < r; y++)
                {
                    Vector2w p = new Vector2w(x - radius, y - radius) + pos;
                    if (IsMap(p))
                    {
                        var b = GetBlock(p);
                        if (b.Item1 == block)
                            return b.Item2;
                    }
                }
            }

            return Vector2w.Zero;
        }

        public Vector2w FindExit(int locId)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);
                    var cell = this[p];
                    if (cell.Block != null && Info.EntityInfo[cell.Block.Id].isExit && cell.Block.ExitLocId.Value == locId)
                        return cell.Block.ExitPos.Pos;
                }
            }

            return Vector2w.Zero;
        }

        public (Entity, Vector2w) FindContainer(ushort itemId, int count)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2w p = new Vector2w(x, y);
                    var cell = this[p];
                    if (cell.Block != null &&  Info.EntityInfo[cell.Block.Id].containerSize > 0 
                                           && UtilInventory.IsAddItem(itemId, Info.EntityInfo[cell.Block.Id].containerSize,  count,cell.Block.Entities.Entities))
                        return (cell.Block, p);
                }
            }

            return (null, Vector2w.Zero);
        }

        public void SetCell(Vector2w pos, MapCell cell)
        {
            if (!cells.ContainsKey(pos))
            {
                cells.Add(pos, cell);
            }
            else
                cells[pos] = cell;
        }

        public void UpdateFree(Vector2w pos)
        {
            if (cells.ContainsKey(pos))
            {
                var c = cells[pos];
                if (c != null && c.Block != null)
                {
                    var info = Info.EntityInfo[c.Block.Id];
                    SetFree(pos, info.size, c.Block.Id, c.Block.Rotate?.Value ?? (byte)0);
                }
            }
        }

        public Vector2w GetFreeRndPos(Rnd rnd, Vector2w size, byte rotate, int offset)
        {
            Vector2w p;
            int error = 0;

            do
            {
                p = rnd.Range(new Vector2w(offset, offset), Size - new Vector2w(offset * 2, offset * 2));
                error++;
                if (error > 100)
                    return Vector2w.Empty;
            } while (!IsFree(p, size, rotate));

            return  p;
        }
    }
}
