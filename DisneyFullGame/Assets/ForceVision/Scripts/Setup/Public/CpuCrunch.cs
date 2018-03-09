using System.Threading;
using System.Timers;
using System;

public class CpuCrunch
{
	public static int Routines = 10000;

	private static System.Timers.Timer timer = null;

	public static void StartCrunch()
	{
		StopCrunch();
		if (timer == null)
		{
			timer = new System.Timers.Timer(50);
			timer.Elapsed += OnTimedEvent;
			timer.Enabled = true;
		}
	}

	public static void StopCrunch()
	{
		if (timer != null)
		{
			timer.Stop();
			timer.Dispose();
			timer = null;
		}
	}

	public static void OnTimedEvent(Object source, ElapsedEventArgs e)
	{
		ThreadPool.QueueUserWorkItem(ThreadWork);
	}

	public static void ThreadWork(Object stateInfo)
	{
		for (int i = 1; i < Routines; i++)
		{
			GetPrimeNumber(i);
		}
		UnityEngine.Debug.LogError("finished primes");
	}

	public static bool GetPrimeNumber(long number)
	{
		for (int i = 2; i < (number / 2); i++)
		{
			if (number % i == 0)
			{
				return false;
			}
		}
		return true;
	}

}
