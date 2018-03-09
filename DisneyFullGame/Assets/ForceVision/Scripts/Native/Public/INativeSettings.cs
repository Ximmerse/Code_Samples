using System;

namespace Disney.ForceVision
{
	public interface INativeSettings
	{
		#region Generic Settings

		/// <summary>
		/// Gets the volume.
		/// </summary>
		/// <returns>The volume.</returns>
		float GetVolume();

		/// <summary>
		/// Sets the volume.
		/// </summary>
		/// <param name="volume">Volume.</param>
		void SetVolume(float volume);

		/// <summary>
		/// Determines whether this instance is muted.
		/// </summary>
		/// <returns><c>true</c> if this instance is muted; otherwise, <c>false</c>.</returns>
		bool IsMuted();

		/// <summary>
		/// Gets the brightness.
		/// </summary>
		/// <param name="callback">Callback.</param>
		void GetBrightness(Action<float> callback);

		/// <summary>
		/// Sets the brightness.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		void SetBrightness(float brightness);

		/// <summary>
		/// Gets the free space in kb.
		/// </summary>
		/// <returns>The free space.</returns>
		long GetFreeSpace();

		/// <summary>
		/// Updates the bluetooth status callback object.
		/// </summary>
		/// <returns>The free space.</returns>
		void GetBluetoothState(string bluetoothCallbackObject);

		/// <summary>
		/// Gets the physical memory.
		/// </summary>
		/// <returns>The physical memory.</returns>
		long GetPhysicalMemory();

		/// <summary>
		/// Gets the available memory.
		/// </summary>
		/// <returns>The available memory.</returns>
		long GetAvailableMemory();

		/// <summary>
		/// Gets the battery remaining.
		/// </summary>
		/// <returns>The battery remaining.</returns>
		float GetBatteryRemaining();

		#if UNITY_ANDROID && !UNITY_EDITOR
		/// <summary>
		/// Gets if the system is low on memory.
		/// </summary>
		/// <returns><c>true</c>, if low memory warning was gotten, <c>false</c> otherwise.</returns>
		bool GetLowMemory();

		/// <summary>
		/// Determines if the app[ is allowed to prompt for a permission.
		/// </summary>
		/// <returns><c>true</c> if this instance is permisson requestable; otherwise, <c>false</c>.</returns>
		bool IsPermissonRequestable(string permission);
		#endif

		/// <summary>
		/// Determines whether a debugger is attached.
		/// </summary>
		/// <returns><c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.</returns>
		bool IsDebuggerAttached();

		#endregion
	}
}