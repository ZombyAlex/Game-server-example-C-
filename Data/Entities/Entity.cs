using System;
using System.Collections.Generic;
using System.IO;
using Lidgren.Network;
using Newtonsoft.Json;

namespace SWFServer.Data.Entities
{
    public class Entity
    {
        public ushort Id;
        public Dictionary<ComponentType, Component> components = new Dictionary<ComponentType, Component>();


        [JsonIgnore] private ComponentUnit unit;
        [JsonIgnore] private ComponentInt count;
        [JsonIgnore] private ComponentEntities entities;
        [JsonIgnore] private ComponentUint userId;
        [JsonIgnore] private ComponentByte rotate;
        [JsonIgnore] private ComponentUint exitLocId;
        [JsonIgnore] private ComponentPosition exitPos;
        [JsonIgnore] private ComponentFloat durability;
        [JsonIgnore] private ComponentInt cost;


        [JsonIgnore] public ComponentUnit Unit => unit ??= components.ContainsKey(ComponentType.unit) ? (ComponentUnit)components[ComponentType.unit].Data : null;
        [JsonIgnore] public ComponentInt Count => count ??= components.ContainsKey(ComponentType.count) ? (ComponentInt)components[ComponentType.count].Data : null;
        [JsonIgnore] public ComponentEntities Entities => entities ??= components.ContainsKey(ComponentType.entities) ? (ComponentEntities)components[ComponentType.entities].Data : null;
        [JsonIgnore] public ComponentUint UserId => userId ??= (userId = components.ContainsKey(ComponentType.id) ? (ComponentUint)components[ComponentType.id].Data : null);
        [JsonIgnore] public ComponentByte Rotate => rotate ??= (rotate = components.ContainsKey(ComponentType.rotate) ? (ComponentByte)components[ComponentType.rotate].Data : null);
        [JsonIgnore] public ComponentUint ExitLocId => exitLocId ??= (exitLocId = components.ContainsKey(ComponentType.exitLocId) ? (ComponentUint)components[ComponentType.exitLocId].Data : null);
        [JsonIgnore] public ComponentPosition ExitPos => exitPos ??= (exitPos = components.ContainsKey(ComponentType.exitPos) ? (ComponentPosition)components[ComponentType.exitPos].Data : null);
        [JsonIgnore] public ComponentFloat Durability => durability ??= (durability = components.ContainsKey(ComponentType.durability) ? (ComponentFloat)components[ComponentType.durability].Data : null);
        [JsonIgnore] public ComponentInt Cost => cost ??= components.ContainsKey(ComponentType.cost) ? (ComponentInt)components[ComponentType.cost].Data : null;

        public Entity()
        { }

        public Entity(ushort id)
        {
            Id = id;
        }

        public Entity(Entity entity)
        {
            Id = entity.Id;
        }

        public static Entity Create(ushort id)
        {
            var info = Info.EntityInfo[id];
            Entity entity = new Entity(id);
            if (info.layer == EntityMapLayer.unit)
            {
                entity.AddComponent(new Component(ComponentClass.unit, ComponentType.unit));
                entity.AddComponent(new Component(ComponentClass.entities, ComponentType.entities));
            }

            if (info.res != null && info.res.Count > 0)
            {
                entity.AddComponent(new Component(ComponentClass.entities, ComponentType.entities));
                for (int i = 0; i < info.res.Count; i++)
                {
                    var item = Create(info.res[i].id);
                    item.Count.Value = info.res[i].count;
                    entity.Entities.Entities.Add(item);
                }
            }

            if (info.layer == EntityMapLayer.item || info.layer == EntityMapLayer.block || info.layer == EntityMapLayer.floor)
            {
                entity.AddComponent(new Component(ComponentClass.valInt, ComponentType.count));
                entity.Count.Value = 1;
            }

            if (info.containerSize > 0 || info.tradeSize > 0)
            {
                entity.AddComponent(new Component(ComponentClass.entities, ComponentType.entities));
            }

            if (info.durability > 0)
            {
                entity.AddComponent(new Component(ComponentClass.valFloat, ComponentType.durability));
                entity.Durability.Value = info.durability;
            }

            return entity;
        }

        public static Entity Create(string id)
        {
            return Create(Info.EntityInfo[id].id);
        }

        public void AddComponent(Component c)
        {
            components.Add(c.Type, c);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(components.Count);
            foreach (var c in components)
            {
                //writer.Write((byte)c.Key);
                c.Value.Write(writer);
            }
        }

        public static Entity Read(BinaryReader reader)
        {
            Entity e = new Entity();
            e.Id = reader.ReadUInt16();
            int cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                //ComponentType t = (ComponentType)reader.ReadByte();
                Component c = Component.Read(reader);
                e.AddComponent(c);
            }

            return e;
        }

        public void RemoveComponent(ComponentType componentType)
        {
            components.Remove(componentType);

            switch (componentType)
            {
                case ComponentType.unit:
                    unit = null;
                    break;
                case ComponentType.count:
                    count = null;
                    break;
                case ComponentType.entities:
                    entities = null;
                    break;
                case ComponentType.id:
                    userId = null; 
                    break;
                case ComponentType.rotate:
                    rotate = null;
                    break;
                case ComponentType.exitLocId:
                    exitLocId = null;
                    break;
                case ComponentType.exitPos:
                    exitPos = null;
                    break;
                case ComponentType.durability:
                    durability = null;
                    break;
                case ComponentType.cost:
                    cost = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
        }
    }
}
