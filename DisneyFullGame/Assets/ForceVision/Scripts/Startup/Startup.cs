using System;
using System.Collections;
using Disney.ForceVision.Internal;
using Disney.Vision;
using UnityEngine;
using UnityEngine.UI;
using DCPI.Platforms.SwrveManager.Analytics;

namespace Disney.ForceVision
{
	public class Startup : MonoBehaviour
	{
		#region Constants

		public const string JY = "b2JZ";

		#endregion

		#region Public Properties

		public MainNavigationController MainNavigation;

		// The controllers transform.
		public Transform ControllerTransform;

		// For Debugging
		public Text LogField;

		// Our Menu Controller
		public MenuController Menu;

		// The Rotation controller for the galaxy
		public InputRotation GalaxyRotation;

		// Whether or not to use prediction for Ximmerse
		public bool UsePrediction = true;

		#endregion

		#region Private Properties

		// Our instance of the SDK.
		public VisionSDK Sdk;

		// Container Instance
		private ContainerAPI container;

		// If the beacon has been scene at least once.
		private bool beaconSeenOnce = false;

		private bool firmwareButtonDown = false;

		private FadeHandler fadeHandler;

		#endregion

		#region Unity Methods

		private void OnGUI()
		{
			#if !RC_BUILD
			if (firmwareButtonDown)
			{
				GUI.skin.label.fontSize = 42;
				GUI.Label(new Rect(10, 10, Screen.width, Screen.height), "HMD Firmware: " + Sdk.Tracking.Hmd.GetFirmwareVersion());
				GUI.Label(new Rect(10, 60, Screen.width, Screen.height),
				          "Saber Firmware: " + Sdk.Connections.GetPeripheral(VisionSDK.ControllerName).GetFirmwareVersion());
				GUI.Label(new Rect(10, 120, Screen.width, Screen.height), "SDK Version: 3.0.3.6");
				GUI.Label(new Rect(10, 180, Screen.width, Screen.height), "Build Number: " + Application.version);
				GUI.Label(new Rect(10, 240, Screen.width, Screen.height), "Press the mid button also to enter Settings mode");
				GUI.Label(new Rect(10, 300, Screen.width, Screen.height), "\"" + SystemInfo.deviceModel + "\" Settings File: " +
				          Sdk.Settings.CurrentDevice.SettingsFile);
			}
			#endif
		}

		private void Start()
		{
			// Our Container API
			container = new ContainerAPI(Game.ForceVision);
			container.NativeBridge.OnLowMemory += OnLowMemory;

			// Hook up the debug canvas
			Log.SetUILogTarget(LogField);

			// Setup the SDK
			Sdk.SetLogger(new VisionSdkLoggerProxy());
			OnSDKReady();

			Sdk.StereoCamera.UseMagnetometerCorrection = false;

			if (GalaxyRotation != null)
			{
				GalaxyRotation.CameraTransform = Sdk.StereoCamera.transform;
			}

			Analytics.LogAction(new ActionAnalytics("start.complete.app"));

		}

		private void OnDestroy()
		{
			// Remove Input Events
			Sdk.Tracking.OnBeaconStateChange -= OnBeaconStateChanged;
			Sdk.Connections.OnPeripheralStateChange -= OnPeripheralStateChange;
			Sdk.Input.OnButtonDown -= OnButtonDown;
			Sdk.Input.OnButtonUp -= OnButtonUp;
			Sdk.Input.OnButtonPress -= OnButtonPress;

			// Kill SDK
			Sdk = null;

			// Kill the Container
			container.NativeBridge.OnLowMemory -= OnLowMemory;
			container.Dispose();
			container = null;
		}

		#endregion

		#region Private Methods
					
		private void OnSDKReady()
		{
			Log.Debug("SDK Ready");
			Analytics.LogAction(new ActionAnalytics("ready.sdk"));

			if (!fadeHandler)
			{
				fadeHandler = GetComponent<FadeHandler>();
			}
			if (!fadeHandler)
			{
				fadeHandler = gameObject.AddComponent<FadeHandler>();
			}
				
			MainNavigation.Setup(Sdk, false);
			fadeHandler.Setup(MainNavigation, Menu.GetComponent<Animator>());

			// Add the audio Listener
			Sdk.StereoCamera.gameObject.AddComponent<AkAudioListener>();

			// Setup Controllers
			if (!ControllerTransform)
			{
				ControllerTransform = (new GameObject("ControllerTransform")).transform;
			}
			ControllerPeripheral controller = new ControllerPeripheral(VisionSDK.ControllerName,
			                                                           ControllerTransform,
			                                                           null,
			                                                           container.GetSavedSaberColorID());
			controller.UsePositionPrediction = UsePrediction;
			Sdk.Connections.AddPeripheral(controller);

			ControllerTransform.gameObject.SetActive(false);

			Sdk.Tracking.OnBeaconStateChange += OnBeaconStateChanged;

			// Input Events
			Sdk.Connections.OnPeripheralStateChange += OnPeripheralStateChange;
			Sdk.Input.OnButtonDown += OnButtonDown;
			Sdk.Input.OnButtonUp += OnButtonUp;
			Sdk.Input.OnButtonPress += OnButtonPress;

			// Make Lenovo one-time call
			StartCoroutine(MakeLenovoOneTimeCall());

			// We are already looking at the beacon
			if (Application.isEditor || Sdk.Tracking.IsBeaconTracked)
			{
				beaconSeenOnce = true;

				Menu.Setup();
			}
		}

