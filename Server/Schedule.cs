using System.Diagnostics;

namespace SWFServer.Server
{

	public abstract class Schedule
	{
		protected float time;
		protected float curTime = 0.0f;
		public Schedule(float time)
		{
			this.time = time;
        }

        public abstract void Update(float dt);

        protected void WriteLogTime(long timeMilliseconds, string funcName, string id)
		{
			if (timeMilliseconds >= 1000)
				Tools.LogShedule("logs/shedule/time_sheduleH"+id+".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
			else if (timeMilliseconds >= 100)
				Tools.LogShedule("logs/shedule/time_sheduleM"+id+".txt", "f=" + funcName + " time = " + timeMilliseconds / 1000.0);
			//else if (timeMilliseconds >= 50)
			//	Util.WriteLog("time_sheduleL.txt", "f=" + funcName + " time = " + timeMilliseconds/1000.0);
		}
	}

	public delegate void ProcFunction();

	public class ScheduleCall : Schedule
	{
		private readonly ProcFunction func;
		private bool debug = false;
        private string id;
		public ScheduleCall(float time, ProcFunction inFunc, string id, bool inDebug = false) 
			: base(time)
		{
			debug = inDebug;
			func = inFunc;
			debug = false;
            this.id = id;
        }

		public override void Update(float dt)
		{
			curTime += dt;
			if (curTime >= time)
			{
				curTime -= time;
				if (debug)
					Tools.MegaLog("shedule.txt", "s=" + func.Method.Name);
				Stopwatch sw = new Stopwatch();
				sw.Start();
				func();
				sw.Stop();
				WriteLogTime(sw.ElapsedMilliseconds, func.Method.Name, id);
				if (debug)
					Tools.MegaLog("shedule.txt", "d=" + func.Method.Name);
			}
		}
	}

	public delegate void ProcFunctionUpdate(float dt);

	public class ScheduleUpdate: Schedule
	{
		private readonly ProcFunctionUpdate func;
		private bool debug = false;
        private string id;

        public ScheduleUpdate(float time, ProcFunctionUpdate inFunc, string id, bool inDebug = false)
			: base(time)
		{
			debug = inDebug;
			func = inFunc;
			debug = false;
            this.id = id;
		}

		public override void Update(float dt)
		{
			curTime += dt;
			if (curTime >= time)
			{
				curTime -= time;
				if (debug)
					Tools.MegaLog("shedule.txt", "s=" + func.Method.Name);
				Stopwatch sw = new Stopwatch();
				sw.Start();
				func(time);
				sw.Stop();
				WriteLogTime(sw.ElapsedMilliseconds, func.Method.Name, id);
				if (debug)
					Tools.MegaLog("shedule.txt", "d=" + func.Method.Name);
			}
		}
	}
}
