using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Disney.Vision;
using Disney.Vision.Internal;

namespace Disney.ForceVision
{
	public class ResizeAlignScreen : MonoBehaviour
	{
		public GameObject CheckAlignment;
		public GameObject SelectProfile;
		public Button NextProfile;
		public Button PreviousProfile;
		public Button LaunchButton;
		public Button ProfilesButton;
		public GameObject ProfileTextHolder;
		public Text AlignPrompt;

		internal VisionSDK Sdk;

		private bool isSupported;
		private List<Device> knownDevicesForPlatform;
		private Dictionary<Device, DeviceSettings> knownSettings = new Dictionary<Device, DeviceSettings>();
		private int currentIndex;
		private JCSettingsManager SettingsLoader;
		private Text profileText;

		void Start()
		{
			SettingsLoader = Sdk.Settings as JCSettingsManager;

			knownDevicesForPlatform = SettingsLoader.GetDeviceListForPlatform();
			isSupported = JCSettingsManager.HasDeviceProfile;

			profileText = ProfileTextHolder.GetComponentInChildren<Text>();

			currentIndex = PruneAndSortListBySize();
			string previouslyChosenDeviceName = SettingsLoader.DeviceNameChosen();
			if (!string.IsNullOrEmpty(previouslyChosenDeviceName))
			{
				Log.Debug("previouslyChosenDeviceName: " + previouslyChosenDeviceName);
				foreach (Device device in knownDevicesForPlatform)
				{
					if (device.Name.Equals(previouslyChosenDeviceName))
					{
						currentIndex = knownDevicesForPlatform.IndexOf(device);
						//Log.Debug("found previouslyChosenSettingsFile at index: " + currentIndex);
						break;
					}
				}
			}
			else
			{
				if (isSupported)
				{
					// if we're showing the UI as an override then pick the matching device but allow them to change 
					currentIndex = knownDevicesForPlatform.IndexOf(Sdk.Settings.CurrentDevice);
				}
				if (currentIndex < 0)
				{
					Log.Warning("no saved settings or valid supportedDevice, using null");
				}
				else
				{
					Log.Debug("no saved settings, using : " + knownDevicesForPlatform[currentIndex].Name);
				}
			}
			UpdateForCurrentSetting();

			AlignPrompt.text = isSupported ? Localizer.Get("App.Prompt.PhoneProfile") : (Localizer.Get("App.Prompt.NotOnRecList") + " " + Localizer.Get("App.Prompt.PhoneProfile"));
		}

		public void SetProfileSelectable(bool chooseProfile, bool isFtue)
		{
			bool gotoProfileScreenFirst = chooseProfile && isFtue;
			CheckAlignment.SetActive(!gotoProfileScreenFirst);
			SelectProfile.SetActive(gotoProfileScreenFirst);

			ProfilesButton.gameObject.SetActive(chooseProfile);
		}

		public void OnNextDeviceSetting()
		{
			currentIndex = (currentIndex + 1) % knownDevicesForPlatform.Count;
			// skip if there is no Settings file
			if (string.IsNullOrEmpty(knownDevicesForPlatform[currentIndex].SettingsFile))
			{
				OnNextDeviceSetting();
			}
			    
			UpdateForCurrentSetting();
		}

		public void OnPreviousDeviceSetting()
		{
			if (--currentIndex < 0)
			{
				currentIndex = knownDevicesForPlatform.Count - 1;
			}

			// skip if there is no Settings file
			if (string.IsNullOrEmpty(knownDevicesForPlatform[currentIndex].SettingsFile))
			{
				OnPreviousDeviceSetting();
			}
			UpdateForCurrentSetting();
		}

		public void OnLaunched()
		{
			Log.Debug("OnLaunched");
			// save the current setting
			SettingsLoader.SaveDeviceNameChosen(knownDevicesForPlatform[currentIndex].Name);
		}

		private void UpdateForCurrentSetting()
		{
			if (currentIndex < 0)
			{
				Log.Warning("no current settings, using null");
			}
			else
			{
				DeviceSettings settings = SettingsLoader.LoadHardwareSettingsFile(knownDevicesForPlatform[currentIndex].SettingsFile);
				profileText.text = knownDevicesForPlatform[currentIndex].Name;
				ResizeForSettings(settings);
			}
		}

