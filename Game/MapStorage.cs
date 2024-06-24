using System;
using System.Collections.Generic;
using SWFServer.Data;
using SWFServer.Server;

namespace SWFServer.Game
{
    public class RequestMap
    {
        public Game Game;
        public uint mapId;
    }

    public class MapStorage: GameThread
    {
        private object locker = new object();

        private Dictionary<uint, Data.Map> maps = new Dictionary<uint, Data.Map>();
        private List<RequestMap> requestMaps = new List<RequestMap>();
        private List<uint> saveMaps = new List<uint>();

        private Rnd rnd = new Rnd();

        public MapStorage(int sleepWait, string threadName) : base(sleepWait, threadName, GameConst.mapStorageIndex)
        {
        }

        protected override void Init()
        {
            AddScheduleCall(0.1f, LoadMaps);
            AddScheduleCall(0.1f, SaveMaps);

            AddScheduleCall(30f, GenerateMaps);
        }

        protected override void Update(float dt)
        {
            
        }

        protected override void OnTerminate()
        {
            
        }

        private void GenerateMaps()
        {
            /*
            for (uint i = 0; i < WorldMap.CountMap; i++)
            {
                if (!File.Exists(GetMapFileName(i)))
                {
                    var map = new Map(i, rnd);
                    return;
                }
            }
            */
        }

        private void SaveMaps()
        {
            lock (locker)
            {
                if (saveMaps.Count > 0)
                {
                    for (int i = 0; i < saveMaps.Count; i++)
                    {
                        uint mapId = saveMaps[i];
                        if (maps.ContainsKey(mapId))
                        {
                            maps[mapId].Save();
                            maps.Remove(mapId);
                            Console.WriteLine("Unload map = " + mapId);
                        }
                    }

                    saveMaps.Clear();
                }
            }
        }

        private void LoadMaps()
        {
            lock (locker)
            {
                if (requestMaps.Count > 0)
                {
                    /*
                    for (int i = 0; i < requestMaps.Count; i++)
                    {
                        if(maps.ContainsKey(requestMaps[i].mapId))
                            requestMaps[i].Game.LoadMap(maps[requestMaps[i].mapId]);
                        else
                        {
                            var map = new Map(requestMaps[i].mapId);
                            requestMaps[i].Game.LoadMap(map);
                            maps.Add(map.Value, map);
                            Console.WriteLine("Load map = " + requestMaps[i].mapId);
                        }
                    }
                    */
                    requestMaps.Clear();
                }
            }
        }

        public void LoadMap(uint mapId, Game game)
        {
            lock (locker)
            {
                if (requestMaps.Find(f => f.mapId == mapId) != null)
                    return;

                requestMaps.Add(new RequestMap(){Game = game, mapId = mapId});
            }
        }

        public void SaveMap(uint mapId)
        {
            lock (locker)
            {
                if (!maps.ContainsKey(mapId))
                    return;
                saveMaps.Add(mapId);
            }
        }

        private string GetMapFileName(uint id)
        {
            return "data/maps/map" + id + ".dat";
        }
    }
}
