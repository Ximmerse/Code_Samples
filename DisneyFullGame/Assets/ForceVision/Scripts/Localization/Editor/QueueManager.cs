using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Disney.ForceVision.Internal
{
	internal static class QueueManager
	{
		private class Job
		{
			public Func<bool> Completed { get; private set; }

			public Action ContinueWith { get; private set; }

			public Job(Func<bool> completed, Action continueWith)
			{
				Completed = completed;
				ContinueWith = continueWith;
			}
		}

		private static readonly List<Job> jobs = new List<Job>();

		/// <summary>
		/// Add the specified method and continueWith method, if one.
		/// </summary>
		/// <param name="completed">Completed method.</param>
		/// <param name="continueWith">Continue with method.</param>
		public static void Add(Func<bool> completed, Action continueWith)
		{
			if (!jobs.Any())
			{
				EditorApplication.update += Update;
			}
		
			jobs.Add(new Job(completed, continueWith));
		}
	
		// Call this each frame
		private static void Update()
		{	
			for (int i = 0; i >= 0; i--)
			{
				if (jobs[i].Completed())
				{
					jobs[i].ContinueWith();
					jobs.RemoveAt(i);
				}
			}	
		
			if (!jobs.Any())
			{
				EditorApplication.update -= Update;
			}	
		}
	}
}