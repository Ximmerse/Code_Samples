using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Disney.ForceVision.Internal;
using Disney.Vision;

namespace Disney.ForceVision
{
	public class QualityController
	{
		public const string ResolutionFileName = "Resolution.txt";
		public const string QualitySettingsFileName = "Quality.txt";

		private PersistentDataStorage storage;
		private long physicalMemory;
		private VisionSDK sdk;

		public QualityController(VisionSDK sdk)
		{
			physicalMemory = new NativeSettings().GetPhysicalMemory() / 1000000;
			Log.Debug("QualityController availableMemroy = " + physicalMemory);
			storage = new PersistentDataStorage(Game.ForceVision);
			this.sdk = sdk;
		}

		public Quality GetQuality()
		{
			int storeadQuality = (int)Enum.GetValues(typeof(Quality)).Cast<Quality>().Last();

			if (storage.FileExists(QualitySettingsFileName) == true)
			{
				storeadQuality = int.Parse(storage.LoadText(QualitySettingsFileName));
				Log.Debug("loading quality from file. quality = " + storeadQuality);
			}
			else
			{
				Device device = sdk.Settings.CurrentDevice;
				if (device != null)
				{
					Log.Debug("getting qaultiy on first run based on device.");
					storeadQuality = device.Quality;
				}
				Log.Debug("saving first time run quality. quality = " + storeadQuality);
				SaveQuality(storeadQuality);
			}

			return (Quality)storeadQuality;
		}

		public void SaveQuality(int quality)
		{
			storage.SaveText(QualitySettingsFileName, quality.ToString(), false);
		}

		public void ApplyQuality()
		{
			Quality quality = GetQuality();

			Log.Debug("ApplyQuality availableMemroy = " + physicalMemory);
			int qualityToSet = (int)quality;
			if (physicalMemory < 1200)
			{
				if (quality > Quality.Medium)
				{
					Log.Debug("High quality requires more than one gig physical memory, default to medium quality.");
					qualityToSet = (int)Quality.Medium;
				}
			}
			Log.Debug("setting quality to " + qualityToSet);
			UnityEngine.QualitySettings.SetQualityLevel(qualityToSet, true);

			if (storage.FileExists(ResolutionFileName) == false)
			{
				storage.SaveText(ResolutionFileName, Screen.currentResolution.width + "," + Screen.currentResolution.height, false);
			}

			string savedResolution = storage.LoadText(ResolutionFileName);
			string[] parts = savedResolution.Split(',');
			int[] loadedResolution = new int[2]{ int.Parse(parts[0]), int.Parse(parts[1]) };

			if (quality == Quality.High || quality == Quality.Medium)
			{
				foreach (Display display in Display.displays)
				{
					display.SetRenderingResolution(loadedResolution[0], loadedResolution[1]);
				}
			}
			if (quality == Quality.Low)
			{
				foreach (Display display in Display.displays)
				{
					if (loadedResolution[0] > 1200 && loadedResolution[1] > 1200)
					{
						display.SetRenderingResolution(loadedResolution[0] / 2, loadedResolution[1] / 2);
					}
				}
			}
		}

		private void SetSustainedPerformanceMode(bool mode)
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			// turn on sustained performance mode on Android devices
			NativeSettingsAndroid nativeSettings = new NativeSettingsAndroid();
			nativeSettings.SetSustainedPerformanceMode(mode);
			#endif
		}

	}
}