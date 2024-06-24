using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SWFServer.Data.Entities
{
    public enum EntityMapLayer
    {
        ground,
        floor,
        block,
        item, //TODO
        unit
    }

    public enum WorkbenchType
    {
        not,
        workbench,
        furnance,
        sawmill,
        masons_workshop
    }

    public class EntityInfoItemRes
    {
        public string id;
        public int count;
    }

    public class EntityInfoItemCraft
    {
        public float time;
        public List<EntityInfoItemRes> items;
    }

    public enum ToolType
    {
        axe,
        pick
    }

    public class EntityInfoItemTool
    {
        public ToolType type;
        public int level;
    }

    public class EntityInfoItemBuild
    {
        public int cost;
        public List<LocationType> locations = new List<LocationType>();
        public LocationType location = LocationType.world;

        public bool IsLocation(LocationType locationType)
        {
            for (int i = 0; i < locations.Count; i++)
            {
                if (locations[i] == locationType)
                    return true;
            }
            return false;
        }
    }

    public enum MiningType
    {
        wood,
        ore,
        plant
    }


    public class EntityInfoItem
    {
        public ushort id;
        public string name;
        public int costCraft;
        public int costSell = 1;
        public EntityMapLayer layer;
        public bool isMove = true;
        public Vector2w size = Vector2w.One;
        public float speedMove = 1f;
        public string switchItem = String.Empty;
        public bool isSwitch = false;
        public bool isBlockUnit = false;
        public bool isSleep = false;
        public bool isExit = false;
        public Vector2w exit = Vector2w.Zero;
        public int stackCount = 1;
        public WorkbenchType workbenchType = WorkbenchType.not;
        public WorkbenchType workbenchCraft = WorkbenchType.not;
        public int containerSize = 0;
        public int tradeSize = 0;
        public bool isLiquid = false;
        public bool isLiquidContainer = false;
        public bool isLargeSize = false;
        public int liquidSize = 0;
        public bool isWater = false;
        public bool isUse = false;
        public bool isTake = false;
        public bool isEquip = false;

        public List<EntityInfoItemRes> res;
        public float timeMining = 1f;
        public MiningType miningType;

        public EntityInfoItemCraft craft;
        public float durability = 0;

        public EntityInfoItemTool tool;
        public EntityInfoItemTool toolMining;
        public EntityInfoItemBuild build;
    }

    public class EntityInfo
    {
        public List<EntityInfoItem> items = new List<EntityInfoItem>();

        [JsonIgnore] private Dictionary<int, EntityInfoItem> dicItems = new Dictionary<int, EntityInfoItem>();
        [JsonIgnore] public Dictionary<string, EntityInfoItem> dicItemsStr = new Dictionary<string, EntityInfoItem>();

        public void Init()
        {
            foreach (EntityInfoItem it in items)
            {
                dicItems.Add(it.id, it);
                dicItemsStr.Add(it.name, it);
            }
        }

        public EntityInfoItem this[ushort id]
        {
            get { return dicItems[id]; }
        }

        public EntityInfoItem this[string id]
        {
            get { return dicItemsStr[id]; }
        }
    }
}
