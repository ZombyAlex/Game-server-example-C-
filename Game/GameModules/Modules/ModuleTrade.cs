using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;
using System.Collections.Generic;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleTrade: GameModule
    {
        private ModuleLocation moduleLocation;
        private ModuleUnits moduleUnits;

        private Location locTrade;

        private readonly List<string> products = new List<string>()
            { "wood", "stone_item", "axe_stone", "pick_stone", "coal", "bottle"};

        public ModuleTrade(ModuleLocation moduleLocation, ModuleUnits moduleUnits)
        {
            this.moduleLocation = moduleLocation;
            this.moduleUnits = moduleUnits;
        }


        public override void Update(float dt)
        {
            if (locTrade == null)
            {
                foreach (var loc in moduleLocation.Locations)
                {
                    if (loc.Value.LocationType == LocationType.shop)
                    {
                        locTrade = loc.Value;
                        break;
                    }
                }
            }
            else
            {
                UpdateTradeLocation();
            }

        }

        private void UpdateTradeLocation()
        {
            ushort tableId = Info.EntityInfo["table_wood"].id;
            var map = locTrade.ModuleSystem.ModuleMap.Map;
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int y = 0; y < map.Size.y; y++)
                {
                    var p = new Vector2w(x, y);
                    if (map[p].Block != null && map[p].Block.Id == tableId)
                    {
                        var table = map[p].Block;
                        int need = Info.EntityInfo[tableId].tradeSize - table.Entities.Entities.Count;
                        if (need > 0)
                        {
                            CreateProduct(table);
                            locTrade.ModuleSystem.ModuleMapUpdate.SendAllCell(p);
                        }
                    }
                }
            }
        }

        private void CreateProduct(Entity table)
        {
            var info = Info.EntityInfo[products[Rnd.Range(0, products.Count)]];

            var item = Entity.Create(info.id);

            if (info.stackCount > 1)
            {
                item.Count.Value = Rnd.Range(5, 100);
            }

            item.AddComponent(new Component(ComponentClass.valInt, ComponentType.cost));
            item.Cost.Value = info.costSell * 2 * item.Count.Value;

            table.Entities.Entities.Add(item);
        }

        public override void Terminate()
        {
            
        }

        public void Buy(User user, Vector2w pos, int index, ushort itemId, int count)
        {
            var unit = moduleUnits.GetUnit(user);
            if (unit == null) return;

            var loc = moduleLocation.Locations[user.LocId];
            if (loc.OwnerId == user.Id) return;

            if (unit.Unit.CellPos.GetR(pos) > 1) return;

            var block = loc.ModuleSystem.ModuleMap.Map.GetBlock(pos);

            if (block.Item1 == null) return;
            if (block.Item1.Entities == null) return;
            if (index >= block.Item1.Entities.Entities.Count) return;

            var item = block.Item1.Entities.Entities[index];
            if (item.Id != itemId || item.Count.Value != count) return;

            if(item.Cost == null) return;

            if (user.Money < item.Cost.Value)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("no_money")));
                return;
            }

            if (!UtilInventory.IsAddItem(itemId, unit.Unit.InventorySize, count, unit.Entities.Entities))
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                return;
            }

            user.Money -= item.Cost.Value;

            item.RemoveComponent(ComponentType.cost);

            UtilInventory.AddItem(item, count, null, unit.Entities.Entities);
            block.Item1.Entities.Entities.RemoveAt(index);
            loc.ModuleSystem.ModuleMapUpdate.SendAllCell(block.Item2);

            if (loc.OwnerId != 0)
            {
                //TODO add money
            }

            user.SendMsg(new MsgServer(user.Id, MsgServerType.money, new MsgServerMoney(user.Money)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
        }
    }
}
