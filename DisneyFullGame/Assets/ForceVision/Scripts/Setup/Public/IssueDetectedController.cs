using Disney.Vision;
using UnityEngine;

namespace Disney.ForceVision
{
	public class IssueDetectedController : MonoBehaviour
	{
		#region Constants

		private const float MinimumPhoneBatteryLevel = .25f;
		private const int MinimumSaberBatteryLevel = 25;

		#endregion

		#region Properties

		public StereoSetupController SetupController;
		public SwitchController Header;
		public GameObject LightSaberNotSynced;
		public GameObject LightSaberNotCalibrated;
		public GameObject LightSaberLowBattery;
		public GameObject PhoneLowBattery;
		public GameObject SoundMuted;
		public GameObject ContinueAnywayButton;
		public SwitchController ButtonTextSwitch;

		public int TotalIssues
		{
			get
			{
				UpdateView();

				int numIssues = GetNumberOfIssues();

				return numIssues;
			}
		}

		protected bool IsPhoneBatteryLow
		{
			get
			{
				return SetupController.Container.NativeSettings.GetBatteryRemaining() < MinimumPhoneBatteryLevel;
			}
		}

		protected bool IsSaberBatteryLow
		{
			get
			{
				bool isLow = false;

				for (int i = 0; i < SetupController.Sdk.Connections.Peripherals.Count; i++)
				{
					if (SetupController.Sdk.Connections.Peripherals[i] is ControllerPeripheral)
					{
						isLow = SetupController.Sdk.Connections.Peripherals[i].GetBatteryLevel() < MinimumSaberBatteryLevel;
					}
				}

				return isLow;
			}
		}

		protected bool IsSoundMuted
		{
			get
			{
				return SetupController.Container.NativeSettings.GetVolume() <= 0;
			}
		}

		private int previousTotalIssues;
		private int currentTotalIssues;

		#endregion

		#region MonoBehaviour

		protected void Start()
		{
			// adding listeners for setup events
			StereoSetupEvents.OnPeripheralConnected += OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected += OnPeripheralDisconnected;
			StereoSetupEvents.OnMutedStateUpdate += OnMutedState;
			StereoSetupEvents.OnVolumeChanged += OnVolumeChanged;
			StereoSetupEvents.OnLightSaberCalibration += OnLightSaberCalibration;

			previousTotalIssues = 0;
			currentTotalIssues = 0;
		}

		protected void OnDestroy()
		{
			// removing listeners for setup events
			StereoSetupEvents.OnPeripheralConnected -= OnPeripheralConnected;
			StereoSetupEvents.OnPeripheralDisconnected -= OnPeripheralDisconnected;
			StereoSetupEvents.OnMutedStateUpdate -= OnMutedState;
			StereoSetupEvents.OnVolumeChanged -= OnVolumeChanged;
			StereoSetupEvents.OnLightSaberCalibration -= OnLightSaberCalibration;
		}

		protected void OnEnable()
		{
			currentTotalIssues = GetNumberOfIssues();

			if (previousTotalIssues == currentTotalIssues && currentTotalIssues != 0)
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.ConnectionNotFound, gameObject);
			}
		}

		#endregion

		#region Class Methods

		private int GetNumberOfIssues()
		{
			// calculating number of issues
			int totalIssues = 0;

			totalIssues += IsPhoneBatteryLow ? 1 : 0;
			totalIssues += IsSaberBatteryLow ? 1 : 0;
			totalIssues += IsSoundMuted ? 1 : 0;
			totalIssues += (!SetupController.IsPeripheralCalibrated && !FtueDataController.IsFtueComplete(FtueType.Setup)) ? 1 : 0;
			totalIssues += !SetupController.IsControllerConnected ? 1 : 0;

			return totalIssues;
		}

		private void UpdateView()
		{
			if (!Application.isEditor)
			{
				ContinueAnywayButton.SetActive(SetupController.IsControllerConnected);
			}

			LightSaberNotSynced.SetActive(!SetupController.IsControllerConnected);
			LightSaberNotCalibrated.SetActive(!SetupController.IsPeripheralCalibrated && !FtueDataController.IsFtueComplete(FtueType.Setup));

			// only displaying saber battery warning when saber is synced
			if (!SetupController.IsControllerConnected)
			{
				LightSaberLowBattery.SetActive(false);
			}
			else
			{
				LightSaberLowBattery.SetActive(IsSaberBatteryLow);
			}
			PhoneLowBattery.SetActive(IsPhoneBatteryLow);
			SoundMuted.SetActive(IsSoundMuted);

			currentTotalIssues = GetNumberOfIssues();

			// Handle UI and sounds based on when issues change while on this screen
			if (previousTotalIssues != currentTotalIssues && gameObject.activeSelf)
			{
				if (currentTotalIssues == 0)
				{
					Header.SetState(SwitchState.Off);
					ButtonTextSwitch.SetState(SwitchState.Off);
					AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);
				}
				else
				{
					Header.SetState(SwitchState.On);
					ButtonTextSwitch.SetState(SwitchState.On);

					if (currentTotalIssues > previousTotalIssues)
					{
						AudioEvent.Play(AudioEventName.Ftue.Stereo.ConnectionNotFound, gameObject);
					}
					else
					{
						AudioEvent.Play(AudioEventName.Ftue.Stereo.MinusButton, gameObject);
					}
				}

				previousTotalIssues = currentTotalIssues;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the continue anyway selected event
		/// </summary>
		public void OnContinueAnywaySelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);

			SetupController.LoadScene();
		}

		protected void OnPeripheralConnected()
		{
			UpdateView();
		}

		protected void OnPeripheralDisconnected()
		{
			UpdateView();
		}

		protected void OnMutedState(bool state)
		{
			UpdateView();
		}

		protected void OnVolumeChanged(float volumeLevel)
		{
			UpdateView();
		}

		protected void OnLightSaberCalibration(CalibrationState state)
		{
			UpdateView();
		}

		#endregion
	}
}