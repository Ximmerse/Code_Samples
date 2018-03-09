using UnityEngine;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	/// <summary>
	/// Navigation controller.
	/// This class is only in charge of the top level menu, each menu will have it's own controller. 
	/// </summary>
	public class NavigationController : BaseController
	{
		public static bool IsWaitingForAnimationToComplete = false;

		public GameObject Settings;
		public GameObject JediKnight;
		public GameObject HolocronMaster;
		public GameObject Options;
		public GameObject Rules;
		public GameObject Debug;

		public JediProgressionController JediKnightController;
		public SettingsController SettingsController;
		public HolocronMasterController HolocronMasterController;
		public OptionsController OptionsController;
		public DebugMenuController DebugMenuController;

		private bool shown = false;

		public override void Setup(VisionSDK sdk, GazeWatcher gazeWatcher, Game game, MainNavigationController controller)
		{
			base.Setup(sdk, gazeWatcher, game, controller);

			Sdk.Input.OnButtonUp += OnButtonUp;

			Init();
		}

		public override void Init()
		{
			if (!shown)
			{
				CurrentElement = LastElement = null;
			}

			// Menu Buttons
			Settings.SetActive(shown && true);
			Options.SetActive(shown && Game != Game.ForceVision);
			HolocronMaster.SetActive(shown && Game == Game.ForceVision);

			// Setup Based on Game
			SettingsController.Init();
			HolocronMasterController.Init();
			OptionsController.Init();

			#if !RC_BUILD
			Debug.SetActive(shown);
			DebugMenuController.Init();
			#endif
		}

		protected override void OnEnable()
		{
			
		}

		protected override void OnDisable()
		{
			
		}

		protected override void OnDestroy()
		{
			if (Sdk != null && Sdk.Input != null)
			{
				Sdk.Input.OnButtonUp -= OnButtonUp;
			}
		}

		protected override void OnGazedAt(object sender, GazeEventArgs eventArguments)
		{
			base.OnGazedAt(sender, eventArguments);

			if (CurrentElement != null)
			{
				CurrentElement.OnGazedAt();
			}
		}

		protected override void OnGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (CurrentElement != null)
			{
				CurrentElement.OnGazedOff();
			}

			base.OnGazedOff(sender, eventArguments);
		}

		public void ToggleVisiblity(bool show)
		{
			shown = show;

			if (Controller.OnlyThingUsingGazeWatcher)
			{
				GazeWatcher.RaycastEnabled = shown;
			}

			if (shown && Controller.OnMenuShown != null)
			{
				Controller.OnMenuShown.Invoke(this, new EventArgs());
				AudioEvent.Play(AudioEventName.ProgressionUI.SettingsButton, gameObject);
			}

			if (!shown && Controller.OnMenuHidden != null)
			{
				Controller.OnMenuHidden.Invoke(this, new EventArgs());
			}

			Init();
		}

		public void ClearElement()
		{
			LastElement = CurrentElement = null;
		}

		public void Pause()
		{
			if (!shown && Game != Game.ForceVision)
			{
				OnButtonUp(this, new ButtonEventArgs(null, ButtonType.HmdMenu, ButtonEventType.Up));
			}
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (eventArguments.Button == ButtonType.HmdMenu)
			{
				if (IsWaitingForAnimationToComplete == true)
				{
					return;
				}

				CurrentElement = null;
				ToggleVisiblity(!shown);

				// Auto Show Holocron mastery
				if (shown && Game == Game.ForceVision)
				{
					HolocronMasterController.HolocronMasterButton.OnClicked();
					LastElement = HolocronMasterController.HolocronMasterButton;
				}
				else if (shown)
				{
					OptionsController.OptionsButton.OnClicked();
					LastElement = OptionsController.OptionsButton;
				}

				return;
			}

			if(!ButtonIsActivateOrControl(eventArguments.Button))
			{
				return;
			}

			if (CurrentElement != null && CurrentElement.Interactable)
			{
				if (LastElement != null && LastElement != CurrentElement)
				{
					LastElement.OnClicked();
				}

				CurrentElement.OnClicked();
				LastElement = (LastElement == CurrentElement) ? null : CurrentElement;
			}

			base.OnButtonUp(sender, eventArguments);
		}
	}
}