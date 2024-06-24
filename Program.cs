using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Game;
using SWFServer.Server;
using SWFServer.Server.Net;

namespace SWFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLog("start");

            Console.WriteLine("Proccesor count = " + Environment.ProcessorCount);
            Console.WriteLine("Platform = " + Environment.OSVersion.Platform);

            bool aIsWindows = Environment.OSVersion.Platform.ToString() == "Win32NT";



           



#if DEBUG_TRY

            try
            {
#endif

            InitDirectory();

                if (File.Exists("close.txt"))
                {
                    File.Delete("close.txt");
                }

                ServerData.Init();

                WorldManager.Init();
                Rating.Init();



                NetMaster netMaster = new NetMaster(20, "NetMaster");
                Thread netMasterThread = new Thread(netMaster.Run) {Name = "NetMaster"};

                netMasterThread.Start();

                
                Console.WriteLine("Server is ready");

                while (true)
                {
                    if (ServerData.stopServer)
                        break;

                    Thread.Sleep(500);

                    if (aIsWindows)
                    {
                        string? aCommand = Console.ReadLine();
                        if (aCommand == "exit")
                        {
                            break;
                        }
                        else if (aCommand == "regen")
                        {
                            WorldManager.RegenWorld();
                        }
                    }

                    if (File.Exists("exit.txt"))
                    {
                        File.Delete("exit.txt");
                        ServerData.stopServer = true;
                    }

                }

                netMaster.Terminate();
                netMasterThread.Join();

                StreamWriter aWriter = new StreamWriter("close.txt", true);
                aWriter.WriteLine("0");
                aWriter.Close();


                WorldManager.Terminate();
                Rating.Terminate();

                WriteLog("close");


#if DEBUG_TRY
            }
            catch (Exception ex)
            {
                Tools.SaveCrash(ex.ToString(), true);
            }
#endif
        }


        private static void InitDirectory()
        {
            CheckAndCreateDir("backup");
            CheckAndCreateDir("logs");
            CheckAndCreateDir("logs/shedule");
            CheckAndCreateDir("data");
            CheckAndCreateDir("data/game");
            CheckAndCreateDir("data/maps");
            CheckAndCreateDir("data/units");
            CheckAndCreateDir("data/locations");
            
            CheckAndCreateDir("users");
            CheckAndCreateDir("static");
            
        }

        private static void CheckAndCreateDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void WriteLog(string inText)
        {
            StreamWriter aWriter = new StreamWriter("start.txt", true);
            aWriter.WriteLine("------------------------------------------------------------------------------------------------");
            aWriter.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + inText);
            aWriter.Close();
        }
    }
}
