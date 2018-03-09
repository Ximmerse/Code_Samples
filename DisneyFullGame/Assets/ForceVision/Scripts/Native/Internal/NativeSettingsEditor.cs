#if UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using Disney.ForceVision;

namespace Disney.ForceVision.Internal
{
	public class NativeSettingsEditor : INativeSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.NativeSettingsEditor"/> class.
		/// </summary>
		public NativeSettingsEditor()
		{
		}

		/// <summary>
		/// Gets the volume.
		/// </summary>
		/// <returns>The volume.</returns>
		public float GetVolume()
		{
			return 1.0f;
		}

		/// <summary>
		/// Sets the volume.
		/// </summary>
		/// <param name="volume">Volume.</param>
		public void SetVolume(float volume)
		{
			
		}

		/// <summary>
		/// Determines whether this instance is muted the specified volume.
		/// </summary>
		/// <returns><c>true</c> if this instance is muted the specified volume; otherwise, <c>false</c>.</returns>
		/// <param name="volume">Volume.</param>
		public bool IsMuted()
		{
			return false;
		}

		/// <summary>
		/// Gets the brightness.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void GetBrightness(Action<float> callback)
		{
			callback(1.0f);
		}

		/// <summary>
		/// Sets the brightness.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		public void SetBrightness(float brightness)
		{
			
		}

		/// <summary>
		/// Gets the free space in kb.
		/// </summary>
		/// <returns>The free space.</returns>
		public long GetFreeSpace()
		{
			return 1024;
		}

		/// <summary>
		/// Updates the bluetooth status callback object.
		/// </summary>
		/// <returns>The free space.</returns>
		public void GetBluetoothState(string bluetoothCallbackObject)
		{

		}
			
		/// <summary>
		/// Gets the physical memory.
		/// </summary>
		/// <returns>The physical memory.</returns>
		public long GetPhysicalMemory()
		{
			return 9999999999;
		}

		/// <summary>
		/// Gets the available memory.
		/// </summary>
		/// <returns>The available memory.</returns>
		public long GetAvailableMemory()
		{
			return 9999999999;
		}

		/// <summary>
		/// Gets the battery remaining.
		/// </summary>
		/// <returns>The battery remaining.</returns>
		public float GetBatteryRemaining()
		{
			return 0.5f;
		}

		/// <summary>
		/// Determines whether a debugger is attached.
		/// </summary>
		/// <returns><c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.</returns>
		public bool IsDebuggerAttached()
		{
			return true;
		}
	}
}

#endif