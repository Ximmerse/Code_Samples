using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class SetAndroidBrightnessRunnable : NativeAndroid
	{
		#region Private Fields

		private float brightness;

		#endregion

		#region Constructor

		public SetAndroidBrightnessRunnable(float brightness)
		{
			this.brightness = brightness;
			AndroidJavaObject activity = GetCurrentAndroidActivity();
			activity.Call("runOnUiThread", new AndroidJavaRunnable(RunOnUIThread));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Set Android Layout brightness
		/// </summary>
		private void RunOnUIThread()
		{
			AndroidJavaObject window = GetAndroidWindow();
			AndroidJavaObject layout = window.Call<AndroidJavaObject>("getAttributes");

			layout.Set<float>("screenBrightness", brightness);
			window.Call("setAttributes", layout);			
		}

		#endregion
	}
}