using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using BSG.Util;

namespace Disney.ForceVision
{
	public class PreLaunch : MonoBehaviour
	{
		public const string RG = "R+Aq";

		public Text BluetoothMessage;

		public Text BrightnessMessage;

		public Text VolumeMessage;

		public Text PlayButtonMessage;

		public Slider VolumeSlider;

		public Slider BrightnessSlider;

		public Dropdown HeadsetChooser;

		public GameObject DownloadPopoutPrefab;

		public Transform MainCanvas;

		// scene to load when the play button is pressed
		public string PlayButtonScene = "Main";

		public GameObject ContainerSoundBanks;

		private ContainerAPI container = null;

		private bool bluetoothOn = false;

		#if UNITY_ANDROID
		private static float bluetoothCheckInterval = 1.0f;

		private float bluetoothCheckTime;
		#endif

		private PlayerPrefsStorage prefsStorage;

		private bool isStarted = false;

		private GameObject downloadPopout;

		/// <summary>
		/// Initializes sliders and bluetooth interrogation.
		/// </summary>
		void Start()
		{
#if RC_BUILD
			if ((new NativeSettings()).IsDebuggerAttached())
			{
				Application.Quit();
			}
#endif

			SetLanguage();

			LoadContent();

#if UNITY_ANDROID
			bluetoothCheckTime = Time.time + bluetoothCheckInterval;
#endif

			container = new ContainerAPI(Game.ForceVision);
			container.NativeSettings.GetBluetoothState("PreLaunchViewController");

			float volumeLevel = container.NativeSettings.GetVolume();

			VolumeSlider.value = volumeLevel;

			container.NativeSettings.GetBrightness(OnBrightnessChanged);

			//check for and load hmd type player pref
			prefsStorage = new PlayerPrefsStorage(Game.ForceVision);

			isStarted = true;
		}

		#if UNITY_ANDROID
		/// <summary>
		/// Checks bluetooth status every n seconds.
		/// </summary>
		void Update()
		{
			// Only require for android.  The OnApplicationPause gets triggered on the device this was tested with
			// when turning bluetooth on via the drop down menu as a dialog is displayed but turning it off by the
			// same means does not.  The other alternatives are using an Invoke/InvokeRepeating, or setting up a
			// BroadcastReceiver (Native Android).
			if (Time.time > bluetoothCheckTime)
			{
				container.NativeSettings.GetBluetoothState("PreLaunchViewController");
				bluetoothCheckTime = Time.time + bluetoothCheckInterval;
			}
		}
		#endif

		/// <summary>
		/// Receiver for brightness status changes.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		public void OnBrightnessChanged(float brightness)
		{
			BrightnessSlider.value = brightness;
		}

		/// <summary>
		/// Receiver for volume slider status changes.
		/// </summary>
		public void OnVolumeSliderChanged()
		{
			container.NativeSettings.SetVolume(VolumeSlider.value);
		}

		/// <summary>
		/// Receiver for brightness slider status changes.
		/// </summary>
		public void OnBrightnessSliderChanged()
		{
			container.NativeSettings.SetBrightness(BrightnessSlider.value);
		}

		/// <summary>
		/// Receiver for bluetooth status changes.
		/// </summary>
		/// <param name="state">State.</param>
		public void OnBluetoothState(string state)
		{
			// TODO: LOCALIZATION!!!
			switch (state)
			{
				case "ON":
					bluetoothOn = true;
					BluetoothMessage.text = Localize.Get("FTUE.BluetoothMessage.On");
					PlayButtonMessage.text = Localize.Get("FTUE.PlayButton.On");
					break;

				case "OFF":
					bluetoothOn = false;
					BluetoothMessage.text = Localize.Get("FTUE.BluetoothMessage.Off");
					PlayButtonMessage.text = Localize.Get("FTUE.PlayButton.Off");
					break;
			}
		}

		/// <summary>
		/// Handler for the play button.
		/// </summary>
		public void OnPlayButton()
		{
			if (bluetoothOn || Application.isEditor)
			{
				KpiTracking.TrackSceneLoadTime();
				SceneManager.LoadScene(PlayButtonScene);
			}
		}

		/// <summary>
		/// Launch the SettingsDemo scene.
		/// </summary>
		public void OnSettingsButton()
		{
			if (bluetoothOn || Application.isEditor)
			{
				SceneManager.LoadScene("HardwareSettings");
			}
		}

		/// <summary>
		/// Raises the dropdown changed event.
		/// </summary>
		public void OnDropdownChanged()
		{
			if (isStarted == true)
			{
				string pref = HeadsetChooser.options[HeadsetChooser.value].text;
				prefsStorage.SetPrefString(JCSettingsManager.HmdTypePlayerPref, pref);
			}
		}

		/// <summary>
		///changes application pause status.
		/// </summary>
		/// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
		void OnApplicationPause(bool pauseStatus)
		{
			if (!pauseStatus && container != null)
			{
				container.NativeSettings.GetBluetoothState("PreLaunchViewController");

				LoadContent();
			}
		}

		private void LoadContent()
		{
			DownloadControllerFactory.CancelAll();
			DownloadControllerFactory factory = new DownloadControllerFactory();
			downloadPopout = Instantiate(DownloadPopoutPrefab);
			downloadPopout.transform.SetParent(MainCanvas, false);
			DownloadController downloadController = factory.CreateDownloadController(this, (success, id) =>
			{
				if (success == true)
				{
					Localizer.Load(true);
				}
				if (downloadPopout != null)
				{
					Destroy(downloadPopout);
				}
				downloadPopout = null;
			}, Localizer.Locale, (prog) =>
			{
				if (downloadPopout != null)
				{
					downloadPopout.GetComponent<Disney.ForceVision.Internal.DownloadPopoutPanel>().UpdateProgress(prog);
				}
			});
			downloadController.Init();
		}

		private void SetLanguage()
		{
			Localizer.GetLocaleFromPref();
			string[] bankNames = Localizer.GetSoundBankNames();
			StartCoroutine(Localizer.ChangeWWiseLanguage(bankNames, bankNames));
		}

	}
}
