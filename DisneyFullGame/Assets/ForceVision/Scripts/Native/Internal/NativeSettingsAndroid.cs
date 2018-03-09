#if UNITY_ANDROID

using System;
using Disney.ForceVision;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class NativeSettingsAndroid : NativeAndroid, INativeSettings
	{
		
		#region Public Methods

		/// <summary>
		/// Gets the volume.
		/// </summary>
		/// <returns>The volume.</returns>
		public float GetVolume()
		{
			AndroidJavaObject audioManager = GetAndroidAudioManager();

			int STREAM_MUSIC = audioManager.GetStatic<int>("STREAM_MUSIC");
			int maxVolume = audioManager.Call<int>("getStreamMaxVolume", STREAM_MUSIC);
			int currentVolume = audioManager.Call<int>("getStreamVolume", STREAM_MUSIC);

			return (float)currentVolume / (float)maxVolume;
		}

		/// <summary>
		/// Sets the volume.
		/// </summary>
		/// <param name="volume">Volume.</param>
		public void SetVolume(float volume)
		{
			AndroidJavaObject audioManager = GetAndroidAudioManager();

			int STREAM_MUSIC = audioManager.GetStatic<int>("STREAM_MUSIC");
			int maxVolume = audioManager.Call<int>("getStreamMaxVolume", STREAM_MUSIC);
			int normalizedVolume = (int)((float)maxVolume * volume);

			audioManager.Call("setStreamVolume", STREAM_MUSIC, normalizedVolume, 0);
		}

		/// <summary>
		/// Determines whether this instance is muted.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		public bool IsMuted()
		{
			AndroidJavaObject audioManager = GetAndroidAudioManager();

			int RINGER_MODE_NORMAL = audioManager.GetStatic<int>("RINGER_MODE_NORMAL");
			int ringerMode = audioManager.Call<int>("getRingerMode");

			if (ringerMode == RINGER_MODE_NORMAL)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the brightness.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void GetBrightness(Action<float> callback)
		{
			new GetAndroidBrightnessRunnable(callback);
		}

		/// <summary>
		/// Sets the brightness.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		public void SetBrightness(float brightness)
		{
			new SetAndroidBrightnessRunnable(brightness);
		}

		/// <summary>
		/// Gets the free space in kb.
		/// </summary>
		/// <returns>The free space.</returns>
		public long GetFreeSpace()
		{
			AndroidJavaObject statFs = new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath);
			return (statFs.Call<int>("getBlockSize") * statFs.Call<int>("getAvailableBlocks")) / 1024;
		}

		/// <summary>
		/// Gets if the system is low on memory.
		/// </summary>
		/// <returns><c>true</c>, if low memory warning was gotten, <c>false</c> otherwise.</returns>
		public bool GetLowMemory()
		{
			AndroidJavaObject activityManager = GetAndroidActivityManager();
			AndroidJavaObject info = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
			activityManager.Call("getMemoryInfo", info);

			return info.Get<bool>("lowMemory");
		}

		/// <summary>
		/// Updates the bluetooth status callback object.
		/// </summary>
		/// <returns>The free space.</returns>
		public void GetBluetoothState(string bluetoothCallbackObject)
		{
			AndroidJavaClass bluetoothAdapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
			AndroidJavaObject bluetoothAdapter = bluetoothAdapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
			AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

			int bluetoothState = bluetoothAdapter.Call<int>("getState");
			string blueToothStateString = "0";

			switch (bluetoothState)
			{
				case 12:
					blueToothStateString = "ON";
					break;

				case 10:
					blueToothStateString = "OFF";
					break;

				default:
					blueToothStateString = "OFF";
					break;
			}

			unityPlayerClass.CallStatic("UnitySendMessage", bluetoothCallbackObject, "OnBluetoothState", blueToothStateString);
		}

		/// <summary>
		/// Gets the physical memory.
		/// </summary>
		/// <returns>The physical memory.</returns>
		public long GetPhysicalMemory()
		{
			AndroidJavaObject activityManager = GetAndroidActivityManager();
			AndroidJavaObject info = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
			activityManager.Call("getMemoryInfo", info);
			return info.Get<long>("totalMem");
		}

		/// <summary>
		/// Gets the available memory.
		/// </summary>
		/// <returns>The available memory.</returns>
		public long GetAvailableMemory()
		{
			AndroidJavaObject activityManager = GetAndroidActivityManager();
			AndroidJavaObject info = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
			activityManager.Call("getMemoryInfo", info);
			return info.Get<long>("availMem");
		}

		/// <summary>
		/// Gets the battery remaining.
		/// </summary>
		/// <returns>The battery remaining.</returns>
		public float GetBatteryRemaining()
		{
			AndroidJavaObject currentActivity = GetCurrentAndroidActivity();
			AndroidJavaObject intentFilter = new AndroidJavaObject("android.content.IntentFilter", new object[]{ "android.intent.action.BATTERY_CHANGED" });
			AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("registerReceiver", new object[]{ null, intentFilter });
			int level = intent.Call<int>("getIntExtra", new object[]{"level", -1});
			int scale = intent.Call<int>("getIntExtra", new object[]{"scale", -1});
			return ((float)level / (float)scale); 
		}

		/// <summary>
		/// Determines whether a debugger is attached.
		/// </summary>
		/// <returns><c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.</returns>
		public bool IsDebuggerAttached()
		{
			AndroidJavaObject debug = new AndroidJavaObject("android.os.Debug");
			return debug.CallStatic<bool>("isDebuggerConnected", new object[0]);
		}

		/// <summary>
		/// Sets the sustained performance mode.
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		public void SetSustainedPerformanceMode(bool enabled)
		{
			AndroidJavaObject androidActivity = GetCurrentAndroidActivity();
			AndroidJavaObject androidWindow = GetAndroidWindow();

			// The sim thread in Unity is single-threaded, so we don't need to lock when accessing
			// or assigning androidWindow.
			androidActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
			{
				androidWindow.Call("setSustainedPerformanceMode", enabled);
				Log.Debug("Set sustained performance mode: " + (enabled ? "ON" : "OFF"));
			})
			);
		}

		public bool IsPermissonRequestable(string permission)
		{
			AndroidJavaObject androidActivity = GetCurrentAndroidActivity();
			return androidActivity.Call<bool>("shouldShowRequestPermissionRationale", permission);
		}
		#endregion
	}
}

#endif