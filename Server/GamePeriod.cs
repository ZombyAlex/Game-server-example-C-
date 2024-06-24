using System;

namespace SWFServer.Server
{
    public class GamePeriod
    {
        private int period = 0;
        private int time = 0;
        private Action action;

        public GamePeriod(double serverTime, int time, Action action)
        {
            this.time = time;
            this.action = action;
            period = (int) (serverTime / time);
        }

        public void UpdateServerTime()
        {
            int nextPeriod = (int)(ServerData.serverTime / time);
            if (period != nextPeriod)
            {
                period = nextPeriod;
                action();
            }
        }
    }
}
