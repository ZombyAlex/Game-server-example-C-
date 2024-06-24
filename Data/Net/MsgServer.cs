using Lidgren.Network;
using Newtonsoft.Json;
using SWFServer.Data.Entities;
using System.Collections.Generic;
using System;
using System.IO;

namespace SWFServer.Data.Net
{
    public enum MsgServerType
    {
        connect,
        user,
        map,
        time,
        info,
        userName,
        userList,
        chat,
        rating,
        signal,
        mapCell,
        unitAvatar,
        mapCellLayer,
        unit,
        hideUnit,
        unitAttr,
        money,
        inventory,
        unitAction,
        tasks,
        entity
    }

    public enum MsgServerTypeSignal
    {
        exitBattle
    }


    public abstract class NetData
    {
        public abstract void Write(BinaryWriter writer);
        public abstract void Read(BinaryReader reader);

        /*
        public static void Write<T>(NetOutgoingMessage writer, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            Util.WriteString(writer, json);
        }

        public static T Read<T>(NetIncomingMessage reader)
        {
            var json = Util.ReadString(reader);
            var cmp = JsonConvert.DeserializeObject<T>(json);
            return cmp;
        }

        public static void WriteZip<T>(NetOutgoingMessage writer, T data)
        {
            //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
            var json = JsonConvert.SerializeObject(data);

            MemoryStream d;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(stream))
                {
                    Util.WriteString(w, json);

                    Util.CompressData(stream, out d);
                }
            }

            var bytes = d.ToArray();

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public static T ReadZip<T>(NetIncomingMessage reader)
        {
            int size = reader.ReadInt32();
            var array = reader.ReadBytes(size);

            MemoryStream stream = new MemoryStream(array);
            MemoryStream outStream;
            Util.DecompressData(stream, out outStream);
            BinaryReader r = new BinaryReader(outStream);

            var bites = Util.ReadString(r);
            //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
            var cmp = JsonConvert.DeserializeObject<T>(bites);
            return cmp;
        }
        */
    }



    public class MsgServer
    {
        [JsonIgnore] public uint UserId;
        [JsonIgnore] public MsgServerType Type;
        [JsonIgnore] public NetData Data;

        public MsgServer()
        {
        }

        public MsgServer(uint userId, MsgServerType type, NetData data)
        {
            UserId = userId;
            Type = type;
            Data = data;
        }

        public void Write(NetOutgoingMessage msg)
        {
            msg.Write((byte)Type);
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(stream))
                {
                    Data.Write(w);
                    var b = stream.ToArray();
                    msg.Write(b.Length);
                    msg.Write(b);
                }
            }

