using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class NativeAndroid
	{

		#region Protected Methods

		/// <summary>
		/// Gets the current android activity.
		/// </summary>
		/// <returns>The current android activity.</returns>
		protected AndroidJavaObject GetCurrentAndroidActivity()
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		}

		/// <summary>
		/// Gets the android audio manager.
		/// </summary>
		/// <returns>The android audio manager.</returns>
		protected AndroidJavaObject GetAndroidAudioManager()
		{
			AndroidJavaObject currentActivity = GetCurrentAndroidActivity();
			//using the constant to get the static string value breaks for dev builds.
			//string AUDIO_SERVICE = currentActivity.GetStatic<string>("AUDIO_SERVICE");
			return currentActivity.Call<AndroidJavaObject>("getSystemService", "audio");
		}

		/// <summary>
		/// Gets the android activity manager.
		/// </summary>
		/// <returns>The android activity manager.</returns>
		protected AndroidJavaObject GetAndroidActivityManager()
		{
			AndroidJavaObject currentActivity = GetCurrentAndroidActivity();
			//using the constant to get the static string value breaks for dev builds.
			//string ACTIVITY_SERVICE = currentActivity.GetStatic<string>("ACTIVITY_SERVICE");
			return currentActivity.Call<AndroidJavaObject>("getSystemService", "activity");
		}

		/// <summary>
		/// Gets the android window.
		/// </summary>
		/// <returns>The android window.</returns>
		protected AndroidJavaObject GetAndroidWindow()
		{
			AndroidJavaObject currentActivity = GetCurrentAndroidActivity();
			return currentActivity.Call<AndroidJavaObject>("getWindow");
		}

		#endregion

	}
}