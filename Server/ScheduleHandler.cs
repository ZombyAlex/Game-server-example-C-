using System.Collections.Generic;

namespace SWFServer.Server
{
    public class ScheduleHandler
    {
        private List<Schedule> scheduleList = new List<Schedule>();

        private string id;

        public ScheduleHandler(string id)
        {
            this.id = id;
        }

        public void AddScheduleCall(float time, ProcFunction function)
        {
            scheduleList.Add(new ScheduleCall(time, function, id));
        }

        public void AddScheduleUpdate(float time, ProcFunctionUpdate function)
        {
            scheduleList.Add(new ScheduleUpdate(time, function, id));
        }

        public void Update(float dt)
        {
            for (int i = 0; i < scheduleList.Count; i++)
            {
                scheduleList[i].Update(dt);
            }
        }
    }
}
