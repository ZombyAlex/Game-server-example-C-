using System;
using System.Diagnostics;
using System.IO;

namespace SWFServer.Server
{
	public class Tools
	{
        static object lockAnaliticsFile = new object();
        static object lockFile = new object();
        public static void SaveCrash(string text, bool isStopServer)
		{
			using (StreamWriter writer = File.AppendText("crash.txt"))
			{
				writer.WriteLine("------------------------------------------------------------------------------------------------------------");
				writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
			    if (isStopServer)
			        ServerData.stopServer = true;
			}
		}

		public static void MegaLog(string threadName, string text)
		{
			File.WriteAllText("mega_log_" + threadName + ".txt", text);
		}

		public static void Log(string filename, string text)
		{
			using (StreamWriter writer = File.AppendText(filename))
			{
				writer.WriteLine(text);
			}
		}

        public static void LogSafe(string filename, string text)
        {
            lock (lockFile)
            {
                using (StreamWriter writer = File.AppendText(filename))
                {
                    writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
                }
            }
        }

        public static void LogData(string filename, string text)
        {
            using (StreamWriter writer = File.AppendText(filename))
            {
                writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
            }
        }

		public static void LogRewrite(string filename, string text)
        {
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine(text);
            }
        }

		private static object lockSheduleFile = new object();

        public static void LogShedule(string filename, string text)
        {
            //lock (lockSheduleFile)
            {
                using (StreamWriter writer = File.AppendText(filename))
                {
                    writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
                }
            }
        }


        public delegate void ProcFunction();

		public static void CallFunc(ProcFunction func, uint index)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			func();
			sw.Stop();
			WriteLogTime(sw.ElapsedMilliseconds, func.Method.Name, index);
		}

        private static void WriteLogTime(long timeMilliseconds, string funcName, uint idx)
		{
			if (timeMilliseconds >= 1000)
				LogShedule("logs/shedule/time_sheduleH" + idx + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
			else if (timeMilliseconds >= 100)
				LogShedule("logs/shedule/time_sheduleM" + idx + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
			//else if (timeMilliseconds >= 50)
			//	Util.WriteLog("time_sheduleL.txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
		}

		public delegate void ProcFunctionUpdate(float dt);

		public static void CallFunc(ProcFunctionUpdate func, float dt, uint index)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			func(dt);
			sw.Stop();
			WriteLogTime(sw.ElapsedMilliseconds, func.Method.Name, index);
		}

        public static string GetTimeString(double time)
        {
            int h = (int) (time / 3600);
            int m = (int) ((time % 3600) / 60);
            int s = (int) (time % 60);

            return h.ToString() + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        public static void SaveAnalytics(string text)
        {
            lock (lockAnaliticsFile)
            {
                var f = File.AppendText("analytics.txt");
                f.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
                f.Close();
            }
        }
        /*
        public delegate void ProcFunctionUnit(Unit unit);

        public static void CallFuncUnit(ProcFunctionUnit func, Unit unit, uint index)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            func(unit);
            sw.Stop();
            WriteLogTime(sw.ElapsedMilliseconds, func.Method.Name, index);
        }
        */


        public static void TestAction(string funcName, Action action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            WriteLogTimeTest(sw.ElapsedMilliseconds, funcName);
        }

        private static void WriteLogTimeTest(long timeMilliseconds, string funcName)
        {
            if (timeMilliseconds >= 1000)
                LogShedule("logs/shedule/test_sheduleH" + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
            else if (timeMilliseconds >= 100)
                LogShedule("logs/shedule/test_sheduleM" + ".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);

            //else if (timeMilliseconds >= 50)
            //	Util.WriteLog("time_sheduleL.txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
        }
    }
}
