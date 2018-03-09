using System;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class GetAndroidBrightnessRunnable : NativeAndroid
	{
		#region Private Fields

		/// <summary>
		/// The callback.
		/// </summary>
		private Action<float> callback;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.GetAndroidBrightnessRunnable"/> class.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public GetAndroidBrightnessRunnable(Action<float> callback)
		{
			this.callback = callback;
			AndroidJavaObject activity = GetCurrentAndroidActivity();
			activity.Call("runOnUiThread", new AndroidJavaRunnable(RunOnUIThread));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Get Android phone brightness
		/// </summary>
		private void RunOnUIThread()
		{
			AndroidJavaObject window = GetAndroidWindow();
			AndroidJavaObject layout = window.Call<AndroidJavaObject>("getAttributes");

			float brightness = layout.Get<float>("screenBrightness");
			if (brightness < 0)
			{
				AndroidJavaObject currentActivity = GetCurrentAndroidActivity();
				AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
				AndroidJavaClass system = new AndroidJavaClass("android.provider.Settings$System");
				string SCREEN_BRIGHTNESS = system.GetStatic<string>("SCREEN_BRIGHTNESS");
				int intBrightness = system.CallStatic<int>("getInt", contentResolver, SCREEN_BRIGHTNESS);
				brightness = (float)intBrightness / 255.0f;
			}

			callback(brightness);			
		}

		#endregion
	}
		
}
