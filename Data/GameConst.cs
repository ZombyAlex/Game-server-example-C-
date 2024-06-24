using System;
using System.Collections.Generic;

namespace SWFServer.Data
{
    public static class GameConst
    {
        public static int port = 58888;
        public static int countNetThreadConnect = 64;
        public static string serverName = "SWF_0_1_0";

        public static float cellSize = 1f;
        public static int mapGrid = 16;
        public static int mapUnitGrid = 4;

        public static float timeDisconnectUnload = 5.0f;
        public static float timeMapUnload = 600.0f;

        public static long GetLevelExp(long level)
        {
            return (long) Math.Pow((5 * level), 3.0);
        }

        public static int[] timeBan = new[] {5, 10, 30, 60, 60 * 5, 60 * 24};
        public static float timeRemoveEmptyNetThread = 5;
        public static int netServerCount = 5;

        public static float minute = 2.5f;
        public static int hour = 150;
        public static int day = 3600;
        public static float month = 86400;
        public static float year = 1036800;
        public static uint netIndex = 4000000000;
        public static uint netMasterIndex = 4004000000;
        public static uint mapStorageIndex = 4005000000;


        public static int costClan = 100000;//TODO
        public static float timeCreateModule = 10f;

        public static List<WRect> buildPlaces = new List<WRect> { new WRect(64, 0, 48, 112), new WRect(144, 0, 48, 112), new WRect(112, 64, 32, 48) };
        public static WRect cityRect = new WRect(64, 0, 128, 112);

        public static float GetActionExp(long level)
        {
            return (float) Math.Pow(level + 1, 1.05f);
        }
    }
}
