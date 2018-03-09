using Disney.Vision;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DCPI.Platforms.SwrveManager.Analytics;
using Disney.ForceVision.Internal;

namespace Disney.ForceVision
{
	public class StereoSetupEvents
	{
		public static Action OnHmdConnected;
		public static Action OnHmdDisconnected;
		public static Action OnPeripheralConnected;
		public static Action OnPeripheralDisconnected;
		public static Action<bool> OnBluetoothStateUpdate;
		public static Action<bool> OnMutedStateUpdate;
		public static Action<float> OnVolumeChanged;
		public static Action<CalibrationState> OnLightSaberCalibration;
	}

	public class StereoSetupController : MonoBehaviour
	{
		#region Constants

		public const string EnterAnimationClip = "SetupScreen_Intro";
		public const string EnterAnimationComplete = "StereoSetupEnterAnimation";
		public const float MinConfidenceLevel = 0.8f;
		public const string PositionTokenIn = "FTUE.Prompt.StopperPosition.In";
		public const string PositionTokenOut = "FTUE.Prompt.StopperPosition.Out";

		private const float MaxBrightness = 1f;
		private const float VolumeChangeAmount = 1f / 16f;

		#endregion

		#region Properties

		public GameObject AndroidPermissionPrompt;
		public GameObject PrePromptAndroidPermission;
		public GameObject AndroidPermissionConfirmed;
		public GameObject AndroidPermissionDenied;
		public GameObject TitleScreen;
		public GameObject LanguageSelection;
		public DownloadPanel DownloadPopout;
		public Transform MainCanvas;
		public SwitchController DebugFtueState;
		public string NextScene;
		public Toggle[] SpatializationToggles;
		public Toggle ProfileSelectionToggle;

		// Demo related options
		public GameObject OptionsSpacer;
		public GameObject DemoOptions;
		public Toggle AllProgressionUnlockedToggle;
		public Toggle GoProModeToggle;

		public GameObject CreditsEndMessage;
		public CanvasScaler Scaler;
		public GameObject ContainerSoundBanks;

		public TrailerPlayer TrailerPlayer;

		public Button ARKitButton;
		public string ARKitScene = "Holochess ARKit Menu";

		public ResizeAlignScreen resizeAlignFTUE;
		public ResizeAlignScreen resizeAlignRTUE;

		public bool IsPeripheralCalibrated { get; private set; }

		public VisionSDK Sdk;

		public ContainerAPI Container
		{
			get
			{
				if (container == null)
				{
					// creating a instance of the ContainerAPI
					container = new ContainerAPI(Game.ForceVision);
				}

				return container;
			}
		}

		public bool IsControllerConnected
		{
			get
			{
				return Controller != null && Controller.Connected;
			}
		}

		public ControllerPeripheral Controller;

		private ContainerAPI container;
		private static float deviceCheckInterval = 0.5f;
		private float deviceCheckTime;
		private float currentVolume = 0f;
		private bool isBluetoothEnabled;
		private int calibrationState = -1;
		private bool isConfirmShownOnce = false;
		private GameObject InSceneContainerSounds;

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (ContainerSoundBanks)
			{
				InSceneContainerSounds = GameObject.FindGameObjectWithTag(ContainerSoundBanks.name);
				if (InSceneContainerSounds == null)
				{
					InSceneContainerSounds = Instantiate(ContainerSoundBanks);
				}
			}
		}

		private void Start()
		{
#if UNITY_IOS
			if (Application.isEditor)
			{
				ARKitButton.gameObject.SetActive(true);
			}
			else
			{
				bool supported = UnityEngine.XR.iOS.ARKitSessionConfiguration.IsSupported;
				ARKitButton.gameObject.SetActive(supported);
			}
#else
			ARKitButton.gameObject.SetActive(false);
#endif

			// checking to see if trailer has played
			if (TrailerPlayer.PlayCount < 1)
			{
				// IntroPlayedCount key doesn't exist, so trailer has never been played, so start playback
				StartCoroutine(PlayTrailerThenCheckForSetup());
			}
			else
			{
				// trailer has been played, continue with setup
				Setup();
			}
		}

		/// <summary>
		/// Plays the trailer then checks for setup.
		/// </summary>
		private IEnumerator PlayTrailerThenCheckForSetup()
		{
			yield return TrailerPlayer.PlayTrailer();

			// displaying the language panel once the trailer is done only if this is the first play thru
			if (TrailerPlayer.PlayCount == 1)
			{
				Setup();
			}
		}

