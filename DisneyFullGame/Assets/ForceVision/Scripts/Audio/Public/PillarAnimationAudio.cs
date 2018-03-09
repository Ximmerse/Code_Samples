using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class PillarAnimationAudio : MonoBehaviour 
	{		
		private void PlayAnimAudio(string eventName)
		{
			AudioEvent.Play(eventName, gameObject);
		}

		private void PlayAMAudio(string eventName)
		{
			string characterAvatar = GetComponent<Animator>().avatar.name;
			AudioEvent.Play(eventName + characterAvatar, gameObject);
		}

	}
}