		private void ResizeForSettings(DeviceSettings settings)
		{
			CanvasScaler scaler = gameObject.GetComponentInParent<CanvasScaler>();
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			float width;
			float height;
			if (scaler != null)
			{
				width = scaler.referenceResolution.x;
				height = scaler.referenceResolution.y;
				Log.Debug("use scaler size: " + width + "x" + height);
			}
			else
			{
				width = (float)Screen.width;
				height = (float)Screen.height;
				Log.Debug("use screen size: " + width + "x" + height);
			}


			float left = width - settings.UILeftOffset * width;
			float top = height - settings.UITopOffset * height;
			float right = width - settings.UIRightOffset * width;
			float bottom = height - settings.UIBottomOffset * height;

			//Log.Debug("left:" + left + " top:" + top + " right:" + right + "bottom:" + bottom);

			rectTransform.offsetMin = new Vector2(left, bottom); //left, bottom;
			rectTransform.offsetMax = new Vector2(-right, -top); //-right, -top
		}

		private int PruneAndSortListBySize()
		{
			int bestMatch = 0;
			Device bestDeviceMatch = null;
			bool exactMatchFound = false;
			float bestScore = float.MaxValue;

			List<Device> newListWithNoMissingSettings = new List<Device>();
			// remove from list if there is no Settings file else load settings
			foreach (Device device in knownDevicesForPlatform)
			{
				if (!string.IsNullOrEmpty(device.SettingsFile))
				{
					DeviceSettings setting = SettingsLoader.LoadHardwareSettingsFile(device.SettingsFile);
					if (setting != null)
					{	
						knownSettings[device] = setting;
						newListWithNoMissingSettings.Add(device);

						// while we are at it, find the closest matching profile to recommend
						Vector2 size = GetPhysicalWindowSize(setting);
						float delta = GetDeltaFromTraySize(size);
						float aspectDelta = Mathf.Abs(size.x / size.y - Sdk.Settings.HeadsetSettings.UIPhysicalWidth / Sdk.Settings.HeadsetSettings.UIPhysicalHeight);
						float matchScore = GetMatchScore(size);
						//Log.Debug(device.Name + " tray size: " + size.x + "x" + size.y);
						//Log.Debug(" size delta: " + delta + " aspect delta: " + aspectDelta + " matchScore: " + matchScore);

						if (matchScore < bestScore && !exactMatchFound)
						{
							bestScore = matchScore;
							bestDeviceMatch = device;
						}

						// see if the dpi and screen size match exactly
						Vector2 dpi = SettingsManager.GetXYDpi();
						if (dpi.x == device.XDpi && dpi.y == device.YDpi && Screen.width == device.ScreenWidth && Screen.height == device.ScreenHeight)
						{
							exactMatchFound = true;
							bestScore = 0;
							bestDeviceMatch = device;
							//Log.Debug("exact match for dpi and screen size found for " + device.Name);
						}
					}
				}

			}
			knownDevicesForPlatform = newListWithNoMissingSettings;

			// sort the list by width
			knownDevicesForPlatform.Sort((deviceA, deviceB) =>
			{
				Vector2 sizeA = GetPhysicalWindowSize(knownSettings[deviceA]);
				Vector2 sizeB = GetPhysicalWindowSize(knownSettings[deviceB]);
				 
				// mult by dpi cuz the deltas might be less than 1 
				return sizeA.x > sizeB.x ? 1 : -1;
			});
			bestMatch = knownDevicesForPlatform.IndexOf(bestDeviceMatch);
			if (bestMatch < 0)
			{
				Log.Warning("no bestMatch found.");
			}
			else
			{
				Log.Debug("bestMatch: " + knownDevicesForPlatform[bestMatch].Name);
			}
			return bestMatch;
		}

		private float GetDeltaFromTraySize(Vector2 size)
		{
			return Mathf.Abs(size.x - Sdk.Settings.HeadsetSettings.UIPhysicalWidth) + Mathf.Abs(size.y - Sdk.Settings.HeadsetSettings.UIPhysicalHeight);
		}

		private float GetMatchScore(Vector2 size)
		{
			float aspectDelta = Mathf.Abs(size.x / size.y - Sdk.Settings.HeadsetSettings.UIPhysicalWidth / Sdk.Settings.HeadsetSettings.UIPhysicalHeight);
			return GetDeltaFromTraySize(size) * aspectDelta;
		}

		// return tray window size the settings would produce in inches
		Vector2 GetPhysicalWindowSize(DeviceSettings settings)
		{
			Vector2 result;
			float pixelWidth = Screen.width * (1f - (1f - settings.UILeftOffset) - (1f - settings.UIRightOffset));
			float pixelHeight = Screen.height * (1f - (1f - settings.UITopOffset) - (1f - settings.UIBottomOffset));
			Vector2 xyDpi = SettingsManager.GetXYDpi();
			result.x = pixelWidth / xyDpi.x;
			result.y = pixelHeight / xyDpi.y;
			return result;
		}
	}
}