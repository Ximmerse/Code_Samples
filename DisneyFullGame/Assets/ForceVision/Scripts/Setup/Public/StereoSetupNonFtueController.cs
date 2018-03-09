using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;

namespace Disney.ForceVision
{
	public class StereoSetupNonFtueController : SwipeHandler
	{
		public enum ReturnSetupStep
		{
			None = -1,
			Reminders,
			AlignPhone
		}

		#region Constants

		private const string EnterAnimationComplete = "StereoSetupNonFtueEnterAnimation";

		#endregion

		#region Properties

		public StereoSetupController SetupController;
		public TitleScreenController TitleController;
		public IssueDetectedController IssueController;
		public SwitchController BluetoothStatusSwitch;
		public SwitchController BluetoothDetailsSwitch;
		public SwitchController LightSaberStatusSwitch;
		public SwitchController LightSaberSyncSwitch;
		public SwitchController LightSaberSwitch;
		public SwitchController CalibrationStatusSwitch;
		public SwitchController VolumeStatusSwitch;
		public SwitchController VolumeDetailsHeaderSwitch;
		public SwitchController VolumeDetailsPromptSwitch;
		public SwitchController VolumeDetailsIconSwitch;
		public SwitchController StopperSwitch;
		public BatteryLevels LightSaberDetailsBattery;
		public GameObject SaberNotFound;
		public Text LightSaberSyncCountdownText;
		public Text StopperPrompt;
		public Image VolumeBar;
		public Image CalibrationBar;
		public GameObject CalibrationComplete;
		public GameObject UnsupportedPhoneButton;
		public StereoToggleGroup MenuToggleGroup;
		public GameObject LeftButtons;
		public GameObject RightButtons;

		private ReturnSetupStep currentStep
		{
			get
			{
				return (ReturnSetupStep)CurrentStepInt;
			}	
			set
			{
				CurrentStepInt = (int)value;
			}
		}

		private ReturnSetupStep farthestStep = ReturnSetupStep.None;
		private string defaultLightSaberSyncText;
		private bool isSearchingForLightSaber;
		private bool isLightSaberCalibrationInProgress;
		private int calibrationState = -1;
		private bool setupScreenActive = false;
		internal VisionSDK Sdk;

		#endregion

		#region MonoBehaviour

		protected void Start()
		{
			// adding listeners for stereo setup events
			StereoSetupEvents.OnBluetoothStateUpdate += OnBluetoothState;
			StereoSetupEvents.OnHmdConnected += OnHmdConnected;
			StereoSetupEvents.OnPeripheralConnected += OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected += OnPeripheralDisconnected;
			StereoSetupEvents.OnMutedStateUpdate += OnMutedState;
			StereoSetupEvents.OnVolumeChanged += OnVolumeChanged;
			StereoSetupEvents.OnLightSaberCalibration += OnLightSaberCalibration;

			// adding listener for toggle change event (which happens when a toggle is selected)
			StereoToggleGroup.OnToggleChange += OnToggleChangeHandler;

			// adding listener for animation events
			GetComponent<AnimationEvents>().OnAnimationComplete += OnAnimationCompleteHandler;

			// adding listener for countdown event
			Countdown.OnCountdownEvent += OnCountdownEventHandler;

			// setting default light saber sync countdown text
			defaultLightSaberSyncText = Localizer.Get(LightSaberSyncCountdownText.gameObject.GetComponent<LocalizedText>().Token);

			// setting view for remove tray step
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

			// If device unsupported show button that leads to relevant message
			if (!JCSettingsManager.HasDeviceProfile)
			{
				UnsupportedPhoneButton.SetActive(true);
			}

			UpdateScreen(ReturnSetupStep.Reminders);
		}