		private void Setup()
		{
			DontDestroyOnLoad(InSceneContainerSounds);

			// adding listeners
			LanguageSelectionEvents.OnLanguageSelected += OnLanguageSelected;
			LanguageSelectionEvents.OnLanguageSelectionClosed += OnLanguageSelectionClosed;

			// playing background music
			AudioEvent.Play(AudioEventName.Ftue.Stereo.BackgroundMusicStart, gameObject, 1.0f);

			//show language selection if not set already
			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (prefsStorage.PrefKeyExists(Localizer.LanguagePrefKey) == true)
			{
				LanguageSelection.SetActive(false);
				TitleScreen.SetActive(true);
			}
			else
			{
				LanguageSelection.SetActive(true);
				TitleScreen.SetActive(false);
			}

			// getting bluetooth, listening for bluetooth state changes
			Container.NativeSettings.GetBluetoothState("StereoSetupCanvas");

			// getting brightness level, listening brightness level changes
			Container.NativeSettings.SetBrightness(MaxBrightness);

			Controller = new ControllerPeripheral(VisionSDK.ControllerName);
			Sdk.Connections.AddPeripheral(Controller);

			Sdk.Connections.OnPeripheralStateChange += OnPeripheralStateChange;

			resizeAlignFTUE.Sdk = Sdk;
			resizeAlignRTUE.Sdk = Sdk;

			StereoSetupFtueController ftue = MainCanvas.GetComponentInChildren<StereoSetupFtueController>(true);
			StereoSetupNonFtueController rtue = MainCanvas.GetComponentInChildren<StereoSetupNonFtueController>(true);
			ftue.Sdk = Sdk;
			rtue.Sdk = Sdk;

			// starting device check timer
			deviceCheckTime = Time.time + deviceCheckInterval;

			// adding listeners for title screen related events
			TitleScreenEvents.OnMenuSelected += OnMenuSelected;

			#if !RC_BUILD

			// adding listener for touch down time reached
			TouchDownTimer.OnTouchDownTimeReached += OnTouchDownTimeReached;

			#endif

			QualityController qualityController = new QualityController(Sdk);
			qualityController.ApplyQuality();

			#if UNITY_EDITOR

			currentVolume = Mathf.Floor(UnityEngine.Random.Range(0, 1f) * 10) / 10f;

			#else

			Log.Debug("Call LoadContent from Start");
			LoadContent();
			currentVolume = container.NativeSettings.GetVolume();

			#endif

			// 3D sound setting
			for (int i = 0; i < SpatializationToggles.Length; i++)
			{
				SpatializationToggles[i].isOn = container.UseSpatialization();
			}

			// If the phone is not supported, profile selection is always on
			if (!JCSettingsManager.HasDeviceProfile)
			{
				ProfileSelectionToggle.isOn = true;
				ProfileSelectionToggle.interactable = false;
			}
			else
			{
				// profile selection override setting
				ProfileSelectionToggle.isOn = ContainerAPI.UseProfileSelection();
			}
			SetPofilesSelectableInFTUEAndRTUE(ProfileSelectionToggle.isOn);

			#if IS_DEMO_BUILD

			ContainerAPI.LoadDemoOptions();

			// make demo options visible and set UI state
			DemoOptions.SetActive(true);
			OptionsSpacer.SetActive(false);
			AllProgressionUnlockedToggle.isOn = ContainerAPI.AllProgressionUnlocked;
			GoProModeToggle.isOn = Disney.Vision.Internal.XimmerseTracker.UseGoProCameras;

			#endif

			// this controller is common to both ftue and rtue so lets log the OTA version here (new ota data should already be downloaded)
			string otaVersion = DownloadController.GetManifestVersion()[0];

			Analytics.LogAction(new ActionAnalytics(SystemInfo.deviceModel,
			                                        "StereoSetupStart.ota" + otaVersion + (JCSettingsManager.HasDeviceProfile ? ".supported_device" : ".unsupported_device"),
			                                        -1,
			                                        ContainerAPI.UseProfileSelection().ToString()
			));
			SetLanguage();

			if (CreditsEndMessage != null)
			{
				CreditsEndMessage.SetActive(ContainerAPI.IsMedalUnlocked(MedalType.Mastery));
			}

			// Check what device the app is running on and adjust the UI accordingly
			var device = Sdk.Settings.CurrentDevice;

			if (device != null)
			{
				switch (device.Name)
				{
					case "iPhone X":
						Scaler.matchWidthOrHeight = 1f;
						break;
				}
			}
		}

