using System;
using Disney.ForceVision.Internal;

namespace Disney.ForceVision
{
	public class NativeSettings : INativeSettings
	{
		#region Private Fields

		private readonly INativeSettings settings;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeSettings"/> class.
		/// </summary>
		public NativeSettings()
		{
			#if UNITY_EDITOR
			settings = new NativeSettingsEditor();
			#elif UNITY_IPHONE
			settings = new NativeSettingsIos();
			#elif UNITY_ANDROID
			settings = new NativeSettingsAndroid();
			#endif
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the volume.
		/// </summary>
		/// <returns>The volume.</returns>
		public float GetVolume()
		{
			return settings.GetVolume();
		}

		/// <summary>
		/// Sets the volume.
		/// </summary>
		/// <param name="volume">Volume.</param>
		public void SetVolume(float volume)
		{
			settings.SetVolume(volume);
		}

		/// <summary>
		/// Determines whether this instance is muted.
		/// </summary>
		/// <returns><c>true</c> if this instance is muted; otherwise, <c>false</c>.</returns>
		public bool IsMuted()
		{
			return settings.IsMuted();
		}

		/// <summary>
		/// Gets the brightness.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void GetBrightness(Action<float> callback)
		{
			settings.GetBrightness(callback);
		}

		/// <summary>
		/// Sets the brightness.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		public void SetBrightness(float brightness)
		{
			settings.SetBrightness(brightness);
		}

		/// <summary>
		/// Gets the free space in kb.
		/// </summary>
		/// <returns>The free space.</returns>
		public long GetFreeSpace()
		{
			return settings.GetFreeSpace();
		}

		/// <summary>
		/// Updates the bluetooth status callback object.
		/// </summary>
		/// <returns>The free space.</returns>
		public void GetBluetoothState(string bluetoothCallbackObject)
		{
			settings.GetBluetoothState(bluetoothCallbackObject);
		}

		/// <summary>
		/// Gets the physical memory.
		/// </summary>
		/// <returns>The physical memory.</returns>
		public long GetPhysicalMemory()
		{
			return settings.GetPhysicalMemory();
		}

		/// <summary>
		/// Gets the available memory.
		/// </summary>
		/// <returns>The available memory.</returns>
		public long GetAvailableMemory()
		{
			return settings.GetAvailableMemory();
		}

		public float GetBatteryRemaining()
		{
			return settings.GetBatteryRemaining();
		}

		#if UNITY_ANDROID && !UNITY_EDITOR
		/// <summary>
		/// Gets if the system is low on memory.
		/// </summary>
		/// <returns><c>true</c>, if low memory warning was gotten, <c>false</c> otherwise.</returns>
		public bool GetLowMemory()
		{
			return settings.GetLowMemory();
		}

		/// <summary>
		/// Determines if the app[ is allowed to prompt for a permission.
		/// </summary>
		/// <returns><c>true</c> if this instance is permisson requestable; otherwise, <c>false</c>.</returns>
		public bool IsPermissonRequestable(string permission)
		{
			return settings.IsPermissonRequestable(permission);
		}
		#endif

		/// <summary>
		/// Determines whether a debugger is attached.
		/// </summary>
		/// <returns><c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.</returns>
		public bool IsDebuggerAttached()
		{
			return settings.IsDebuggerAttached();
		}

		#endregion

	}
}