using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SWFServer.Server
{
	public abstract class GameThread
	{
		private bool isTerminate;
        private ScheduleHandler scheduleHandler;
		private int sleepWait;
		private string threadName;

        private List<GamePeriod> periods = new List<GamePeriod>();

        protected uint gameIndex;


		public GameThread(int sleepWait, string threadName, uint gameIndex)
		{
			this.sleepWait = sleepWait;
			this.threadName = threadName;
            this.gameIndex = gameIndex;
            scheduleHandler = new ScheduleHandler("g_" + gameIndex);
        }

        public void Run()
        {
#if DEBUG_TRY
            try
            {
#endif
                PreInit();
                Init();

#if DEBUG_TRY
            }
            catch (Exception ex)
            {
                Tools.SaveCrash(ex.ToString(), true);
                throw;
            }
#endif


            Data.Timer timer = new Data.Timer();

            while (!isTerminate)
            {
#if DEBUG_TRY
                try
                {
#endif
                    float dt = (float) timer.DeltaTime();

                    scheduleHandler.Update(dt);

                    Update(dt);

                    Thread.Sleep(sleepWait);

#if DEBUG_TRY
                }
                catch (Exception ex)
                {
                    Tools.SaveCrash(ex.ToString(), false);
                }
#endif

            }


#if DEBUG_TRY
            try
            {
#endif

                OnTerminate();
                Console.WriteLine("Stop " + threadName);
#if DEBUG_TRY
            }
            catch (Exception ex)
            {
                Tools.SaveCrash(ex.ToString(), false);
            }
#endif
        }

        private void PreInit()
        {
            AddScheduleCall(10.0f, UpdatePeriods);
        }

        private void UpdatePeriods()
        {
            for (int i = 0; i < periods.Count; i++)
            {
                periods[i].UpdateServerTime();
            }
        }

        protected abstract void Init();

        protected abstract void Update(float dt);

        protected abstract void OnTerminate();

        public void Terminate()
		{
			Console.WriteLine("Terminate " + threadName);
			isTerminate = true;
		}


		protected void WriteLog(string text)
		{
			StreamWriter aWriter = new StreamWriter("logs/log_" + threadName + ".txt", true);
			aWriter.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
			aWriter.Close();
		}

	    protected void AddScheduleCall(float time, ProcFunction function)
	    {
            scheduleHandler.AddScheduleCall(time, function);
	    }

        protected void AddScheduleUpdate(float time, ProcFunctionUpdate function)
        {
            scheduleHandler.AddScheduleUpdate(time, function);
        }

        protected void AddPeriod(GamePeriod period)
        {
			periods.Add(period);
        }

	}
}
