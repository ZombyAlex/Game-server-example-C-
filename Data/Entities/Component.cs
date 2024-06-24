using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SWFServer.Data.Entities
{
    public enum ComponentClass
    {
        unit,
        valInt,
        entities,
        valUint,
        valByte,
        position,
        valFloat
    }

    public enum ComponentType
    {
        unit,
        count,
        entities,
        id,
        rotate,
        exitLocId,
        exitPos,
        durability,
        cost
    }

    public class Component
    {
        public ComponentClass Class;
        public ComponentType Type;
        public ComponentData Data;

        public Component()
        {
        }

        public Component(ComponentClass @class, ComponentType type)
        {
            Class = @class;
            Type = type;
            Data = CreateData();
        }

        public ComponentData CreateData()
        {
            ComponentData d;
            switch (Class)
            {
                case ComponentClass.unit:
                    d = new ComponentUnit();
                    break;
                case ComponentClass.valInt:
                    d = new ComponentInt();
                    break;
                case ComponentClass.entities:
                    d = new ComponentEntities();
                    break;
                case ComponentClass.valUint:
                    d = new ComponentUint();
                    break;
                case ComponentClass.valByte:
                    d = new ComponentByte();
                    break;
                case ComponentClass.position:
                    d = new ComponentPosition();
                    break;
                case ComponentClass.valFloat:
                    d = new ComponentFloat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Class), Class, null);
            }

            return d;
        }


        /*
        public static ComponentData CreateData(ComponentClass @class, BinaryReader reader)
        {
            switch (@class)
            {
                case ComponentClass.unit:
                    return ComponentData.Read<ComponentUnit>(reader);
                case ComponentClass.valInt:
                    return ComponentData.Read<ComponentInt>(reader);
                case ComponentClass.entities:
                    return ComponentData.Read<ComponentEntities>(reader);
                case ComponentClass.valUint:
                    return ComponentData.Read<ComponentUint>(reader);
                case ComponentClass.valByte:
                    return ComponentData.Read<ComponentByte>(reader);
                case ComponentClass.position:
                    return ComponentData.Read<ComponentPosition>(reader);
                case ComponentClass.valFloat:
                    return ComponentData.Read<ComponentFloat>(reader);
                default:
                    throw new ArgumentOutOfRangeException(nameof(@class), @class, null);
            }

            return null;
        }
        */

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)Class);
            writer.Write((byte)Type);
            Data.Write(writer);
        }

        public static Component Read(BinaryReader reader)
        {
            Component c = new Component();
            c.Class = (ComponentClass)reader.ReadByte();
            c.Type = (ComponentType)reader.ReadByte();
            c.Data = c.CreateData();
            c.Data.Read(reader);
            return c;
        }
    }

    public abstract class ComponentData
    {
        public abstract void Write(BinaryWriter writer);
        public abstract void Read(BinaryReader reader);
    }

    public class ComponentInt : ComponentData
    {
        public int Value;
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            Value = reader.ReadInt32();
        }
    }


    public class ComponentEntities : ComponentData
    {
        public List<Entity> Entities = new List<Entity>();
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Entities.Count);
            Entities.ForEach(f => f.Write(writer));
        }

        public override void Read(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++) Entities.Add(Entity.Read(reader));
        }
    }


    public class ComponentUint : ComponentData
    {
        public uint Value;
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            Value = reader.ReadUInt32();
        }
    }


    public class ComponentByte : ComponentData
    {
        public byte Value;
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            Value = reader.ReadByte();
        }
    }


    public class ComponentPosition : ComponentData
    {
        public Vector2w Pos;
        public override void Write(BinaryWriter writer)
        {
            Pos.Write(writer);
        }

        public override void Read(BinaryReader reader)
        {
            Pos = Vector2w.Read(reader);
        }
    }


    public class ComponentFloat : ComponentData
    {
        public float Value;
        public override void Write(BinaryWriter writer)
        {
           writer.Write(Value);
        }

        public override void Read(BinaryReader reader)
        {
            Value = reader.ReadSingle();
        }
    }
}