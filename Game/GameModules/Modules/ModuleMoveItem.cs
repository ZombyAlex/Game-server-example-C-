using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleMoveItem: GameModule
    {
        private ModuleUnits moduleUnits;
        private ModuleLocation moduleLocation;

        public ModuleMoveItem(ModuleUnits moduleUnits, ModuleLocation moduleLocation)
        {
            this.moduleUnits = moduleUnits;
            this.moduleLocation = moduleLocation;
        }

        public override void Update(float dt)
        {
        }

        public override void Terminate()
        {
        }

        private void GetItems(User user, Vector2w pos, List<int> list)
        {
            var unit = moduleUnits.GetUnit(user);
            if(unit == null) return;

            //if (unit.Unit.CellPos.GetR(pos) > 1) return;

            var loc = moduleLocation.Locations[user.LocId];
            if (loc.OwnerId != user.Id) return;

            List<Entity> itemsGet = new List<Entity>();
            var block = loc.ModuleSystem.ModuleMap.Map.GetBlock(pos);
            if(block.Item1 == null) return;

            if (Info.EntityInfo[block.Item1.Id].isLiquid) return;

            foreach (var item in list)
            {
                itemsGet.Add(block.Item1.Entities.Entities[item]);
            }

            foreach (var item in itemsGet)
            {
                if (UtilInventory.IsAddItem(item.Id, unit.Unit.InventorySize, item.Count.Value, unit.Entities.Entities))
                {
                    UtilInventory.AddItem(item, item.Count.Value, block.Item1.Entities.Entities, unit.Entities.Entities);
                }
            }

            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(block.Item1.Entities.Entities, block.Item2)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
        }

        private void DropItems(User user, Vector2w pos, List<int> list)
        {
            var unit = moduleUnits.GetUnit(user);
            if (unit == null) return;

            var loc = moduleLocation.Locations[user.LocId];
            if (loc.OwnerId != user.Id) return;

            List<Entity> itemsGet = new List<Entity>();
            var block = loc.ModuleSystem.ModuleMap.Map.GetBlock(pos);
            if (block.Item1 == null) return;

            if (Info.EntityInfo[block.Item1.Id].isLiquid) return;

            foreach (var item in list)
            {
                if (!Info.EntityInfo[unit.Entities.Entities[item].Id].isLargeSize)
                    itemsGet.Add(unit.Entities.Entities[item]);
            }

            foreach (var item in itemsGet)
            {
                if (UtilInventory.IsAddItem(item.Id, Info.EntityInfo[block.Item1.Id].containerSize, item.Count.Value, block.Item1.Entities.Entities))
                {
                    UtilInventory.AddItem(item, item.Count.Value, unit.Entities.Entities, block.Item1.Entities.Entities);
                }
            }

            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(block.Item1.Entities.Entities, block.Item2)));
            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
        }

        public void MoveItems(User user, Vector2w pos, List<int> list, bool isGet)
        {
            if(isGet)
                GetItems(user, pos, list);
            else
                DropItems(user, pos, list);
        }

        public void TakeBlock(User user, Vector2w pos)
        {
            var unit = moduleUnits.GetUnit(user);
            if (unit == null) return;

            var loc = moduleLocation.Locations[user.LocId];
            if (loc.OwnerId != user.Id) return;

            if (unit.Entities.Entities.Count + 1 >= unit.Unit.InventorySize)
            {
                user.SendMsg(new MsgServer(user.Id, MsgServerType.info, new MsgServerInfo("inventory_full")));
                return;
            }

            var block = loc.ModuleSystem.ModuleMap.Map.GetBlock(pos);
            if(block.Item1 == null) return;

            if (!Info.EntityInfo[block.Item1.Id].isTake) return;

            unit.Entities.Entities.Add(block.Item1);

            loc.ModuleSystem.ModuleMap.Map.RemoveEntity(block.Item2, EntityMapLayer.block);
            loc.ModuleSystem.ModuleMapUpdate.SendAllCell(block.Item2);

            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
        }
    }
}
