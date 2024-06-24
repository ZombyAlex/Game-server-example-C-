using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.IO;

namespace SWFServer.Data.Net
{
    public enum MsgClintType
    {
        login,
        id,
        chat,
        signal,
        pos,
        inputKey,
        getTask,
        craft,
        index,
        build,
        moveItems,
        task,
        buy
    }

    public enum MsgClintTypeSignal
    {
        createUnit,
        exitLocation,
        sleep,
        sit,
        shower,
        cancelTask,
        requestSkills
    }

    public enum MsgClintTypePos
    {
        leftClick,
        rightClick,
        takeBlock
    }

    public enum MsgClientTypeIndex
    {
        equip,
        unEquip,
        useItem
    }

    public enum MsgClientTypeId
    {
        requestUserName,
        removeTask,
        performTask
    }

    public class MsgClient
    {
        public uint UserId;
        public MsgClintType Type;
        public NetData Data;

        public MsgClient()
        {
        }

        public MsgClient(MsgClintType type, NetData data)
        {
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

            /*

            switch (Type)
            {
                case MsgClintType.login:
                    NetData.Write(msg, (MsgClientLogin)Data);
                    break;
                case MsgClintType.requestUserName:
                    NetData.Write(msg, (MsgClientRequestUserName)Data);
                    break;
                case MsgClintType.chat:
                    NetData.Write(msg, (MsgClientChat)Data);
                    break;
                case MsgClintType.signal:
                    NetData.Write(msg, (MsgClientSignal)Data);
                    break;
                case MsgClintType.pos:
                    NetData.Write(msg, (MsgClientPos)Data);
                    break;
                case MsgClintType.inputKey:
                    NetData.Write(msg, (MsgClientInputKey)Data);
                    break;
                case MsgClintType.getTask:
                    NetData.Write(msg, (MsgClientGetTask)Data);
                    break;
                case MsgClintType.craft:
                    NetData.Write(msg, (MsgClientCraft)Data);
                    break;
                case MsgClintType.index:
                    NetData.Write(msg, (MsgClientIndex)Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            */
        }

        
        public void Read(NetIncomingMessage msg)
        {
            Type = (MsgClintType)msg.ReadByte();

            Data = CreateData();
            int size = msg.ReadInt32();
            var array = msg.ReadBytes(size);
            MemoryStream stream = new MemoryStream(array);
            BinaryReader r = new BinaryReader(stream);
            Data.Read(r);
        }

        public NetData CreateData()
        {
            switch (Type)
            {
                case MsgClintType.login:
                    return new MsgClientLogin();
                case MsgClintType.id:
                    return new MsgClientId();
                case MsgClintType.chat:
                    return new MsgClientChat();
                case MsgClintType.signal:
                    return new MsgClientSignal();
                case MsgClintType.pos:
                    return new MsgClientPos();
                case MsgClintType.inputKey:
                    return new MsgClientInputKey();
                case MsgClintType.getTask:
                    return new MsgClientGetTask();
                case MsgClintType.craft:
                    return new MsgClientCraft();
                case MsgClintType.index:
                    return new MsgClientIndex();
                case MsgClintType.build:
                    return new MsgClientBuild();
                case MsgClintType.moveItems:
                    return new MsgClientMoveItems();
                case MsgClintType.task:
                    return new MsgClientTask();
                case MsgClintType.buy:
                    return new MsgClientBuy();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    
    public  class MsgClientLogin : NetData
    {
        public string login;
        public string pass;

        
        public MsgClientLogin()
        {
        }

        public MsgClientLogin(string login, string pass)
        {
            this.login = login;
            this.pass = pass;
        }

        public override void Write(BinaryWriter writer)
        {
            Util.WriteString(writer, login);
            Util.WriteString(writer, pass);
        }

        public override void Read(BinaryReader reader)
        {
            login = Util.ReadString(reader);
            pass = Util.ReadString(reader);
        }
    }
    
    public  class MsgClientId : NetData
    {
        public uint val;
        public MsgClientTypeId type;

        public MsgClientId()
        {
        }

        public MsgClientId(uint val, MsgClientTypeId type)
        {
            this.val = val;
            this.type = type;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(val);
            writer.Write((byte)type);
        }

        public override void Read(BinaryReader reader)
        {
            val = reader.ReadUInt32();
            type = (MsgClientTypeId)reader.ReadByte();
        }
    }

    
    public  class MsgClientChat : NetData
    {
        public bool isChannel;
        public uint channelId;
        public string text;

        
        public MsgClientChat()
        {
        }

        public MsgClientChat(bool isChannel, uint channelId, string text)
        {
            this.isChannel = isChannel;
            this.channelId = channelId;
            this.text = text;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(isChannel);
            writer.Write(channelId);
            Util.WriteString(writer, text);
        }

        public override void Read(BinaryReader reader)
        {
            isChannel = reader.ReadBoolean();
            channelId = reader.ReadUInt32();
            text = Util.ReadString(reader);
        }
    }

    
    public  class MsgClientSignal : NetData
    {
        public MsgClintTypeSignal signal;

        
        public MsgClientSignal()
        {
        }

        public MsgClientSignal(MsgClintTypeSignal signal)
        {
            this.signal = signal;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)signal);
        }

        public override void Read(BinaryReader reader)
        {
            signal = (MsgClintTypeSignal)reader.ReadByte();
        }
    }

    
    public  class MsgClientPos : NetData
    {
        public MsgClintTypePos type;
        public Vector2w pos;

        
        public MsgClientPos()
        {
        }

