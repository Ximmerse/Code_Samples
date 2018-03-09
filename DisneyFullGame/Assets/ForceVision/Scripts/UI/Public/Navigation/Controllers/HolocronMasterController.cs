using UnityEngine;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	public class HolocronMasterController : BaseController
	{
		public ToggleElement HolocronMasterButton;

		protected override void OnEnable()
		{
			base.OnEnable();

			if (!AudioEvent.PlayOnceEver(AudioEventName.Menu.HolocronMasteryFirstTime, gameObject))
			{
				AudioEvent.Play(AudioEventName.Menu.OpenHolocronMaster, gameObject);
			}
			AudioEvent.Play(AudioEventName.ProgressionUI.HolocronMasteryButton, gameObject);
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (eventArguments.Button != ButtonType.SaberActivate && eventArguments.Button != ButtonType.SaberControl && eventArguments.Button != ButtonType.HmdSelect)
			{
				return;
			}

			if (CurrentElement != null && CurrentElement.Interactable)
			{
				CurrentElement.OnClicked();
			}

			base.OnButtonUp(sender, eventArguments);
		}

		protected override void OnGazedAt(object sender, GazeEventArgs eventArguments)
		{
			if (CurrentElement != null && eventArguments.Hit.IsChildOf(transform))
			{
				CurrentElement.OnGazedOff();
			}

			base.OnGazedAt(sender, eventArguments);

			if (CurrentElement != null)
			{
				CurrentElement.OnGazedAt();
			}
		}

		protected override void OnGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (eventArguments.Hit.IsChildOf(transform) && CurrentElement != null)
			{
				CurrentElement.OnGazedOff();
			}

			base.OnGazedOff(sender, eventArguments);
		}
	}
}