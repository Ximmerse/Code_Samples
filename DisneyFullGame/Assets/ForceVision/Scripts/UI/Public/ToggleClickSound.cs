using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Toggle))]

	public class ToggleClickSound : MonoBehaviour
	{
		private Toggle toggle;

		void Start()
		{
			toggle = GetComponent<Toggle>();
		}

		/// <summary>
		/// Plays the correct sound depending on whether the toggle is turned on or off.
		/// </summary>
		public void PlaySound()
		{
			if (!toggle.interactable)
			{
				return;
			}

			if (toggle.isOn)
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.PlusButton, gameObject);
			}
			else
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.MinusButton, gameObject);
			}
		}
	}
}
