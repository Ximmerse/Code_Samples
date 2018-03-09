using DG.Tweening;
using Disney.Vision;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public enum ScreenAnimationType
	{
		Enter,
		Exit
	}

	public class StereoSetupFtueController : SwipeHandler
	{
		private enum StereoSetupStep
		{
			None = -1,
			Welcome,
			UnmuteSound,
			PreparedToSync,
			SyncLightSaber,
			CalibrateLightSaber,
			PlaceBeacon,
			ConnectCable,
			RemoveTray,
			PhoneInTray,
			AlignPhone
		}

		#region Constants

		private const int BatteryLevelUnknown = -1;
		private const float MaxBrightness = 1f;
		private const float VolumeLevelStep = 0.1f;
		private const float VolumeBarMutedState = 0.4f;
		private const float VolumeBarUnmutedState = 1f;
		private const string EnterAnimationComplete = "StereoSetupFtueEnterAnimation";
		private const string RequiredHardwareShown = "RequiredHardwareShown";

		#endregion

		#region Properties

		public StereoSetupController SetupController;
		public TitleScreenController TitleController;
		public IssueDetectedController IssueController;
		public StereoToggleGroup MenuToggleGroup;
		public GameObject ExitPopup;
		public Image VolumeBar;
		public Image CalibrationBar;
		public SwitchController BluetoothSwitch;
		public SwitchController LightSaberSwitch;
		public SwitchController VolumeIconSwitch;
		public SwitchController VolumeTitleSwitch;
		public SwitchController VolumePromptSwitch;
		public SwitchController LightSaberSyncSwitch;
		public SwitchController StopperSwitch;
		public GameObject SaberNotFound;
		public Text LightSaberSyncCountdownText;
		public Text StopperPrompt;
		public BatteryLevels LightSaberBatteryLevel;
		public Transform Controller;
		public GameObject LeftButtons;
		public GameObject RightButtons;
		public GameObject UnsupportedPhoneButton;
		public GameObject UnsupportedPhonePopup;
		public GameObject AndroidPopup;
		public GameObject CloseButton;
		public GameObject CalibrationComplete;
		public GameObject NavIcons;
		public GameObject AndroidPermissionPrompt;
		public GameObject PrePromptAndroidPermission;
		public GameObject AndroidPermissionConfirmed;
		public GameObject AndroidPermissionDenied;
		public GameObject RequiredHardwareScreen;

		private StereoSetupStep currentStep
		{
			get
			{
				return (StereoSetupStep)CurrentStepInt;
			}

			set
			{
				CurrentStepInt = (int)value;
			}
		}

		private string defaultLightSaberSyncText;
		private StereoSetupStep farthestStep = StereoSetupStep.None;
		private bool isSearchingForLightSaber;
		private bool isLightSaberCalibrationInProgress;
		private int calibrationState = -1;
		private bool shouldDisplayAndroidPopup = true;
		private bool shouldDisplayLocServicesPopup = true;
		internal VisionSDK Sdk;

		#endregion

		#region MonoBehaviour

		private void Start()
		{
			// adding listeners for setup events
			StereoSetupEvents.OnBluetoothStateUpdate += OnBluetoothState;
			StereoSetupEvents.OnHmdConnected += OnHmdConnected;
			StereoSetupEvents.OnPeripheralConnected += OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected += OnPeripheralDisconnected;
			StereoSetupEvents.OnMutedStateUpdate += OnMutedState;
			StereoSetupEvents.OnVolumeChanged += OnVolumeChanged;
			StereoSetupEvents.OnLightSaberCalibration += OnLightSaberCalibration;

			// adding listener for toggle change event (which happens when a toggle is selected)
			StereoToggleGroup.OnToggleChange += OnToggleChangeHandler;

			// adding listener for countdown event
			Countdown.OnCountdownEvent += OnCountdownEventHandler;

			// adding listener for animation events
			GetComponent<AnimationEvents>().OnAnimationComplete += OnAnimationCompleteHandler;

			// getting volume level
			VolumeBar.fillAmount = SetupController.Container.NativeSettings.GetVolume();

			// setting default light saber sync countdown text
			defaultLightSaberSyncText = Localizer.Get(LightSaberSyncCountdownText.gameObject.GetComponent<LocalizedText>().Token);

			// updating the initial state of setup screens
			UpdateScreen(StereoSetupStep.Welcome);

			if (!JCSettingsManager.HasDeviceProfile)
			{
				UnsupportedPhonePopup.SetActive(true);
				UnsupportedPhoneButton.SetActive(true);
			}

			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
	
			// show required hardware screen the first time the player sees the FTUE
			if (!prefsStorage.PrefKeyExists(RequiredHardwareShown))
			{
				prefsStorage.SetPrefInt(RequiredHardwareShown, 1);
				RequiredHardwareScreen.SetActive(true);
			}
		}

		private void OnDestroy()
		{
			// removing listeners for stereo setup events
			StereoSetupEvents.OnBluetoothStateUpdate -= OnBluetoothState;
			StereoSetupEvents.OnHmdConnected -= OnHmdConnected;
			StereoSetupEvents.OnPeripheralConnected -= OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected -= OnPeripheralDisconnected;
			StereoSetupEvents.OnMutedStateUpdate -= OnMutedState;
			StereoSetupEvents.OnVolumeChanged -= OnVolumeChanged;
			StereoSetupEvents.OnLightSaberCalibration -= OnLightSaberCalibration;

			// removing listener for animation events
			GetComponent<AnimationEvents>().OnAnimationComplete -= OnAnimationCompleteHandler;

			// removing listener for toggle change event
			StereoToggleGroup.OnToggleChange -= OnToggleChangeHandler;

			// removing listener for countdown event
			Countdown.OnCountdownEvent -= OnCountdownEventHandler;
		}

		#endregion

		#region Class Methods

		protected override void DetectSwipe()
		{
			if (ExitPopup.activeSelf || IssueController.gameObject.activeSelf)
			{
				return;
			}

			base.DetectSwipe();
		}

		protected override void UpdateScreen(int step)
		{
			UpdateScreen((StereoSetupStep)step);
		}

		private void UpdateScreen(StereoSetupStep step, bool shouldAnimate = true)
		{
			// validating that is not the current step number
			if (step == currentStep || step < StereoSetupStep.Welcome || step > StereoSetupStep.AlignPhone)
			{
				return;
			}

			// setting the current step of setup
			currentStep = step;

			if (currentStep == StereoSetupStep.PreparedToSync)
			{
				#if UNITY_ANDROID && !UNITY_EDITOR
				//check for permissons, pip screen if needed
				AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
				object[] args = new object[1];
				args[0] = new string[] {"android.permission.ACCESS_COARSE_LOCATION"};
				bool isPermitted = permissions.CallStatic<bool>("CheckPermission", args);
				if (isPermitted == false && shouldDisplayLocServicesPopup)
				{
					AndroidPermissionPrompt.SetActive(true);
					PrePromptAndroidPermission.SetActive(true);
					AndroidPermissionConfirmed.SetActive(false);

					shouldDisplayLocServicesPopup = false;
				}
				#endif
			}

			// updating the farthest step flag
			if (currentStep > farthestStep)
			{
				farthestStep = currentStep;

				// enabling interactable
				MenuToggleGroup.SetToggleInteractable((int)farthestStep, true);

				// if there's a step after the current step, make its toggle interactable
				if ((currentStep + 1) <= StereoSetupStep.AlignPhone)
				{
					MenuToggleGroup.SetToggleInteractable((int)(currentStep + 1), true);
				}
			}

			// Show and hide UI elements
			ShowAndHideScreenElement(NavIcons, "NavIconsExit", StereoSetupStep.Welcome, StereoSetupStep.AlignPhone);
			ShowAndHideScreenElement(LeftButtons, "CornerButtons_Outro", StereoSetupStep.None, StereoSetupStep.AlignPhone);
			ShowAndHideScreenElement(RightButtons, "CornerButtons_Outro", StereoSetupStep.None, StereoSetupStep.AlignPhone);

			if (currentStep == StereoSetupStep.CalibrateLightSaber && !SetupController.IsPeripheralCalibrated)
			{
				if (SetupController.IsControllerConnected)
				{
					isLightSaberCalibrationInProgress = true;

					SetupController.Controller.StartCalibration(SetupController.OnCalibrationStateChanged);
				}
			}
			else if (currentStep == StereoSetupStep.CalibrateLightSaber && SetupController.IsPeripheralCalibrated)
			{
				CalibrationBar.fillAmount = 1;
				CalibrationComplete.SetActive(true);
			}
			else if (currentStep != StereoSetupStep.CalibrateLightSaber && isLightSaberCalibrationInProgress)
			{
				isLightSaberCalibrationInProgress = false;

				if (SetupController.IsControllerConnected)
				{
					SetupController.Controller.StopCalibrating();
				}
			}

			// setting stopper switch
			if (currentStep == StereoSetupStep.RemoveTray)
			{
				Device device = Sdk.Settings.CurrentDevice;
				if (device != null && device.StopperIn)
				{
					// updating prompt
					StopperPrompt.text = Localizer.Get(StereoSetupController.PositionTokenIn);

					// setting stopper switch to on
					StopperSwitch.SetState(SwitchState.On);
				}
				else
				{
					// updating prompt
					StopperPrompt.text = Localizer.Get(StereoSetupController.PositionTokenOut);

					// setting stopper switch to on
					StopperSwitch.SetState(SwitchState.Off);
				}
			}

			// displaying popup when placed in tray for Android
			if (Application.platform == RuntimePlatform.Android && currentStep == StereoSetupStep.PhoneInTray && shouldDisplayAndroidPopup)
			{
				AndroidPopup.SetActive(true);
				shouldDisplayAndroidPopup = false;
			}

			// setting toggle by passing the current step
			MenuToggleGroup.SetToggleByIndex((int)currentStep);

			// animating to the screen
			AnimateToScreen(shouldAnimate);
		}

		private void UpdateVolumeView(SwitchState state)
		{
			VolumeIconSwitch.SetState(state);
			VolumeTitleSwitch.SetState(state);
			VolumePromptSwitch.SetState(state);
		}

		/// <summary>
		/// Shows or hides an element on the screen based on what step in the FTUE the player is on
		/// </summary>
		private void ShowAndHideScreenElement(GameObject element,
		                                      string exitAnimation,
		                                      StereoSetupStep step1,
		                                      StereoSetupStep step2)
		{
			if (currentStep > step1 && currentStep < step2 && !element.activeSelf)
			{
				element.SetActive(true);
			}
			if ((currentStep == step1 || currentStep == step2) && element.activeSelf)
			{
				element.GetComponent<Animator>().Play(exitAnimation);
			}
		}

		#endregion

		#region Event Handlers

		protected void OnAnimationCompleteHandler(object sender, AnimationEventArgs eventArgs)
		{
			// hiding title screen
			TitleController.gameObject.SetActive(false);
		}

		/// <summary>
		/// Receiver for bluetooth status changes.
		/// </summary>
		/// <param name="state">State.</param>
		public void OnBluetoothState(bool state)
		{
			// setting bluetooth switch
			SwitchState switchState = state ? SwitchState.On : SwitchState.Off;
			BluetoothSwitch.SetState(switchState);
		}

		/// <summary>
		/// Handler when the close button is selected
		/// </summary>
		public void OnCloseButtonSelected()
		{
			ExitPopup.SetActive(true);
		}

		protected void OnCountdownEventHandler(object sender, CountdownEventArgs eventArgs)
		{
			string countdownText = defaultLightSaberSyncText.Replace("#Number#", eventArgs.TimeRemaining.ToString());
			LightSaberSyncCountdownText.text = countdownText;

			if (eventArgs.EventType == CountdownEventType.Finish)
			{
				// setting flag to indicate that search is complete
				isSearchingForLightSaber = false;

				DOVirtual.DelayedCall(1f, () =>
				{
					LightSaberSyncSwitch.SetState(SwitchState.On);
				});

				// playing sound
				AudioEvent.Play(AudioEventName.Ftue.Stereo.ConnectionNotFound, gameObject);
			}
		}

		/// <summary>
		/// Raises the enter headset mode event.
		/// </summary>
		public void OnEnterHeadsetMode()
		{
			KpiTracking.TrackSceneLoadTime();

			// TODO: mathh010 remove once enter headset mode button is removed
			// updating ftue data

			if (IssueController.TotalIssues > 0)
			{
				// checking for issues
				IssueController.gameObject.SetActive(true);
			}
			else
			{
				// playing sound
				AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);

				if (!FtueDataController.IsFtueComplete(FtueType.Setup))
				{
					FtueDataController.SetFtueComplete(FtueType.Setup);
				}

				// launching the appropriate game scene
				SetupController.LoadScene();
			}
		}

		/// <summary>
		/// Handler when the exit button is selected
		/// </summary>
		public void OnExitButtonSelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ExitButton, gameObject);

			// hide premission denied ui
			AndroidPermissionDenied.SetActive(false);

			// hiding the exit popup
			ExitPopup.SetActive(false);

			// going back to the title screen
			TitleController.gameObject.SetActive(true);

			// displaying welcome screen
			UpdateScreen(StereoSetupStep.Welcome, false);

			// resetting nav icons
			MenuToggleGroup.ResetToggles();

			// exiting, so farthest step is back to none
			farthestStep = StereoSetupStep.None;

			// hiding the nav icons
			NavIcons.SetActive(false);

			// hiding saber not found ui
			SaberNotFound.SetActive(false);

			// allow popups to show up if the player starts up FTUE again
			shouldDisplayAndroidPopup = true;
			shouldDisplayLocServicesPopup = true;
		}

		/// <summary>
		/// Handler when the help button is selected
		/// </summary>
		public void OnHelpSelected()
		{
			// TODO: mathh010 display help menu here
		}

		protected void OnHmdConnected()
		{
			// updating ftue data
			if (!FtueDataController.IsFtueComplete(FtueType.Setup))
			{
				FtueDataController.SetFtueComplete(FtueType.Setup);
			}

			// launching the game
			if (!AndroidPopup.activeSelf)
			{
				SetupController.LoadScene();
			}
		}

		protected void OnLightSaberCalibration(CalibrationState state)
		{
			if (state == CalibrationState.Complete && calibrationState == -1)
			{
				return;
			}

			// getting fill amount from calibration state
			float fillAmount = ((int)state + 1) * .25f;

			// setting calibration bar
			CalibrationBar.fillAmount = fillAmount;

			// displaying calibration complete
			CalibrationComplete.SetActive(state == CalibrationState.Complete);

			// playing sound
			if (state == CalibrationState.Complete && calibrationState != (int)state)
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.CalibrationSuccess, gameObject);
			}
			else
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.Calibration, gameObject);
			}

			// setting calibration state
			calibrationState = (int)state;

		}

		/// <summary>
		/// Raises the light saber sync selected event.
		/// </summary>
		public void OnLightSaberSyncSelected()
		{
			bool success = SetupController.StartPeripheralPairing();

			// starting the pairing process
			if (!success)
			{
				// transition = light saber searching view
				SaberNotFound.SetActive(true);
				LightSaberSyncSwitch.SetState(SwitchState.Transition);

				// setting flag to indicate searching
				isSearchingForLightSaber = true;
			}
			else
			{
				OnPeripheralConnected();
			}

			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.SearchSaber, gameObject);
		}

		protected void OnMutedState(bool isMuted)
		{
			VolumeIconSwitch.SetState(isMuted ? SwitchState.Off : SwitchState.On);
		}

		protected void OnPeripheralConnected()
		{
			// setting flag to indicate searching
			isSearchingForLightSaber = false;

			// turning light saber blue
			LightSaberSwitch.SetState(SwitchState.On);

			// off state = light saber synced view
			LightSaberSyncSwitch.SetState(SwitchState.Off);

			// updating battery levels data
			int batteryLevel = SetupController.Controller != null ? SetupController.Controller.GetBatteryLevel() : 0;
			// TODO: mathh010 update from false to proper state of controller being charged or not
			LightSaberBatteryLevel.SetBatteryLevel(batteryLevel, false);

			if (SetupController.Container.PlayingDarkSide)
			{
				if (SetupController.Controller.GetModelName() != SetupController.Container.KyloSaberName)
				{
					SetupController.Container.PlayingDarkSideWithWrongSaber = true;
					Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
					Debug.Log("Playing Dark Side with wrong saber!");
				}
			}
		}

		protected void OnPeripheralDisconnected()
		{
			if (!isSearchingForLightSaber)
			{
				// turning light saber white
				LightSaberSwitch.SetState(SwitchState.Off);

				// on state = light saber not synced view
				LightSaberSyncSwitch.SetState(SwitchState.On);
			}
		}

		protected void OnToggleChangeHandler(object sender, ToggleEventArgs eventArgs)
		{
			StereoSetupStep step = (StereoSetupStep)eventArgs.SelectedToggle.transform.GetSiblingIndex();

			UpdateScreen(step);
		}

		protected void OnVolumeChanged(float volumeLevel)
		{
			VolumeBar.fillAmount = volumeLevel;

			if (volumeLevel <= 0)
			{
				UpdateVolumeView(SwitchState.Off);
			}
			else
			{
				UpdateVolumeView(SwitchState.On);
			}
		}

		#endregion
	}
}