		private void OnDestroy()
		{
			// adding listeners for stereo setup events
			StereoSetupEvents.OnBluetoothStateUpdate -= OnBluetoothState;
			StereoSetupEvents.OnHmdConnected -= OnHmdConnected;
			StereoSetupEvents.OnPeripheralConnected -= OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected -= OnPeripheralDisconnected;
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

		public void Enter()
		{
			Animator animator = GetComponent<Animator>();
			animator.Play(StereoSetupController.EnterAnimationClip);
		}

		protected override void DetectSwipe()
		{
			if (setupScreenActive || IssueController.gameObject.activeSelf)
			{
				return;
			}

			base.DetectSwipe();
		}

		protected override void UpdateScreen(int step)
		{
			UpdateScreen((ReturnSetupStep)step);
		}

		private void UpdateScreen(ReturnSetupStep step, bool shouldAnimate = true)
		{
			// validating that is not the current step number
			if (step == currentStep || step < ReturnSetupStep.Reminders || step > ReturnSetupStep.AlignPhone)
			{
				return;
			}

			// setting the current step of setup
			currentStep = step;

			// updating the farthest step flag
			if (currentStep > farthestStep)
			{
				farthestStep = currentStep;

				// enabling interactable
				MenuToggleGroup.SetToggleInteractable((int)farthestStep, true);

				// if there's a step after the current step, make its toggle interactable
				if ((currentStep + 1) <= ReturnSetupStep.AlignPhone)
				{
					MenuToggleGroup.SetToggleInteractable((int)(currentStep + 1), true);
				}
			}

			// Show and hide UI elements
			ShowAndHideScreenElement(LeftButtons, "CornerButtons_Outro");
			ShowAndHideScreenElement(RightButtons, "CornerButtons_Outro");

			// setting toggle by passing the current step
			MenuToggleGroup.SetToggleByIndex((int)currentStep);

			// animating to the screen
			AnimateToScreen(shouldAnimate);
		}

		private void UpdateVolumeView(SwitchState state)
		{
			VolumeStatusSwitch.SetState(state);
			VolumeDetailsHeaderSwitch.SetState(state);
			VolumeDetailsPromptSwitch.SetState(state);
			VolumeDetailsIconSwitch.SetState(state);
		}

		/// <summary>
		/// Shows or hides an element on the screen based on what step in the FTUE the player is on
		/// </summary>
		private void ShowAndHideScreenElement(GameObject Element, string ExitAnimation)
		{
			if (currentStep == ReturnSetupStep.Reminders && !Element.activeSelf)
			{
				Element.SetActive(true);
			}
			if (currentStep == ReturnSetupStep.AlignPhone && Element.activeSelf)
			{
				Element.GetComponent<Animator>().Play(ExitAnimation);
			}
		}

		#endregion

		#region Event Handlers

		protected void OnAnimationCompleteHandler(object sender, AnimationEventArgs eventArgs)
		{
			TitleController.gameObject.SetActive(false);
		}

		public void OnBluetoothState(bool state)
		{
			// setting bluetooth switch
			SwitchState switchState = state ? SwitchState.On : SwitchState.Off;
			BluetoothStatusSwitch.SetState(switchState);
			BluetoothDetailsSwitch.SetState(switchState);
		}

		public void OnCloseButtonSelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ExitButton, gameObject);

			// displaying title screen
			TitleController.gameObject.SetActive(true);
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

		public void OnEnterHeadsetMode()
		{
			KpiTracking.TrackSceneLoadTime();

			if (IssueController.TotalIssues > 0)
			{
				// checking for issues
				IssueController.gameObject.SetActive(true);
			}
			else
			{
				// playing sound
				AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);

				// launching the game
				SetupController.LoadScene();
			}
		}

		protected void OnHmdConnected()
		{
			// launching the game
			SetupController.LoadScene();
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
			CalibrationComplete.SetActive(SetupController.IsPeripheralCalibrated);

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

		public void OnLightSaberCalibrationButtonSelected()
		{
			if (!SetupController.IsPeripheralCalibrated)
			{
				isLightSaberCalibrationInProgress = true;

				SetupController.Controller.StartCalibration(SetupController.OnCalibrationStateChanged);
			}
			else
			{
				// setting max fill for calibration bar
				CalibrationBar.fillAmount = 1;
			}
		}

		public void OnLightSaberCalibrationCloseButtonSelected()
		{
			if (isLightSaberCalibrationInProgress)
			{
				isLightSaberCalibrationInProgress = false;

				SetupController.Controller.StopCalibrating();
			}
		}

		public void OnLightSaberSyncButtonSelected()
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
			/*
			VolumeStatusSwitch.SetState(isMuted ? SwitchState.Off : SwitchState.On);
			VolumeDetailsPromptSwitch.SetState(isMuted ? SwitchState.Off : SwitchState.On);
			VolumeDetailsIconSwitch.SetState(isMuted ? SwitchState.Off : SwitchState.On);
			*/
		}

		protected void OnPeripheralConnected()
		{
			LightSaberStatusSwitch.SetState(SwitchState.On);

			// turning light saber blue
			LightSaberSwitch.SetState(SwitchState.On);

			// off state = light saber synced view
			LightSaberSyncSwitch.SetState(SwitchState.Off);

			// updating battery levels on status and detail
			int batteryLevel = SetupController.Controller.GetBatteryLevel();
			// TODO: mathh010 update from false to proper state of controller being charged or not
			LightSaberDetailsBattery.SetBatteryLevel(batteryLevel, false);
		}

		protected void OnPeripheralDisconnected()
		{
			if (!isSearchingForLightSaber)
			{
				LightSaberStatusSwitch.SetState(SwitchState.Off);

				// turning light saber white
				LightSaberSwitch.SetState(SwitchState.Off);

				// on state = light saber not synced view
				LightSaberSyncSwitch.SetState(SwitchState.On);
			}
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

		protected void OnToggleChangeHandler(object sender, ToggleEventArgs eventArgs)
		{
			ReturnSetupStep step = (ReturnSetupStep)eventArgs.SelectedToggle.transform.GetSiblingIndex();

			UpdateScreen(step);
		}

		public void OnSetupScreenActive(bool isActive)
		{
			setupScreenActive = isActive;
		}

		#endregion
	}
}