		/// <summary>
		/// Sets the language.
		/// </summary>
		private void SetLanguage()
		{
			Localizer.GetLocaleFromPref();
			string[] bankNames = Localizer.GetSoundBankNames();
			StartCoroutine(Localizer.ChangeWWiseLanguage(bankNames, bankNames));
		}

		/// <summary>
		/// Checks various device settings on Android or iOS every deviceCheckInterval seconds.
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				#if UNITY_ANDROID
				using (AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
				{
					AndroidJavaObject unityActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
					unityActivity.Call<bool>("moveTaskToBack", true);
				}
				#endif
				return;
			}

			if (Time.time > deviceCheckTime)
			{
				#if UNITY_ANDROID

				// Only require for android.  The OnApplicationPause gets triggered on the device this was tested with
				// when turning bluetooth on via the drop down menu as a dialog is displayed but turning it off by the
				// same means does not.  The other alternatives are using an Invoke/InvokeRepeating, or setting up a
				// BroadcastReceiver (Native Android).
				Container.NativeSettings.GetBluetoothState("StereoSetupCanvas");

				#endif

				deviceCheckTime = Time.time + deviceCheckInterval;

				DispatchSetupEvents();
			}
		}

		/// <summary>
		///changes application pause status.
		/// </summary>
		/// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
		void OnApplicationPause(bool pauseStatus)
		{
			if (!pauseStatus)
			{
				#if UNITY_ANDROID && !UNITY_EDITOR
				//hide goto settings screen if permissions are true.
				if (HasAndroidPermission())
				{
					AndroidPermissionDenied.SetActive(false);
				}
				#endif

				Log.Debug("Call LoadContent from OnApplicationPause.");
				LoadContent();
			}
		}

		private void OnDestroy()
		{
			// removing listeners for HmdWatcher
			if (Sdk != null)
			{
				Sdk.Connections.OnPeripheralStateChange -= OnPeripheralStateChange;
			}

			// removing events
			LanguageSelectionEvents.OnLanguageSelected -= OnLanguageSelected;
			LanguageSelectionEvents.OnLanguageSelectionClosed -= OnLanguageSelectionClosed;
			TitleScreenEvents.OnMenuSelected -= OnMenuSelected;

			// Kill the Container
			Container.Dispose();
			container = null;
		}

		#endregion

		#region Class Methods

		/// <summary>
		/// Checks the connected state of the peripheral.
		/// </summary>
		/// <returns><c>true</c>, if peripheral is connected, <c>false</c> otherwise.</returns>
		public bool StartPeripheralPairing()
		{
			bool isPaired = IsControllerConnected;

			if (!isPaired)
			{
				Sdk.Tracking.StartPairing();
			}

			return isPaired;
		}

		private void DispatchSetupEvents()
		{
			// dispatching bluetooth state event
			if (StereoSetupEvents.OnBluetoothStateUpdate != null)
			{
				StereoSetupEvents.OnBluetoothStateUpdate(isBluetoothEnabled);
			}

			// dispatching muted state event
			if (StereoSetupEvents.OnMutedStateUpdate != null)
			{
				StereoSetupEvents.OnMutedStateUpdate(Container.NativeSettings.IsMuted());
			}

			// checking if peripheral has auto connected
			if (IsControllerConnected)
			{
				// dispatching the peripheral connected event
				if (StereoSetupEvents.OnPeripheralConnected != null)
				{
					StereoSetupEvents.OnPeripheralConnected();
				}
			}
			else
			{
				// dispatching the peripheral connected event
				if (StereoSetupEvents.OnPeripheralDisconnected != null)
				{
					StereoSetupEvents.OnPeripheralDisconnected();
				}
			}

			// dispatching volume event
			if (StereoSetupEvents.OnVolumeChanged != null)
			{
				StereoSetupEvents.OnVolumeChanged.Invoke(Container.NativeSettings.GetVolume());
			}
		}

		public void LoadScene()
		{
			// stopping background music
			AudioEvent.Play(AudioEventName.Ftue.Stereo.BackgroundMusicStop, gameObject);

			Application.targetFrameRate = 60;
			
			#if IS_DEMO_BUILD
			if (ContainerAPI.AllProgressionUnlocked)
			{
				SG.Lonestar.DuelAPI duelApi = new SG.Lonestar.DuelAPI();
				duelApi.Inventory.ForcePowers.GiveAllItems();
				duelApi.Inventory.ForcePowers.SaveToDisk();
				duelApi.Inventory.PassiveAbilities.GiveAllItems();
				duelApi.Inventory.PassiveAbilities.SaveToDisk();
				Disney.HoloChess.HolochessStatic.Progress.SetPawnsUnlocked(1000);
			}
			else
			{
				Log.Debug("clear data.");
				string path = Application.persistentDataPath;

				System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

				foreach (System.IO.FileInfo file in di.GetFiles())
				{
					file.Delete();
				}

				foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
				{
					dir.Delete(true);
				}

				Log.Debug("unlock naboo pillars.");
				(new Disney.AssaultMode.AssaultAPI()).SetRatingForStage(PlanetType.Naboo, 1, (int)Difficulty.Easy, 1);
				(new Disney.AssaultMode.AssaultAPI()).SetRatingForStage(PlanetType.Naboo, 2, (int)Difficulty.Easy, 1);

				// Set win on tutorial
				SG.Lonestar.DuelAPI duelApi = new SG.Lonestar.DuelAPI();
				duelApi.Progress.SetVictory(duelApi.Progress.Battles[0], 0);
				duelApi.Progress.SaveToDisk();
			}
			#endif

			// loading next scene
			SceneManager.LoadScene(NextScene);
		}

		/// <summary>
		/// Loads the AR kit scene.
		/// </summary>
		public void LoadARKitScene()
		{
			AudioEvent.Play(AudioEventName.Settings.Restart, gameObject);

			SceneManager.LoadScene(ARKitScene);
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Receiver for bluetooth status changes.
		/// </summary>
		/// <param name="state">State.</param>
		public void OnBluetoothState(string state)
		{
			// saving bluetooth state
			isBluetoothEnabled = string.Equals(state, "ON");
		}

		public void OnCalibrationStateChanged(CalibrationState newState)
		{
			// setting calibration flag
			if (newState == CalibrationState.Complete && calibrationState == 2)
			{
				IsPeripheralCalibrated = true;

				Controller.StopCalibrating();
			}

			// saving calibration state
			calibrationState = (int)newState;

			// dispatching calibration event
			if (StereoSetupEvents.OnLightSaberCalibration != null)
			{
				StereoSetupEvents.OnLightSaberCalibration.Invoke(newState);
			}
		}

		private void OnPeripheralStateChange(object sender, PeripheralStateChangeEventArgs eventArguments)
		{
			Sdk.Logger.Log("Is " + eventArguments.Peripheral.Name + " Connected? " + eventArguments.Connected);

			if (eventArguments.Peripheral is ControllerPeripheral)
			{
				// stopping the pairing process
				Sdk.Tracking.StopPairing();
			}
			else if (eventArguments.Peripheral is HmdPeripheral)
			{
				if (eventArguments.Peripheral.Connected)
				{
					KpiTracking.TrackSceneLoadTime();

					// dispatching the HMD connected event
					if (StereoSetupEvents.OnHmdConnected != null)
					{
						StereoSetupEvents.OnHmdConnected();
					}
				}
				else
				{
					// dispatching the HMD disconnected event
					if (StereoSetupEvents.OnHmdDisconnected != null)
					{
						StereoSetupEvents.OnHmdDisconnected();
					}
				}
			}
		}

		public void OnImageZoomCloseSelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ExitButton, gameObject);
		}

		public void OnImageZoomSelected()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ZoomIn, gameObject);
		}

		protected void OnLanguageSelected(object sender, EventArgs eventArgs)
		{
			// displaying title screen
			TitleScreen.SetActive(true);
		}

		protected void OnLanguageSelectionClosed(object sender, EventArgs eventArgs)
		{
			// displaying title screen
			TitleScreen.SetActive(true);
		}

		protected void OnMenuSelected(object sender, EventArgs eventArgs)
		{
			// displaying language screen
			LanguageSelection.SetActive(true);
		}

		protected void OnTouchDownTimeReached()
		{
			// toggling SwitchController
			DebugFtueState.Toggle();

			// updating ftue data based on state
			FtueDataController.SetFtueComplete(FtueType.Intro, !(DebugFtueState.State == SwitchState.On));

			// playing animation
			DebugFtueState.GetComponent<Animator>().Play("2D_Debug_Display");
		}

		public void OnVolumeChangeSelected(int change)
		{
			float newVolume = currentVolume + (change * VolumeChangeAmount);
			newVolume = Mathf.Clamp01(newVolume);

			if (newVolume != currentVolume)
			{
				currentVolume = newVolume;

				#if !UNITY_EDITOR

				container.NativeSettings.SetVolume(currentVolume);

				#endif
			}

			// playing sound
			if (currentVolume > 0 && currentVolume < 1)
			{
				AudioEvent.Play(change < 0 ? AudioEventName.Ftue.Stereo.MinusButton : AudioEventName.Ftue.Stereo.PlusButton,
				                gameObject);
			}
		}

		public void ToggleSpatialization(Toggle toggle)
		{
			container.SetSpatialization(toggle.isOn);
		}

		public void ToggleProfileSelection(Toggle toggle)
		{
			SetPofilesSelectableInFTUEAndRTUE(toggle.isOn);
			ContainerAPI.SaveProfileSelectionEnabled(toggle.isOn);
		}

		public void ToggleUnlockAllProgression(Toggle toggle)
		{
			#if IS_DEMO_BUILD
			Log.Debug("ToggleUnlockAllProgression " + toggle.isOn);
			ContainerAPI.AllProgressionUnlocked = toggle.isOn;
			ContainerAPI.SaveAllProgressionUnlocked(toggle.isOn);
			#endif
		}

		public void ToggleGoProMode(Toggle toggle)
		{
			#if IS_DEMO_BUILD
			Log.Debug("ToggleGoProMode " + toggle.isOn);
			Disney.Vision.Internal.XimmerseTracker.UseGoProCameras = toggle.isOn;
			ContainerAPI.SaveGoProMode(toggle.isOn);
			#endif
		}


		private void SetPofilesSelectableInFTUEAndRTUE(bool isOn)
		{
			Log.Debug("Set Profiles Selectable to " + isOn);

			// they behave opposite in terms of showing profiles first
			resizeAlignFTUE.SetProfileSelectable(isOn, true);
			resizeAlignRTUE.SetProfileSelectable(isOn, false);
		}

		protected void LoadContent()
		{
			#if !SKU_CHINA
			DownloadControllerFactory.CancelAll();

			Log.Debug("LoadContent");
			DownloadPopout.StartWindow(DownloadController.InstanceIdCount + 1, false);
			DownloadControllerFactory factory = new DownloadControllerFactory();
			DownloadController downloadController = factory.CreateDownloadController(this, (success, id) =>
			{
				if (success == true)
				{
					Localizer.Load(true);
				}
				if (DownloadPopout != null)
				{
					DownloadPopout.DownloadComplete(success, id);
				}
			}, Localizer.Locale, (prog) =>
			{
				if (DownloadPopout != null)
				{
					if (prog.CurrentSection == prog.Sections && prog.FileRemainingCount > 0 && prog.PercentOfCurrentLoadingFileDownloaded < 0.9f)
					{
						DownloadPopout.gameObject.SetActive(true);
					}
					DownloadPopout.UpdateProgress(prog);
				}
			});
			downloadController.Init();
			#endif
		}

		public void PlayConfirmSound()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);
		}

		public void PlayDeclineSound()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ConnectionNotFound, gameObject);
		}

		public void PlaySequenceSound()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.Swipe, gameObject);
		}

		public void PlayMenuButtonSound()
		{
			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.SubmenuButton, gameObject);
		}

		public void OnShowAndroidPermission()
		{
			if (Application.isEditor)
			{
				return;
			}

			#if UNITY_ANDROID
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			object[] args = new object[1];
			args[0] = new string[] {"android.permission.ACCESS_COARSE_LOCATION"};
			permissions.CallStatic("RequestPermission", args);
			#endif
		}

		public bool HasAndroidPermission()
		{
			if (Application.isEditor)
			{
				return true;
			}

			#if UNITY_ANDROID
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			object[] args = new object[1];
			args[0] = new string[] {"android.permission.ACCESS_COARSE_LOCATION"};
			return permissions.CallStatic<bool>("CheckPermission", args);
			#else
			return true;
			#endif
		}

		public void OnGoToApplicationSettings()
		{
			if (Application.isEditor)
			{
				return;
			}

			#if UNITY_ANDROID
			Log.Debug("GoToSettings");
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			permissions.CallStatic("GotoApplicationSettings");
			#endif
		}

		#if UNITY_ANDROID
		public void onRequestPermissionsResult(string isGranted)
		{
			if (Application.isEditor)
			{
				return;
			}

		Log.Debug("onRequestPermissionsResult isGranted = " + isGranted);
		if (isGranted == "true")
		{
		if(isConfirmShownOnce == false)
		{
		AndroidPermissionPrompt.SetActive(true);
		AndroidPermissionConfirmed.SetActive(true);
		PrePromptAndroidPermission.SetActive(false);
		}
		isConfirmShownOnce = true;
		}
		else
		{
		AndroidPermissionDenied.SetActive(true);
		AndroidPermissionPrompt.SetActive(false);
		AndroidPermissionConfirmed.SetActive(false);
		PrePromptAndroidPermission.SetActive(false);
		}
		}
		#endif

		#endregion
	}
}
