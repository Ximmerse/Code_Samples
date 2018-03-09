using Disney.Vision;
using UnityEngine;

namespace Disney.ForceVision
{
	public class ConsoleLogger : VisionLogger
	{
		public void Log(string text)
		{
			#if DEVELOPMENT_BUILD
			Debug.Log(text);
			#endif
		}

		public void Destroy()
		{
		}
	}
}

