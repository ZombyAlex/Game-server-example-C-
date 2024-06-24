using System.Collections.Generic;
using System.IO;
using System.Threading;
using SWFServer.Data;

namespace SWFServer.Game
{
    public class GameThreadInfo
    {
        public uint Id;
        public Game Game;
        public Thread GameThread;

        public GameThreadInfo(uint id, Game game, Thread gameThread)
        {
            Id = id;
            Game = game;
            GameThread = gameThread;
        }

        public bool GetLocation(uint locId)
        {
            return Game.GetLocation(locId);
        }
    }

    public static class WorldManager
    {
        private static List<GameThreadInfo> games = new List<GameThreadInfo>();

        public static WorldUserManager UserManager;


        private static object locker = new object();

        public static MapStorage mapStorage = null;
        private static Thread mapStorageThread = null;

        private static Rnd rnd = new Rnd();


        public static void Init()
        {
            UserManager = new WorldUserManager();

            mapStorage = new MapStorage(20, "MapStorage");
            mapStorageThread = new Thread(mapStorage.Run){Name = "MapStorage" };
            mapStorageThread.Start();

            Game game = new Game(10, "Game", 0);
            Thread gameThread = new Thread(game.Run) { Name = "Game0" };

            games.Add(new GameThreadInfo(0, game, gameThread));

            foreach (var g in games)
            {
                g.GameThread.Start();
            }
        }

        public static void Terminate()
        {
            for (int i = 0; i < games.Count; i++)
            {
                games[i].Game.Terminate();
                games[i].GameThread.Join();
            }

            games.Clear();

            mapStorage.Terminate();
            mapStorageThread.Join();

            UserManager.SaveGameState();
        }

        public static GameThreadInfo GetGame(User user)
        {
            foreach (var game in games)
            {
                if (game.GetLocation(user.LocId))
                    return game;
            }

            user.LocId = 1;
            return GetGame(user);
        }

        public static void RegenWorld()
        {
            games[0].Game.RegenWorld();
        }
    }
}
