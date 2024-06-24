using System;
using System.IO;
using Newtonsoft.Json;
using SWFServer.Data;
using SWFServer.Data.Entities;

namespace SWFServer.Server
{
    public static class ServerData
    {
        public static bool stopServer = false;
        public static double serverTime = 360;

        public static void Init()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            Info.Init(LoadJson("entities.json"));
        }

       
        private static string LoadJson(string path)
        {
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();
            return json;
        }
    }
}
