using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Game.GameModules.Modules;
using SWFServer.Server;

namespace SWFServer.Game.GameModules
{
    public class ModuleSystem
    {
        private ScheduleHandler scheduleHandler;
        private List<GameModule> modules = new List<GameModule>();


        public ModuleCheat ModuleCheat { get; private set; }
        public ModuleUnits ModuleUnits { get; private set; }
        public ModuleMap ModuleMap { get; private set; }
        public ModuleMapGrid ModuleMapGrid { get; private set; }
        public ModuleMapUpdate ModuleMapUpdate { get; private set; }
        public ModuleUnitsUpdate ModuleUnitsUpdate { get; private set; }
        public ModuleMining ModuleMining { get; private set; }
        public ModuleTask ModuleTask { get; private set; }
        public ModuleCraft ModuleCraft { get; private set; }
        public ModuleLocation ModuleLocation { get; private set; }
        public ModuleEquip ModuleEquip { get; private set; }
        public ModuleBuild ModuleBuild { get; private set; }
        public ModuleMoveItem ModuleMoveItems { get; private set; }
        public ModuleMiningWater ModuleMiningWater { get; private set; }
        public ModuleUseItem ModuleUseItem { get; private set; }
        public ModuleTrade ModuleTrade { get; private set; }



        public ModuleSystem(uint locationId, ModuleUsers moduleUsers, ModuleTask moduleTask, ModuleLocation moduleLocation, LocationType locationType, FuncCreateLocation createLocation, (uint, Vector2w) exit, int level)
        {
            scheduleHandler = new ScheduleHandler("l_" + locationId);

            ModuleTask = moduleTask;
            ModuleLocation = moduleLocation;

            ModuleMap = (ModuleMap)AddModule(new ModuleMap(locationId, locationType, createLocation, exit, level));
            ModuleMapGrid = (ModuleMapGrid)AddModule(new ModuleMapGrid(ModuleMap));
            ModuleUnits = (ModuleUnits)AddModule(new ModuleUnits(ModuleMap, moduleUsers, ModuleMapGrid, moduleLocation));

            ModuleCheat = (ModuleCheat)AddModuleUpdate(new ModuleCheat(moduleUsers, ModuleUnits), 5f);

            ModuleUseItem = (ModuleUseItem)AddModule(new ModuleUseItem(ModuleUnits));

            ModuleMapUpdate = (ModuleMapUpdate)AddModule(new ModuleMapUpdate(moduleUsers, ModuleMap, ModuleMapGrid, ModuleUnits, ModuleUseItem));
            ModuleUnitsUpdate = (ModuleUnitsUpdate)AddModuleUpdate(new ModuleUnitsUpdate(ModuleUnits, ModuleMap, ModuleMapGrid, ModuleMapUpdate), 0.05f);

            ModuleMining = (ModuleMining)AddModuleUpdate(new ModuleMining(ModuleUnits, ModuleMap, ModuleMapUpdate), 0.1f);
            ModuleCraft = (ModuleCraft)AddModuleUpdate(new ModuleCraft(ModuleUnits, ModuleMap, ModuleLocation, ModuleTask, ModuleMapUpdate), 0.1f);

            ModuleEquip = (ModuleEquip)AddModule(new ModuleEquip(ModuleUnits));
            ModuleBuild = (ModuleBuild)AddModule(new ModuleBuild(ModuleUnits, ModuleLocation, createLocation));
            ModuleMoveItems = (ModuleMoveItem)AddModule(new ModuleMoveItem(ModuleUnits, ModuleLocation));

            ModuleMiningWater = (ModuleMiningWater)AddModuleUpdate(new ModuleMiningWater(ModuleUnits, ModuleMap, ModuleLocation), 0.1f);

            ModuleTrade = (ModuleTrade)AddModuleUpdate(new ModuleTrade(moduleLocation, ModuleUnits), 30f);
        }

        private GameModule AddModule(GameModule module)
        {
            modules.Add(module);
            return module;
        }

        private GameModule AddModuleUpdate(GameModule module, float time)
        {
            modules.Add(module);
            scheduleHandler.AddScheduleUpdate(time, module.Update);
            return module;
        }

        public void Update(float dt)
        {
            scheduleHandler.Update(dt);
        }


        public void Terminate()
        {
            foreach (var m in modules)
            {
                m.Terminate();
            }
        }

    }
}