		/// <summary>
		/// Coroutine to make the Lenovo one time call.
		/// </summary>
		private IEnumerator MakeLenovoOneTimeCall()
		{
			bool callMade = false;
			while (!callMade)
			{
				if (Sdk.Tracking.Hmd == null || !Sdk.Tracking.Hmd.Connected)
				{
					yield return null;
				}
				else
				{
					OneTimeCall.Init((new Config()).GetPlatformConfig(ServiceType.Lenovo), Sdk.Tracking.Hmd);
					callMade = true;
				}
			}
		}

		private void OnPeripheralStateChange(object sender, PeripheralStateChangeEventArgs eventArguments)
		{
			Sdk.Logger.Log("Is " + eventArguments.Peripheral.Name + " Connected? " + eventArguments.Connected);

			if (eventArguments.Connected && eventArguments.Peripheral is ControllerPeripheral)
			{
				StartCoroutine(SetSaberColor(eventArguments.Peripheral as ControllerPeripheral));
			}
		}

		private IEnumerator SetSaberColor(ControllerPeripheral peripheral)
		{
			yield return new WaitForSeconds(2.0f);

			peripheral.SetColor();
		}

		private void OnButtonDown(object sender, ButtonEventArgs eventArguments)
		{
			if (Time.timeSinceLevelLoad < 0.5f)
			{
				return;
			}

			Log.Debug("OnButtonDown " + eventArguments.Button.ToString());

			firmwareButtonDown = (eventArguments.Button == ButtonType.HmdMenu);

			if (eventArguments.Button == ButtonType.SaberActivate || eventArguments.Button == ButtonType.SaberControl)
			{
				Vector3 currentPosition = Vector3.zero;

				if (Application.isEditor)
				{
					currentPosition = Input.mousePosition;
					currentPosition.z = 10.0f;
					currentPosition = Sdk.StereoCamera.RightCamera.ScreenToWorldPoint(currentPosition);
				}
				else
				{
					currentPosition = ControllerTransform.position;
				}

				if (GalaxyRotation != null)
				{
					GalaxyRotation.StartRotation(currentPosition);
				}
			}
		}

		private void OnButtonPress(object sender, ButtonEventArgs eventArguments)
		{
			if (Time.timeSinceLevelLoad < 0.5f)
			{
				return;
			}

			if (eventArguments.Button == ButtonType.SaberActivate || eventArguments.Button == ButtonType.SaberControl)
			{
				Vector3 currentPosition = Vector3.zero;
				if (Application.isEditor)
				{
					currentPosition = Input.mousePosition;
					currentPosition.z = 10.0f;
					currentPosition = Sdk.StereoCamera.RightCamera.ScreenToWorldPoint(currentPosition);
				}
				else
				{
					currentPosition = ControllerTransform.position;
				}

				if (GalaxyRotation != null)
				{
					GalaxyRotation.UpdateRotation(currentPosition);
				}
			}
		}

		private void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (Time.timeSinceLevelLoad < 0.5f)
			{
				return;
			}

			firmwareButtonDown = false;

			if (GalaxyRotation != null)
			{
				if ((eventArguments.Button == ButtonType.SaberActivate || eventArguments.Button == ButtonType.SaberControl) && !GalaxyRotation.EndRotation())
				{
					return;
				}
			}

			if (fadeHandler.MenuOpen)
			{
				return;
			}

			if (beaconSeenOnce)
			{
				if (eventArguments.Button == ButtonType.HmdBack)
				{
					Menu.BackSelected();
				}

				if (eventArguments.Button == ButtonType.SaberActivate || eventArguments.Button == ButtonType.SaberControl || eventArguments.Button == ButtonType.HmdSelect)
				{
					Menu.NodeSelected();
				}
			}
		}

		private void OnLowMemory(object sender, EventArgs eventArguments)
		{
			Log.Debug("Low Memory Warning");
		}

		private void OnBeaconStateChanged(object sender, BeaconStateChangeEventArgs eventArguments)
		{
			if (eventArguments.Tracked)
			{
				if (!beaconSeenOnce)
				{
					beaconSeenOnce = true;

					Menu.Setup();
				}
			}
		}

		#endregion
	}
}