using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class InterstitialEvents : MonoBehaviour
	{
		public ArchivistInterstitialController Controller;

		public void NextAnimation()
		{
			/*
			if (ArchivistInterstitialController.Triggers.Count <= 0)
			{
				Controller.Exit();
				return;
			}

			// Play the next one
			Controller.PlayAnimation(ArchivistInterstitialController.Triggers[0]);
			ArchivistInterstitialController.Triggers.RemoveAt(0);
			*/
		}

		public void PlaySound(string token)
		{
			AudioEvent.Play(token, gameObject);
		}

		public void PlayRigAnimation(string animation)
		{
			Controller.PlayAnimation(animation, true);
		}

		public void TriggerRigAnimation(string trigger)
		{
			Controller.TriggerAnimation(trigger);
		}

		public void PlayAnimation(string animation)
		{
			Controller.PlayAnimation(animation);
		}
	}
}