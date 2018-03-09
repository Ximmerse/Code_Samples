using System;
using UnityEngine;
using Disney.ForceVision.Internal;

namespace Disney.ForceVision
{
	public class NativeBridge : MonoBehaviour 
	{
		#region Public Events

		/// <summary>
		/// Event for listening for the low memory warning.
		/// </summary>
		/// <value>The on low memory.</value>
		public EventHandler<EventArgs> OnLowMemory { get; set; }

		#endregion

		#region Private Properties

		#if UNITY_ANDROID && !UNITY_EDITOR

		private INativeSettings settings;

		#endif

		#endregion

		#region Public Methods

		/// <summary>
		/// Destroy this instance.
		/// </summary>
		public void Destroy()
		{
			// Kill the polling
			CancelInvoke();

			// Clear events
			OnLowMemory = null;
		}

		#if UNITY_ANDROID

		/// <summary>
		/// Starts the low memory poll for Android.
		/// </summary>
		/// <param name="settings">Settings.</param>
		public void StartLowMemoryPoll(INativeSettings settings)
		{
			#if UNITY_ANDROID && !UNITY_EDITOR

			this.settings = settings;

			#endif

			InvokeRepeating("AndroidLowMemoryPoll", 1.0f, 10.0f);
		}

		#endif

		#endregion

		#region Private Methods

		private void ReceivedMemoryWarning(string empty)
		{
			if (OnLowMemory != null)
			{
				OnLowMemory(this, new EventArgs());
			}
		}

		#if UNITY_ANDROID && !UNITY_EDITOR

		private void AndroidLowMemoryPoll()
		{
			if (settings.GetLowMemory() && OnLowMemory != null)
			{
				OnLowMemory(this, new EventArgs());
			}
		}

		#endif

		#endregion
	}
}