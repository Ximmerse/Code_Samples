using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class AudioEvent
	{
		#region Constants

		public const string AN = "k6Cj";

		#endregion

		#region Properties

		private static List<string> playedAudio;
		private static ContainerAPI container;
		private const string PlayOnceKey = "AudioPlayOnceEverEvent_";

		#endregion

		#region Class Methods

		public static bool Play(string eventName, GameObject source)
		{
			uint result = AkSoundEngine.PostEvent(eventName, source);

			bool success = result != 0;

			return success;
		}

		public static void Play(string eventName, GameObject source, float delay)
		{
			DOVirtual.DelayedCall(delay, () => {
				Play(eventName, source);
			});
		}

		public static void Play(string eventName, GameObject source, AkCallbackManager.EventCallback callback)
		{
			AkSoundEngine.PostEvent(eventName, source, (uint)AkCallbackType.AK_EndOfEvent, callback, eventName);
		}

		public static void Play(string eventName, GameObject source, AkCallbackManager.EventCallback callback, float delay = 0f)
		{
			DOVirtual.DelayedCall(delay, () => {
				AkSoundEngine.PostEvent(eventName, source, (uint)AkCallbackType.AK_EndOfEvent, callback, eventName);
			});
		}

		public static bool PlayOnce(string eventName, GameObject source)
		{
			bool success = false;

			if (playedAudio == null)
			{
				playedAudio = new List<string>();
			}

			if (!playedAudio.Contains(eventName))
			{
				playedAudio.Add(eventName);
				success = Play(eventName, source);
			}

			return success;
		}

		public static bool PlayOnceEver(string eventName, GameObject source)
		{
			if (container == null)
			{
				container = new ContainerAPI(Game.ForceVision);
			}

			string key = PlayOnceKey + eventName;

			if (container.PlayerPrefs.PrefKeyExists(key))
			{
				return false;
			}

			container.PlayerPrefs.SetPrefInt(key, 1);

			return Play(eventName, source);
		}

		public static void Stop(string eventName, GameObject source)
		{
			AkSoundEngine.StopAll(source);
		}

		public static void Mute(GameObject source)
		{
			AkSoundEngine.StopAll();
		}

		#endregion
	}
}