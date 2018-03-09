//-----------------------------------------------------------------------
// <copyright file="JCSettingsManager.cs" company="Disney Interactive">
//     Copyright (c) 2018 Disney Interactive. All rights reserved.
// </copyright>
// <contact>Lee Chidgey</contact>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Disney.Vision;
using Disney.Vision.Internal;
using Disney.ForceVision;

namespace Disney.ForceVision
{
	/// <summary>
	/// A manager to keep track of settings for both HMD and Devices.
	/// </summary>
	[CreateAssetMenu(menuName = "ForceVision/Create JC Settings Manager")]
	public class JCSettingsManager : SettingsManager
	{
		#region Public Const Fields

		/// <summary>
		/// The Hmd player preference name.
		/// </summary>
		public static string HmdTypePlayerPref = "HMDType";

		/// <summary>
		/// The hardware setting device name chosen player preference key.
		/// </summary>
		public static string DeviceNameChosenPlayerPref = "DeviceChosen";

		// We used to save the filename of the hardware setting but that was not unique, so we switched to use the device name
		// keeping the legacy name define in case we need to support backward compatibilty
		public static string HardwareSettingChosenPlayerPref = "HardwareSettingChosen";

		private const string DeviceConfig = "devices.json";

		public const string LC = "PWhY";

		#endregion

		#region Public static Properties

		public static bool HasDeviceProfile;

		#endregion

		#region Private Properties

		private PersistentDataStorage persistentDataStorage;

		#endregion

		#region Public Methods

		/// <summary>
		/// Find the proper settings for the given device.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public override void Init(VisionLogger logger)
		{
			Init(logger, DeviceConfig);
		}

		/// <summary>
		/// Find the proper settings for the given device.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="deviceConfigFile">allow using a custom devices for testing.</param>
		public void Init(VisionLogger logger, string deviceConfigFile)
		{
			Logger = logger;

			persistentDataStorage = new PersistentDataStorage(Game.ForceVision);

			// Hardware
			TextAsset headsets = Resources.Load<TextAsset>("Profiles/headset_list");
			if (headsets == null)
			{
				Log.Error("Error: Fail to load hardware list");
				return;
			}

			HeadsetList = JsonUtility.FromJson<HeadsetList>(headsets.text);

			HeadsetSettings = GetRecomendedSettingsForHeadset();

			// Devices
			string devices = LoadFromCdnOrSdkResources(deviceConfigFile);
			if (devices == null)
			{
				Log.Error("Error: Fail to load device list " + deviceConfigFile);
				return;
			}
			DeviceList = JsonUtility.FromJson<Devices>(devices);

			PlatformName = (Application.platform == RuntimePlatform.IPhonePlayer) ? "iOS" : "Android";

			#if UNITY_EDITOR
			PlatformName = EditorPlatform;
			#endif

			DeviceSettings = GetRecomendedSettingsForDevice();		
			Log.Debug("JCSettingsManager inited with " + CurrentDevice.Name + " supported " + HasDeviceProfile);
		}

		/// <summary>
		/// Gets the recomended settings for device.
		/// </summary>
		/// <returns>The recomended settings for device.</returns>
		/// <param name="deviceName">Device name.</param>
		public override DeviceSettings GetRecomendedSettingsForDevice(string deviceName = null)
		{
			string chosenDeviceName = DeviceNameChosen();
			List<Device> devices = GetDeviceListForPlatform();

			if (!string.IsNullOrEmpty(chosenDeviceName))
			{
				CurrentDevice = devices.Find(item => item.Name.Equals(chosenDeviceName));
				if (CurrentDevice == null)
				{
					Log.Exception(new Exception("chosenDeviceName " + chosenDeviceName + " not found in device list"));
				}
				else
				{
					Log.Debug("Using saved device name " + chosenDeviceName);
					DeviceSettings = LoadHardwareSettingsFile(CurrentDevice.SettingsFile);
					HasDeviceProfile = true;
					return DeviceSettings;
				}
			}

			string model = deviceName ?? SystemInfo.deviceModel;

			#if UNITY_EDITOR
			model = EditorDeviceModel;
			#endif

			// Check if the model is in the list of device names
			Device device = devices.Find(item => item.Models.Find(modelName => modelName.Equals(model)) != null);

			CurrentDevice = device = device ?? devices.Find(item => item.BaseModelNames.Find(modelName => modelName.Contains(model)) != null);

			HasDeviceProfile = false;

			if (string.IsNullOrEmpty(device.SettingsFile))
			{
				Log.Error("Error: Device does not spefify a settings file " + device.Name);
				device = null;
			}

			// No device found matching the model.
			if (device == null)
			{
				if (devices.Count > 0)
				{
					CurrentDevice = device = devices[0];
					Log.Warning("Error: No Device Found " + model + " using first one");
				}
				else
				{
					Log.Exception(new Exception("Error: No Device Profiles Found"));
					return new DeviceSettings();
				}
			}
				
			DeviceSettings = LoadHardwareSettingsFile(device.SettingsFile);

			if (DeviceSettings == null)
			{
				Log.Error("Error: unable to load settings file " + device.SettingsFile);
				DeviceSettings = new DeviceSettings();
			}
			else
			{
				//Log.Debug("Supported device found, use settings " + device.SettingsFile);
				HasDeviceProfile = true;
			}
			return DeviceSettings;
		}

		public List<Device> GetDeviceListForPlatform(string platformArg=null)
		{
			platformArg = platformArg ?? PlatformName;
			return DeviceList.Platforms.Single(item => item.Platform == platformArg).Devices;
		}

		public DeviceSettings LoadHardwareSettingsFile(string settingsFile)
		{
			string deviceSettings = LoadFromCdnOrSdkResources(settingsFile);

			// No profile found for the given device.
			if (deviceSettings == null)
			{
				Log.Error("Error: Profile Not Found " + settingsFile);
				return new DeviceSettings();
			}

			return JsonUtility.FromJson<DeviceSettings>(deviceSettings);
		}

		public string DeviceNameChosen()
		{
			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (prefsStorage.PrefKeyExists(DeviceNameChosenPlayerPref))
			{
				return prefsStorage.GetPrefString(DeviceNameChosenPlayerPref);
			}
			return null;
		}

		public void SaveDeviceNameChosen(string deviceNameChosen)
		{
			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (!string.IsNullOrEmpty(deviceNameChosen))
			{
				Log.Debug("SaveDeviceNameChosen : " + deviceNameChosen); 
				prefsStorage.SetPrefString(DeviceNameChosenPlayerPref, deviceNameChosen);
			}
			else
			{
				Log.Debug("SaveDeviceNameChosen, delete key : " + DeviceNameChosenPlayerPref); 
				prefsStorage.DeletePref(DeviceNameChosenPlayerPref);
			}
		}
		#endregion

		private string LoadFromCdnOrSdkResources(string fileName)
		{
			string path = System.IO.Path.Combine("Profiles", fileName);

			// first try the OTA versions
			if (persistentDataStorage.FileExists(path))
			{
				Log.Debug("load settings from persistent : " + persistentDataStorage.GetPersistentDataPath(path));
				string text = persistentDataStorage.LoadText(path);
				if (text != null)
				{
					return text;
				}
			}

			// fallback to load from Resources
			path = path.Replace(".json", "").Replace(".txt", "");
			//Log.Debug("attempt to load Resource from " + path);
			TextAsset textAsset = Resources.Load<TextAsset>(path);
			return textAsset != null ? textAsset.text : null;
		}
	}
}