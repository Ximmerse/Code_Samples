using UnityEngine;
using Disney.Vision;
using Disney.ForceVision.Internal;
using System;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public enum PrepareHeadsetState
	{
		None = -1,
		TrayInHeadset,
		NoConnection,
		ConnectionFound,
		Controls
	}

	public class PrepareHeadsetController : MonoBehaviour
	{
		// Our instance of the SDK.
		public VisionSDK Sdk;

		public bool UsePrediction = true;

		public GameObject PreparePanel;
		public GameObject ControlsPrompt;

		public HMDConnectPopup HMDConnectPopup;

		public Transform UIHolder;

		public string LightSideGameScene;
		public string LightSideFtueScene;
		public string DarkSideGameScene;
		public string DarkSideFtueScene;
		public string PreLaunchScene;

		// Container Instance
		private ContainerAPI container;

		private PrepareHeadsetState currentState = PrepareHeadsetState.None;

		private void Start()
		{
			// Our Container API
			container = new ContainerAPI(Game.ForceVision);
			container.NativeBridge.OnLowMemory += OnLowMemory;

			// Setup the SDK
			Sdk.SetLogger(new VisionSdkLoggerProxy());
			OnSDKReady();

			Sdk.StereoCamera.UseMagnetometerCorrection = false;

			UIHolder.parent = Sdk.StereoCamera.transform;
			UIHolder.SnapToZero();

			ControlsPrompt.SetActive(false);
			PreparePanel.SetActive(true);

			currentState = PrepareHeadsetState.TrayInHeadset;
		}

		private void OnDestroy()
		{
			// Remove Input Events
			Sdk.Input.OnButtonDown -= OnButtonDown;

			// Kill SDK
			Sdk = null;

			// Kill the Container
			container.NativeBridge.OnLowMemory -= OnLowMemory;
			container.Dispose();
			container = null;
		}

		private void OnSDKReady()
		{
			// Add the audio Listener
			Sdk.StereoCamera.transform.gameObject.AddComponent<AkAudioListener>();

			// Setup Controllers
			ControllerPeripheral controller = new ControllerPeripheral(
				                                  VisionSDK.ControllerName,
				                                  new GameObject("Controller").transform,
				                                  null,
				                                  container.GetSavedSaberColorID());
			controller.UsePositionPrediction = UsePrediction;
			Sdk.Connections.AddPeripheral(controller);

			// Input Events
			Sdk.Input.OnButtonDown += OnButtonDown;

			HMDConnectPopup.Setup(Sdk, null);
		}

		private void Update()
		{
			if (currentState > PrepareHeadsetState.TrayInHeadset && !Application.isEditor)
			{
				HMDConnectPopup.CheckConnection();
			}

			switch (currentState)
			{
				case PrepareHeadsetState.NoConnection:
					if (!HMDConnectPopup.IsShowing())
					{
						currentState = PrepareHeadsetState.ConnectionFound;
					}
					break;
				case PrepareHeadsetState.ConnectionFound:
					// if connection is found, either show the controls panel or go straight to main game
					if (FtueDataController.IsFtueComplete(FtueType.Intro))
					{
						LoadScene(container.PlayingDarkSide ? DarkSideGameScene : LightSideGameScene);
					}
					else
					{
						currentState = PrepareHeadsetState.Controls;
					}
					break;
				default:
					if (HMDConnectPopup.IsShowing())
					{
						currentState = PrepareHeadsetState.NoConnection;
					}
					break;
			}

			PreparePanel.SetActive(currentState == PrepareHeadsetState.TrayInHeadset);
			ControlsPrompt.SetActive(currentState == PrepareHeadsetState.Controls);
		}

		private void OnButtonDown(object sender, ButtonEventArgs eventArguments)
		{
			switch (currentState)
			{
				case PrepareHeadsetState.TrayInHeadset:
					currentState = PrepareHeadsetState.ConnectionFound;
					break;
				case PrepareHeadsetState.Controls:
					AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);
					if (FtueDataController.IsFtueComplete(FtueType.Intro))
					{
						LoadScene(container.PlayingDarkSide ? DarkSideGameScene : LightSideGameScene);
					}
					else
					{
						LoadScene(container.PlayingDarkSide ? DarkSideFtueScene : LightSideFtueScene);
					}
					break;
			}
		}

		private void OnLowMemory(object sender, EventArgs eventArguments)
		{
			Log.Debug("Low Memory Warning");
		}

		public void OnBackToPreLaunch()
		{
			LoadScene(PreLaunchScene);
		}

		/// <summary>
		/// Shuts down this scene and loads the next scene
		/// </summary>
		/// <param name="sceneName">Scene to load.</param>
		private void LoadScene(string sceneName)
		{
			PreparePanel.SetActive(false);
			ControlsPrompt.SetActive(false);

			enabled = false;
			SceneManager.LoadScene(sceneName);
		}
	}
}