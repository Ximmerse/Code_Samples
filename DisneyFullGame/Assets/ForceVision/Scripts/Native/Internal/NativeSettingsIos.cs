#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using Disney.ForceVision;

namespace Disney.ForceVision.Internal
{
	public class NativeSettingsIos : INativeSettings
	{
		/// <summary>
		/// Native calls.
		/// </summary>
		private class NativeCalls
		{
			[DllImport("__Internal")]
			public static extern float GetVolume();

			[DllImport("__Internal")]
			public static extern void SetVolume(float volume);

			[DllImport("__Internal")]
			public static extern bool IsMuted();

			[DllImport("__Internal")]
			public static extern float GetBrightness();

			[DllImport("__Internal")]
			public static extern void SetBrightness(float brightness);

			[DllImport("__Internal")]
			public static extern long GetFreeSpace();

			[DllImport ("__Internal")]
			public static extern void GetBluetoothState(string bluetoothCallbackObject);

			[DllImport("__Internal")]
			public static extern long GetPhysicalMemory();

			[DllImport("__Internal")]
			public static extern long GetAvailableMemory();

			[DllImport("__Internal")]
			public static extern float GetBatteryRemaining();

			[DllImport("__Internal")]
			public static extern bool IsDebuggerAttached();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.NativeSettingsIos"/> class.
		/// </summary>
		public NativeSettingsIos()
		{
		}

		/// <summary>
		/// Gets the volume.
		/// </summary>
		/// <returns>The volume.</returns>
		public float GetVolume()
		{
			return NativeCalls.GetVolume();
		}

		/// <summary>
		/// Sets the volume.
		/// </summary>
		/// <param name="volume">Volume.</param>
		public void SetVolume(float volume)
		{
			NativeCalls.SetVolume(volume);
		}

		/// <summary>
		/// Determines whether this instance is muted the specified volume.
		/// </summary>
		/// <returns><c>true</c> if this instance is muted the specified volume; otherwise, <c>false</c>.</returns>
		/// <param name="volume">Volume.</param>
		public bool IsMuted()
		{
			return NativeCalls.IsMuted();
		}

		/// <summary>
		/// Gets the brightness.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void GetBrightness(Action<float> callback)
		{
			callback(NativeCalls.GetBrightness());
		}

		/// <summary>
		/// Sets the brightness.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		public void SetBrightness(float brightness)
		{
			NativeCalls.SetBrightness(brightness);
		}

		/// <summary>
		/// Gets the free space in kb.
		/// </summary>
		/// <returns>The free space.</returns>
		public long GetFreeSpace()
		{
			return NativeCalls.GetFreeSpace() / 1024;
		}

		/// <summary>
		/// Updates the bluetooth status callback object.
		/// </summary>
		/// <returns>The free space.</returns>
		public void GetBluetoothState(string bluetoothCallbackObject)
		{
			NativeCalls.GetBluetoothState(bluetoothCallbackObject);;
		}

		/// <summary>
		/// Gets the physical memory.
		/// </summary>
		/// <returns>The physical memory.</returns>
		public long GetPhysicalMemory()
		{
			return NativeCalls.GetPhysicalMemory();
		}

		/// <summary>
		/// Gets the available memory.
		/// </summary>
		/// <returns>The available memory.</returns>
		public long GetAvailableMemory()
		{
			return NativeCalls.GetAvailableMemory();
		}

		/// <summary>
		/// Gets the battery remaining.
		/// </summary>
		/// <returns>The battery remaining.</returns>
		public float GetBatteryRemaining()
		{
			return NativeCalls.GetBatteryRemaining();
		}

		/// <summary>
		/// Determines whether a debugger is attached.
		/// </summary>
		/// <returns><c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.</returns>
		public bool IsDebuggerAttached()
		{
			return NativeCalls.IsDebuggerAttached();
		}
	}
}

#endif