using System.Collections.Generic;
using SWFServer.Data.Entities;

namespace SWFServer.Data
{
    public static class UtilInventory
    {

        public static bool IsAddItem(ushort itemId, int limit, int count, List<Entity> inventory)
        {
            int maxCnt = Info.EntityInfo[itemId].stackCount;
            var it = inventory.Find(f => f.Id == itemId && f.Count.Value < maxCnt);
            if (it == null)
                return inventory.Count < limit;

            return it.Count.Value + count <= maxCnt;
        }

        public static void AddItem(Entity item, int count, List<Entity> source, List<Entity> inventory)
        {
            int maxCnt = Info.EntityInfo[item.Id].stackCount;
            var it = inventory.Find(f => f.Id == item.Id && f.Count.Value < maxCnt);
            if (it == null)
            {
                if (item.Count.Value > 1)
                {
                    it = Entity.Create(item.Id);
                    it.Count.Value = count;
                    inventory.Add(it);
                    item.Count.Value -= count;
                    if(item.Count.Value<=0)
                        source?.Remove(item);
                }
                else
                {
                    inventory.Add(item);
                    source?.Remove(item);
                }
            }
            else
            {
                it.Count.Value += count;
                item.Count.Value -= count;
                if (item.Count.Value <= 0)
                    source?.Remove(item);
            }
        }

        public static void MoveItem(ushort itemId, int count, List<Entity> source, List<Entity> inventory)
        {
            int maxCnt = Info.EntityInfo[itemId].stackCount;
            var it = inventory.Find(f => f.Id == itemId && f.Count.Value < maxCnt);
            var item = source.Find(f => f.Id == itemId);

            if (it == null)
            {
                if (item.Count.Value > 1)
                {
                    it = Entity.Create(item.Id);
                    int c = count;

                    if (c > maxCnt)
                        c = maxCnt;

                    it.Count.Value = c;
                    inventory.Add(it);
                    item.Count.Value -= c;
                    if (item.Count.Value <= 0)
                        source?.Remove(item);

                    count-= c;
                }
                else
                {
                    inventory.Add(item);
                    source?.Remove(item);
                    count--;
                }
            }
            else
            {

                int c = count;
                if (item.Count.Value < c)
                    c = item.Count.Value;

                if (it.Count.Value + c > maxCnt)
                    c = maxCnt - it.Count.Value;

                it.Count.Value += c;
                item.Count.Value -= c;
                count -= c;
                if (item.Count.Value <= 0)
                    source?.Remove(item);
            }

            if (count > 0)
                MoveItem(itemId, count, source, inventory);
        }

        public static bool IsItems(List<EntityInfoItemRes> items, List<Entity> inventory)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (!IsItem(items[i].id, items[i].count, inventory))
                    return false;
            }

            return true;
        }

        public static bool IsItem(string itemId, int count, List<Entity> inventory)
        {
            int cnt = 0;
            ushort id = Info.EntityInfo[itemId].id;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].Id == id)
                    cnt += inventory[i].Count.Value;
            }

            return cnt >= count;
        }

        public static bool IsItem(ushort itemId, int count, List<Entity> inventory)
        {
            int cnt = 0;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].Id == itemId)
                    cnt += inventory[i].Count.Value;
            }

            return cnt >= count;
        }

        public static void RemoveItem(List<EntityInfoItemRes> items, List<Entity> inventory)
        {
            for (int i = 0; i < items.Count; i++)
            {
                RemoveItem(Info.EntityInfo[items[i].id].id, items[i].count, inventory);
            }
        }

        public static void RemoveItem(ushort itemId, int count, List<Entity> inventory)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].Id == itemId)
                {
                    int c = count;
                    if (inventory[i].Count.Value < c)
                        c = inventory[i].Count.Value;

                    inventory[i].Count.Value -= c;
                    count -= c;

                    if (inventory[i].Count.Value == 0)
                    {
                        inventory.RemoveAt(i);
                        i--;
                    }
                }

                if (count == 0)
                    break;
            }
        }
    }
}
