using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class StereoMenuController : MonoBehaviour
	{
		#region Event Handlers

		public void OnMenuButtonSelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.SubmenuButton, gameObject);
		}

		#endregion
	}
}