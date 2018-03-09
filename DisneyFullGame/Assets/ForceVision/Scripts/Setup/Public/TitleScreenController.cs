using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class TitleScreenEvents
	{
		public static EventHandler OnMenuSelected;
	}

	public class TitleScreenController : MonoBehaviour
	{
		#region Properties

		public StereoSetupFtueController FtueSetup;
		public StereoSetupNonFtueController NonFtueSetup;

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the tap to continue selected event.
		/// </summary>
		public void OnTapToContinueSelected()
		{
			// turning on either stereo setup FTUE or non FTUE
			if (FtueDataController.IsFtueComplete(FtueType.Setup))
			{
				// showing non ftue screen
				NonFtueSetup.gameObject.SetActive(true);
				NonFtueSetup.GetComponent<Animator>().Play(StereoSetupController.EnterAnimationClip);

				// hiding ftue screen
				FtueSetup.gameObject.SetActive(false);
			}
			else
			{
				// hiding non ftue screen
				NonFtueSetup.gameObject.SetActive(false);

				// showing ftue screen
				FtueSetup.gameObject.SetActive(true);
				FtueSetup.GetComponent<Animator>().Play(StereoSetupController.EnterAnimationClip);
			}

			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.MenuStart, gameObject);
		}

		/// <summary>
		/// Handles the menu button selected event.
		/// </summary>
		public void OnMenuButtonSelected()
		{
			// TODO: mathh010 display menu here

			if (TitleScreenEvents.OnMenuSelected != null)
			{
				TitleScreenEvents.OnMenuSelected(this, new EventArgs());
			}
		}

		/// <summary>
		/// Handles the announcement button selected event.
		/// </summary>
		public void OnAnnouncementButtonSelected()
		{
			// TODO: mathh010 display announcements here
		}

		#endregion
	}
}