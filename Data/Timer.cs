using System.Diagnostics;

namespace SWFServer.Data
{
	public class Timer
	{
		private Stopwatch timer;
		private double memDt;
		public Timer()
		{
			timer = new Stopwatch();
			timer.Start();
			memDt = timer.ElapsedTicks/(double)Stopwatch.Frequency;
		}
		public double DeltaTime()
		{
			double aDt = timer.ElapsedTicks / (double)Stopwatch.Frequency;
			double aVal = aDt - memDt;
			memDt = aDt;
			return aVal;
		}
	}
}
