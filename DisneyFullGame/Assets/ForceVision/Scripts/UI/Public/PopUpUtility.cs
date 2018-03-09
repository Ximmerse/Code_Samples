using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	/// <summary>
	/// A class of helpful functions for popup ui
	/// </summary>
	public class PopUpUtility : MonoBehaviour
	{
		public void PlayIntroSound()
		{
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ZoomIn, gameObject);
		}
	}
}
