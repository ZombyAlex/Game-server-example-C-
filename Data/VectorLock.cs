using System.Collections.Generic;
using System.Threading;

namespace SWFServer.Data
{
	public class VectorLock
	{
		private List<System.Object> work = new List<System.Object>();
		private readonly List<System.Object> temp = new List<System.Object>();
		
		private object locker = new object();
		public void Add(System.Object inObj)
		{
			lock (locker)
			{
				temp.Add(inObj);
			}
		}
		public bool AddFast(System.Object inObj)
		{
			if (!Monitor.TryEnter(locker))
				return false;
			temp.Add(inObj);
			Monitor.Exit(locker);
			return true;
		}
		public bool AddFastList(List<System.Object> inList)
		{
			if (!Monitor.TryEnter(locker))
				return false;
			temp.AddRange(inList);
			Monitor.Exit(locker);
			return true;
		}
		public void ToWork()
		{
			lock (locker)
			{
				work.AddRange(temp);
				temp.Clear();
			}
		}
		public bool ToWorkFast()
		{
			if (!Monitor.TryEnter(locker))
				return false;
			if (temp.Count==0)
			{
				Monitor.Exit(locker);
				return false;
			}
			work.AddRange(temp);
			temp.Clear();
			Monitor.Exit(locker);
			return true;
		}

	    public List<System.Object> GetWork()
	    {
            List<System.Object> l = new List<System.Object>(work);
            work.Clear();
	        return l;
	    }

	    public bool IsWork()
	    {
	        return work.Count > 0;
	    }
	}
}

