using UnityEngine;
using Disney.Vision;
using Disney.ForceVision;

namespace Disney.ForceVision.Internal
{
	[CreateAssetMenu(menuName = "ForceVision/Create VisionSdkLoggerProxy Logger")]
	public class VisionSdkLoggerProxy : VisionLogger
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisionLoggerProxy"/> class.
		/// </summary>
		public VisionSdkLoggerProxy()
		{
			#if !RC_BUILD
			LogToConsole = true;
			#endif
		}

		/// <summary>
		/// Log the specified text.
		/// </summary>
		/// <param name="text">Text.</param>
		public override void Log(string text)
		{
			Disney.ForceVision.Log.Debug(text, Disney.ForceVision.Log.LogChannel.VisionSdk);
		}

		/// <summary>
		/// remove the UnityEngine log listener.
		/// </summary>
		public override void Destroy()
		{
		}

	}
}