            if (Type == MsgServerType.map)
                Console.WriteLine("map size = " + msg.LengthBytes);
        }

        public void Read(NetIncomingMessage msg)
        {
            Type = (MsgServerType)msg.ReadByte();

            Data = CreateData();
            int size = msg.ReadInt32();
            var array = msg.ReadBytes(size);
            MemoryStream stream = new MemoryStream(array);
            BinaryReader r = new BinaryReader(stream);
            Data.Read(r);
        }

        private NetData CreateData()
        {
            switch (Type)
            {
                case MsgServerType.connect:
                    return new MsgServerConnect();
                case MsgServerType.user:
                    return new MsgServerUser();
                case MsgServerType.map:
                    return new MsgServerMap();
                case MsgServerType.time:
                    return new MsgServerTime();
                case MsgServerType.info:
                    return new MsgServerInfo();
                case MsgServerType.userName:
                    return new MsgServerUserName();
                case MsgServerType.userList:
                    return new MsgServerUserList();
                case MsgServerType.chat:
                    return new MsgServerChat();
                case MsgServerType.rating:
                    return new MsgServerRating();
                case MsgServerType.signal:
                    return new MsgServerSignal();
                case MsgServerType.mapCell:
                    return new MsgServerMapCell();
                case MsgServerType.unitAvatar:
                    return new MsgServerUnitAvatar();
                case MsgServerType.mapCellLayer:
                    return new MsgServerMapCellLayer();
                case MsgServerType.unit:
                    return new MsgServerUnit();
                case MsgServerType.hideUnit:
                    return new MsgServerHideUnit();
                case MsgServerType.unitAttr:
                    return new MsgServerUnitAttr();
                case MsgServerType.money:
                    return new MsgServerMoney();
                case MsgServerType.inventory:
                    return new MsgServerInventory();
                case MsgServerType.unitAction:
                    return new MsgServerUnitAction();
                case MsgServerType.tasks:
                    return new MsgServerTasks();
                case MsgServerType.entity:
                    return new MsgServerEntity();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class MsgServerConnect : NetData
    {
        public int port;


        public MsgServerConnect()
        {
        }

        public MsgServerConnect(int port)
        {
            this.port = port;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(port);
        }

        public override void Read(BinaryReader reader)
        {
            port = reader.ReadInt32();
        }
    }


    public class MsgServerUser : NetData
    {
        public uint userId;
        public uint mapId;
        public LocationType locationType;
        public uint locationOwner;


        public MsgServerUser()
        {
        }

        public MsgServerUser(uint userId, uint mapId, LocationType locationType, uint locationOwner)
        {
            this.userId = userId;
            this.mapId = mapId;
            this.locationType = locationType;
            this.locationOwner = locationOwner;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(userId);
            writer.Write(mapId);
            writer.Write((byte)locationType);
            writer.Write(locationOwner);
        }

        public override void Read(BinaryReader reader)
        {
            userId = reader.ReadUInt32();
            mapId = reader.ReadUInt32();
            locationType = (LocationType)reader.ReadByte();
            locationOwner = reader.ReadUInt32();
        }
    }


    public class MapSector
    {
        public Vector2w GridPos;
        public MapCell[,] Cells;
        public Vector2w Size;

        public void Write(BinaryWriter writer)
        {
            GridPos.Write(writer);
            Size.Write(writer);

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Cells[x, y].Write(writer);
                }
            }
        }

        public void Read(BinaryReader reader)
        {
            GridPos = Vector2w.Read(reader);
            Size = Vector2w.Read(reader);
            Cells = new MapCell[Size.x, Size.y];

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Cells[x, y] = MapCell.Read(reader);
                }
            }
        }
    }


    public class MsgServerMap : NetData
    {
        public List<MapSector> sectors = new List<MapSector>();
        public Vector2w sectorPos;


        public MsgServerMap()
        {
        }

        public MsgServerMap(Map map, List<Vector2w> grid, Vector2w sectorPos)
        {
            this.sectorPos = sectorPos;

            for (int i = 0; i < grid.Count; i++)
            {
                Vector2w size = new Vector2w(GameConst.mapGrid, GameConst.mapGrid);
                var mapPos = Util.GridToMapPos(grid[i]);
                if (mapPos.x + size.x > map.Size.x)
                    size.x = (short)(map.Size.x % GameConst.mapGrid);
                if (mapPos.y + size.y > map.Size.y)
                    size.y = (short)(map.Size.y % GameConst.mapGrid);

                MapSector s = new MapSector() { GridPos = grid[i], Cells = new MapCell[size.x, size.y], Size = size };

                Vector2w pos = Util.GridToMapPos(grid[i]);
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        var p = pos + new Vector2w(x, y);
                        if (map.IsMap(p))
                            s.Cells[x, y] = map[p];
                    }
                }

                sectors.Add(s);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            MemoryStream d;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(stream))
                {
                    sectorPos.Write(w);
                    w.Write(sectors.Count);
                    foreach (var sector in sectors)
                    {
                        sector.Write(w);
                    }

                    Util.CompressData(stream, out d);
                }
            }

            var bytes = d.ToArray();

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public override void Read(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            var array = reader.ReadBytes(size);

            MemoryStream stream = new MemoryStream(array);
            MemoryStream outStream;
            Util.DecompressData(stream, out outStream);
            BinaryReader r = new BinaryReader(outStream);

            sectorPos = Vector2w.Read(r);
            int cnt = r.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                MapSector s = new MapSector();
                s.Read(r);
                sectors.Add(s);
            }
        }
    }


    public class MsgServerTime : NetData
    {
        public double time;

        public MsgServerTime()
        {
        }

        public MsgServerTime(double time)
        {
            this.time = time;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(time);
        }

        public override void Read(BinaryReader reader)
        {
            time = reader.ReadDouble();
        }
    }


    public class MsgServerInfo : NetData
    {
        public string info;


        public MsgServerInfo()
        {
        }

        public MsgServerInfo(string info)
        {
            this.info = info;
        }

        public override void Write(BinaryWriter writer)
        {
            Util.WriteString(writer, info);
        }

        public override void Read(BinaryReader reader)
        {
            info = Util.ReadString(reader);
        }
    }


    public class MsgServerUserName : NetData
    {
        public uint reqUserId;
        public string userName;


        public MsgServerUserName()
        {
        }

        public MsgServerUserName(uint reqUserId, string userName)
        {
            this.reqUserId = reqUserId;
            this.userName = userName;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(reqUserId);
            Util.WriteString(writer, userName);
        }

        public override void Read(BinaryReader reader)
        {
            reqUserId = reader.ReadUInt32();
            userName = Util.ReadString(reader);
        }
    }


    public class MsgServerUserList : NetData
    {
        public List<uint> users = new List<uint>();


        public MsgServerUserList()
        {
        }

        public MsgServerUserList(List<uint> users)
        {
            this.users = users;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(users.Count);
            foreach (var u in users) writer.Write(u);
        }

        public override void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++) users.Add(reader.ReadUInt32());
        }
    }



    public class MsgServerChat : NetData
    {
        public bool isChannel;
        public uint channelId;
        public string name;
        public string text;


        public MsgServerChat()
        {
        }

        public MsgServerChat(bool isChannel, uint channelId, string name, string text)
        {
            this.isChannel = isChannel;
            this.channelId = channelId;
            this.name = name;
            this.text = text;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(isChannel);
            writer.Write(channelId);
            Util.WriteString(writer, name);
            Util.WriteString(writer, text);
        }

        public override void Read(BinaryReader reader)
        {
            isChannel = reader.ReadBoolean();
            channelId = reader.ReadUInt32();
            name = Util.ReadString(reader);
            text = Util.ReadString(reader);
        }
    }


    public class MsgServerRating : NetData
    {
        public List<UserRating> ratings = new List<UserRating>();


        public MsgServerRating()
        {
        }

        public MsgServerRating(List<UserRating> ratings)
        {
            this.ratings = ratings;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(ratings.Count);
            foreach (var rating in ratings)
            {
                rating.Write(writer);
            }
        }

        public override void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                ratings.Add(UserRating.Read(reader));
            }
        }
    }


    public class MsgServerSignal : NetData
    {
        public MsgServerTypeSignal signal;


        public MsgServerSignal()
        {
        }

        public MsgServerSignal(MsgServerTypeSignal signal)
        {
            this.signal = signal;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)signal);
        }

        public override void Read(BinaryReader reader)
        {
            signal = (MsgServerTypeSignal)reader.ReadByte();
        }
    }


    public class MsgServerMapCell : NetData
    {
        public MapCell cell;
        public Vector2w pos;


        public MsgServerMapCell()
        {
        }

        public MsgServerMapCell(MapCell cell, Vector2w pos)
        {
            this.cell = cell;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            cell.Write(writer);
            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            cell = MapCell.Read(reader);
            pos = Vector2w.Read(reader);
        }
    }


    public class MsgServerUnitAvatar : NetData
    {
        public UnitAvatar avatar;


        public MsgServerUnitAvatar()
        {
        }

        public MsgServerUnitAvatar(UnitAvatar avatar)
        {
            this.avatar = avatar;
        }

        public override void Write(BinaryWriter writer)
        {
            avatar.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            avatar = UnitAvatar.Read(reader);
        }
    }


    public class MsgServerMapCellLayer : NetData
    {
        public Entity cellLayer;
        public EntityMapLayer layer;
        public Vector2w pos;


        public MsgServerMapCellLayer()
        {
        }

        public MsgServerMapCellLayer(Entity cellLayer, Vector2w pos, EntityMapLayer layer)
        {
            this.cellLayer = cellLayer;
            this.pos = pos;
            this.layer = layer;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(cellLayer != null);
            cellLayer?.Write(writer);
            writer.Write((byte)layer);
            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            if (reader.ReadBoolean())
                cellLayer = Entity.Read(reader);
            layer = (EntityMapLayer)reader.ReadByte();
            pos = Vector2w.Read(reader);
        }
    }


    public class MsgServerUnit : NetData
    {
        public Entity unit;


        public MsgServerUnit()
        {
        }

        public MsgServerUnit(Entity unit)
        {
            this.unit = unit;
        }

        public override void Write(BinaryWriter writer)
        {
            unit.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            unit = Entity.Read(reader);
        }
    }


    public class MsgServerHideUnit : NetData
    {
        public uint unitId;


        public MsgServerHideUnit()
        {
        }

        public MsgServerHideUnit(uint unitId)
        {
            this.unitId = unitId;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(unitId);
        }

        public override void Read(BinaryReader reader)
        {
            unitId = reader.ReadUInt32();
        }
    }


    public class MsgServerUnitAttr : NetData
    {
        public UnitAttrType attrType;
        public float val;


        public MsgServerUnitAttr()
        {
        }

        public MsgServerUnitAttr(UnitAttrType attrType, float val)
        {
            this.attrType = attrType;
            this.val = val;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)attrType);
            writer.Write(val);
        }

        public override void Read(BinaryReader reader)
        {
            attrType = (UnitAttrType)reader.ReadByte();
            val = reader.ReadSingle();
        }
    }


    public class MsgServerMoney : NetData
    {
        public int money;


        public MsgServerMoney()
        {
        }

        public MsgServerMoney(int money)
        {
            this.money = money;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(money);
        }

        public override void Read(BinaryReader reader)
        {
            money = reader.ReadInt32();
        }
    }


    public class MsgServerInventory : NetData
    {
        public List<Entity> items;
        public Vector2w pos;

        public MsgServerInventory()
        {
        }

        public MsgServerInventory(List<Entity> items, Vector2w pos)
        {
            this.items = items;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(items.Count);
            foreach (var entity in items)
            {
                entity.Write(writer);
            }

            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            items = new List<Entity>();
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                items.Add(Entity.Read(reader));
            }

            pos = Vector2w.Read(reader);
        }
    }


    public class MsgServerUnitAction : NetData
    {
        public float time;


        public MsgServerUnitAction()
        {
        }

        public MsgServerUnitAction(float time)
        {
            this.time = time;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(time);
        }

        public override void Read(BinaryReader reader)
        {
            time = reader.ReadSingle();
        }
    }


    public class MsgServerTasks : NetData
    {
        public List<GameTask> tasks = new List<GameTask>();


        public MsgServerTasks()
        {
        }

        public MsgServerTasks(List<GameTask> tasks)
        {
            this.tasks = tasks;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(tasks.Count);
            foreach (var task in tasks)
            {
                task.Write(writer);
            }
        }

        public override void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                tasks.Add(GameTask.Read(reader));
            }
        }
    }


    public class MsgServerEntity : NetData
    {
        public Entity entity;
        public int pos;


        public MsgServerEntity()
        {
        }

        public MsgServerEntity(Entity entity, int pos)
        {
            this.entity = entity;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            entity.Write(writer);
            writer.Write(pos);
        }

        public override void Read(BinaryReader reader)
        {
            entity = Entity.Read(reader);
            pos = reader.ReadInt32();
        }
    }
}
