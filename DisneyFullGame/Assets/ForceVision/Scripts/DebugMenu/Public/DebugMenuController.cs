using Disney.Vision;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public class DebugMenuController : BaseController
	{
		public Text SkipLevelText;
		public Text PhoneMagText;
		public Text FloorOffsetText;
		public Text FpsText;
		public Text PredictionText;
		public Text DepthText;

		private void Start()
		{
			PhoneMagText.text = "Use Mags\n" + (Sdk.StereoCamera.UseMagnetometerCorrection ? "YES" : "NO");
			FloorOffsetText.text = "offset = " + Disney.Vision.Internal.XimmerseTracker.BeaconCenterOffsetFromFloor;
			FpsText.text = "Fps " + (KpiTracking.ShowFps ? "On" : "Off");
			PredictionText.text = "Prediciton = " + ((Sdk.Tracking.Hmd.UsePositionPrediction) ? "On" : "Off");
			DepthText.text = "Scale Depth = " + ((Disney.Vision.Internal.XimmerseTracker.ScaleControllerPositions) ? "On" : "Off");
			UpdateLevelToggleText();
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
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

		#region Debug Buttons

		public void OnLowerFloor()
		{
			float num = Disney.Vision.Internal.XimmerseTracker.BeaconCenterOffsetFromFloor += 1.0f / 6.0f;
			if (num > 1.1)
			{
				num = Disney.Vision.Internal.XimmerseTracker.BeaconCenterOffsetFromFloor = 0.0508f;
			}
			FloorOffsetText.text = "offset = " + num;
		}

		public void QuitApplication()
		{
			if (Application.isEditor)
			{
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#endif
			}
			else
			{
				Application.Quit();
			}
		}

		public void SetFTUECompleted()
		{
			if (!FtueDataController.HasPlayerCompletedFtue())
			{
				FtueDataController.SetFtueComplete(FtueType.Intro, true);
			}
		}

		public void GotoHardwareSettings()
		{
			SceneManager.LoadScene("HardwareSettings");
		}

		public void ToggleProgression()
		{
			#if !RC_BUILD || IS_DEMO_BUILD
			ContainerAPI.AllProgressionUnlocked = !ContainerAPI.AllProgressionUnlocked;
			(new ContainerAPI(Game.ForceVision)).LoadNextScene(false, false);
			#endif
		}

		public void LaunchEndlessAssault()
		{
			(new AssaultMode.AssaultAPI()).StartAssaultModeDebug();
		}

		public void SkipLevelToggle()
		{
			PillarMenuNode.SkipLevelType++;

			if (PillarMenuNode.SkipLevelType > 2)
			{
				PillarMenuNode.SkipLevelType = 0;
			}

			UpdateLevelToggleText();
		}

		private void UpdateLevelToggleText()
		{
			switch (PillarMenuNode.SkipLevelType)
			{
				case 0:
					SkipLevelText.text = "Skip Level\nOFF";
					break;

				case 1:
					SkipLevelText.text = "Skip Level\nWIN";
					break;

				case 2:
					SkipLevelText.text = "Skip Level\nLOSE";
					break;
			}
		}

		public void PhoneMagToggle()
		{
			if (Sdk.StereoCamera.UseMagnetometerCorrection)
			{
				if (KpiTracking.ShowMagReading)
				{
					Sdk.StereoCamera.UseMagnetometerCorrection = false;
					KpiTracking.ShowMagReading = false;
					PhoneMagText.text = "Use Mags\nNO";
				}
				else
				{
					KpiTracking.ShowMagReading = true;
					PhoneMagText.text = "Use Mags\nShow Mags";
				}
			}
			else
			{
				Sdk.StereoCamera.UseMagnetometerCorrection = true;
				PhoneMagText.text = "Use Mags\nYES";
			}
		}

		public void ClearProgression()
		{
			PersistentDataStorage vision = new PersistentDataStorage(Game.None);
			vision.DeleteFolder(vision.GetPersistentDataPath(""));

			PlayerPrefs.DeleteAll();

			ProxyReload.SceneToLoad = SceneManager.GetActiveScene().name;
			SceneManager.LoadScene("ReloadProxyScene");
		}

		public void PlayAnimation(string animation)
		{
			ArchivistInterstitialController.Triggers.Clear();
			ArchivistInterstitialController.Triggers.Add(animation);
			(new ContainerAPI(Game.ForceVision)).LoadNextScene(false, false);
		}

		public void ToggleFps()
		{
			KpiTracking.ShowFps = !KpiTracking.ShowFps;
			FpsText.text = "Fps " + (KpiTracking.ShowFps ? "On" : "Off");
		}

		/// <summary>
		/// Unlocks all holochess pawns.
		/// </summary>
		public void UnlockAllHolochessPawns()
		{
			HoloChess.HolochessStatic.Progress.SetPawnsUnlocked(99);
		}

		public void ChangePrediciton()
		{
			Sdk.Tracking.Hmd.UsePositionPrediction = !Sdk.Tracking.Hmd.UsePositionPrediction;
			PredictionText.text = "Prediciton = " + ((Sdk.Tracking.Hmd.UsePositionPrediction) ? "On" : "Off");
		}

		public void ChangeDepthScale()
		{
			Disney.Vision.Internal.XimmerseTracker.ScaleControllerPositions = !Disney.Vision.Internal.XimmerseTracker.ScaleControllerPositions;
			DepthText.text = "Scale Depth = " + ((Disney.Vision.Internal.XimmerseTracker.ScaleControllerPositions) ? "On" : "Off");
		}

		#endregion
	}
}
