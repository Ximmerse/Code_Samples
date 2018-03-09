using System;
using UnityEngine;
using Build;

namespace Disney.ForceVision
{
	public static class DevonCommandLine
	{
		public static void PullFromDevon()
		{
			Console.WriteLine("DevonCommandLine PullFromDevon");

			var buildArgs = new BuildArgs("Devon", 0, 0);

			if (buildArgs.CmdLineArgs.ContainsKey("devonOAuth"))
			{
				bool done = false;

				DevonAPI.PullFromDevon(Game.ForceVision.ToString(), buildArgs.CmdLineArgs["devonOAuth"].ToString(), (result) =>
				{
					Console.WriteLine("Pull Result: " + result);
					done = true;
				});

				// need to wait for async operation above to finish because when run from command line Unity will exit after this command.
				while (!done)
				{
					System.Threading.Thread.Sleep(10);
					UnityEditor.EditorApplication.update();
				}
			}
			else
			{
				Console.WriteLine("Error: devonOAuth must be specified");
				throw new Exception("DevonCommandLine failure: -devonOAuth=<auth> must be specified");
			}
		}
	}
}