        public MsgClientPos(MsgClintTypePos type, Vector2w pos)
        {
            this.type = type;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)type);
            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            type = (MsgClintTypePos)reader.ReadByte();
            pos = Vector2w.Read(reader);
        }
    }

    
    public  class MsgClientInputKey : NetData
    {
        public Vector2f dir;

        
        public MsgClientInputKey()
        {

        }

        public MsgClientInputKey(Vector2f dir)
        {
            this.dir = dir;
        }

        public override void Write(BinaryWriter writer)
        {
            dir.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            dir = Vector2f.Read(reader);
        }
    }

    
    public  class MsgClientGetTask : NetData
    {
        public uint taskId;

        
        public MsgClientGetTask()
        {
        }

        public MsgClientGetTask(uint taskId)
        {
            this.taskId = taskId;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(taskId);
        }

        public override void Read(BinaryReader reader)
        {
            taskId = reader.ReadUInt32();
        }
    }

    
    public  class MsgClientCraft : NetData
    {
        public ushort itemId;
        public int count;
        public Vector2w pos;

        
        public MsgClientCraft()
        {
        }

        public MsgClientCraft(ushort itemId, int count, Vector2w pos)
        {
            this.itemId = itemId;
            this.count = count;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(itemId);
            writer.Write(count);
            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            itemId = reader.ReadUInt16();
            count = reader.ReadInt32();
            pos = Vector2w.Read(reader);
        }
    }

    
    public  class MsgClientIndex : NetData
    {
        public int index;
        public MsgClientTypeIndex type;

        
        public MsgClientIndex()
        {
        }

        public MsgClientIndex(MsgClientTypeIndex type, int index)
        {
            this.type = type;
            this.index = index;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(index);
            writer.Write((byte)type);
        }

        public override void Read(BinaryReader reader)
        {
            index = reader.ReadInt32();
            type = (MsgClientTypeIndex)reader.ReadByte();
        }
    }

    public class MsgClientBuild : NetData
    {
        public int inventoryPos;
        public Vector2w pos;
        public int rotate;


        public MsgClientBuild()
        {
        }

        public MsgClientBuild(int inventoryPos, Vector2w pos, int rotate)
        {
            this.inventoryPos = inventoryPos;
            this.rotate = rotate;
            this.pos = pos;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(inventoryPos);
            writer.Write(rotate);
            pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            inventoryPos = reader.ReadInt32();
            rotate = reader.ReadInt32();
            pos = Vector2w.Read(reader);
        }
    }

    public class MsgClientMoveItems : NetData
    {
        public Vector2w pos;
        public List<int> items;
        public bool isGet;


        public MsgClientMoveItems()
        {
        }


        public MsgClientMoveItems(Vector2w pos, List<int> items, bool isGet)
        {
            this.pos = pos;
            this.items = items;
            this.isGet = isGet;
        }
        public override void Write(BinaryWriter writer)
        {
            pos.Write(writer);
            writer.Write(items.Count);
            foreach (var item in items) 
                writer.Write(item);
            writer.Write(isGet);
        }

        public override void Read(BinaryReader reader)
        {
            pos = Vector2w.Read(reader);
            int cnt = reader.ReadInt32();
            items = new List<int>();
            for (int i = 0; i < cnt; i++) 
                items.Add(reader.ReadInt32());
            isGet = reader.ReadBoolean();
        }
    }

    public class MsgClientTask : NetData
    {
        public GameTask task;

        public MsgClientTask()
        {
        }

        public MsgClientTask(GameTask task)
        {
            this.task = task;
        }

        public override void Write(BinaryWriter writer)
        {
            task.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            task = GameTask.Read(reader);
        }
    }

    public class MsgClientBuy : NetData
    {
        public Vector2w pos;
        public int index;
        public ushort itemId;
        public int count;

        public MsgClientBuy()
        {
        }

        public MsgClientBuy(Vector2w pos, int index, ushort itemId, int count)
        {
            this.pos = pos;
            this.index = index;
            this.itemId = itemId;
            this.count = count;
        }

        public override void Write(BinaryWriter writer)
        {
            pos.Write(writer);
            writer.Write(index);
            writer.Write(itemId);
            writer.Write(count);
        }

        public override void Read(BinaryReader reader)
        {
            pos = Vector2w.Read(reader);
            index = reader.ReadInt32();
            itemId = reader.ReadUInt16();
            count = reader.ReadInt32();
        }
    }

}