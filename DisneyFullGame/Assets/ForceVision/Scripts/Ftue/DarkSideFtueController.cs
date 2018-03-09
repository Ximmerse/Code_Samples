using DG.Tweening;
using Disney.Vision;
using System;
using UnityEngine;
using Disney.AssaultMode;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public enum DarkSideFtueInteractionType
	{
		None = 0,
		AttackKylo1,
		AttackKylo2,
		Finish
	}

	public class DarkSideFtueController : MonoBehaviour, ISaberController
	{
		#region Constants

		private const string HolocronColliderName = "Holocron_Rig_ROOTSHJnt";
		private const string AnimationConfigFile = "ftue-dark-side-animation-sequence.json";

		private const string KyloInterstitialStrikeNow = "kyloInterstitial_003_strikeNow";
		private const string KyloInterstitialStrikeAgain = "kyloInterstitial_004_remakeYourselfAnew";

		private const string MenuActivate = "sithMenuActivate";
		private const string MenuFtue = "sithMenuFTUE";
		private const string MenuHide = "sithMenuHide";
		private const string MenuIntro = "sithMenuIntro";

		#endregion

		#region Properties

		public VisionSDK Sdk;
		public GameObject Menu;
		public GameObject HmdDisconnectedPanel;
		public GameObject SaberNotSyncedPanel;
		public Transform ControllerTransform;
		public GameObject SithInterstitial;
		public GameObject AgentForDarkSide;

		private FtueAnimationSequence sequence;
		private ContainerAPI container;
		private FtueData ftueData;
		private bool beaconSeen = false;
		private DarkSideFtueInteractionType currentInteraction = DarkSideFtueInteractionType.None;
		private ControllerPeripheral saberPeripheral;
		private FtueAgent[] agents;

		#endregion

		#region MonoBehaviour

		protected void Awake()
		{
			// creating container
			container = new ContainerAPI(Game.ForceVision);
		}

		protected void Destroy()
		{
			// removing listener for animation sequence events
			FtueAnimationSequence.OnFtueAnimationSequenceComplete -= OnFtueAnimationSequenceCompleteHandler;
			FtueAnimationSequence.OnFtueAnimationWaiting -= OnFtueAnimationWaiting;
			FtueAnimationSequence.OnFtueAnimationClipComplete -= OnFtueAnimationClipComplete;

			// removing listeners on VisionSdk
			Sdk.Input.OnButtonDown -= OnButtonDown;
			Sdk.Tracking.OnBeaconStateChange -= OnBeaconStateChange;
			Sdk.Heartbeat.OnFrameUpdate -= OnSdkFrameUpdate;
			Sdk.Connections.OnPeripheralStateChange -= OnPeripheralConnected;
			Sdk.Connections.OnPeripheralStateChange -= OnPeripheralDisconnected;
		}

		protected void Start()
		{
			// setup the SDK
			Sdk.SetLogger(new Disney.ForceVision.Internal.VisionSdkLoggerProxy());
			Sdk.StereoCamera.UseMagnetometerCorrection = false;
			Sdk.Heartbeat.OnFrameUpdate += OnSdkFrameUpdate;
			OnSdkReady();
		}

		protected void Update()
		{
			#if UNITY_EDITOR

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				if (currentInteraction == DarkSideFtueInteractionType.AttackKylo1 || currentInteraction == DarkSideFtueInteractionType.AttackKylo2)
				{
					sequence.Play();
				}
			}

			#endif
		}

		#endregion

		#region Class Methods

		public void Enable()
		{
			// turning on the FTUE gameobject
			gameObject.SetActive(true);
		}

		protected void SetupInteraction(DarkSideFtueInteractionType interactionType)
		{
			// setting interaction
			currentInteraction = interactionType;

			switch (currentInteraction)
			{
			case DarkSideFtueInteractionType.AttackKylo1:
					break;
			case DarkSideFtueInteractionType.AttackKylo2:
				break;
			}
		}

		protected void ShowErrorPanel(GameObject errorPanel, bool isError)
		{
			if (errorPanel == null)
			{
				Log.Error("Null reference for errorPanel. Please ensure valid references on DarkSideFtueController.");

				return;
			}

			#if UNITY_EDITOR

			return;

			#else

			// showing/hiding error panel
			errorPanel.SetActive(isError);

			// setting flag in FtueAnimationSequence
			sequence.IsErrorState = isError;

			// playing audio if isError == true
			if (isError)
			{
				AudioEvent.Play(AudioEventName.Ftue.Stereo.ConnectionNotFound, gameObject);
			}

			#endif
		}

		#endregion

		#region Event Handlers

		protected void OnBeaconStateChange(object sender, BeaconStateChangeEventArgs eventArguments)
		{
			if (eventArguments.Tracked)
			{
				if (!beaconSeen)
				{
					beaconSeen = true;
				}
			}	
		}

		public void OnButtonDown(object sender, ButtonEventArgs eventArgs)
		{
			// enabling re-center
			if (eventArgs.Peripheral is ControllerPeripheral && eventArgs.Button == ButtonType.SaberControl)
			{
				(eventArgs.Peripheral as ControllerPeripheral).Recenter(Sdk.StereoCamera);
			}

			if (currentInteraction == DarkSideFtueInteractionType.None)
			{
				// getting hits at current camera position
				RaycastHit[] hits;
				Transform cameraTransform = Sdk.StereoCamera.transform;
				Vector3 fwd = cameraTransform.forward;
				hits = Physics.RaycastAll(cameraTransform.position, fwd, 1000f);

				// checking for hit on holocron
				if (hits != null)
				{
					for (int i = 0; i < hits.Length; i++)
					{
					}
				}
			}
			else if (currentInteraction == DarkSideFtueInteractionType.AttackKylo1)
			{
			}
			else if (currentInteraction == DarkSideFtueInteractionType.AttackKylo2)
			{
			}
		}

		protected void OnConfigLoaded(string error, string content)
		{
			// logging error
			if (!string.IsNullOrEmpty(error))
			{	
				Log.Error(string.Format("Error! {0}", error));
				return;
			}

			// signing up for animation step completion events
			FtueAnimationSequence.OnFtueAnimationSequenceComplete += OnFtueAnimationSequenceCompleteHandler;
			FtueAnimationSequence.OnFtueAnimationWaiting += OnFtueAnimationWaiting;
			FtueAnimationSequence.OnFtueAnimationClipComplete += OnFtueAnimationClipComplete;

			// parsing the config
			sequence = JsonUtility.FromJson<FtueAnimationSequence>(content);

			// creating array of FtueAgent instances
			FtueAgent[] agents = new FtueAgent[5] { null, null, null, null, AgentForDarkSide.GetComponent<DarkSideAgent>() };

			// initializing sequence
			sequence.Init(agents, gameObject, FtueType.DarkSide);
		}

		protected void OnFtueAnimationClipComplete(FtueAgentAnimationEvent eventArgs)
		{
		}

		protected void OnFtueAnimationSequenceCompleteHandler(FtueType state)
		{
			// setting FTUE flag
			if (state == FtueType.DarkSide)
			{
				// FTUE animation sequence is complete
				//FtueDataController.SetFtueComplete(FtueType.DarkSide);

				// displaying menu
				Menu.GetComponent<Animator>().Play(MenuIntro);
			}

			// TODO: disable the dark side FTUE
		}

		protected void OnFtueAnimationWaiting(FtueAgentAnimationEvent eventArgs)
		{
			if (string.Equals(eventArgs.Clip, KyloInterstitialStrikeNow))
			{
				currentInteraction = DarkSideFtueInteractionType.AttackKylo1;
			}
			else if (string.Equals(eventArgs.Clip, KyloInterstitialStrikeAgain))
			{
				currentInteraction = DarkSideFtueInteractionType.AttackKylo2;
			}
		}

		public void OnMenuAnimationComplete(string clipName)
		{
			if (string.Equals(clipName, MenuFtue))
			{
				Menu.GetComponent<Animator>().Play(MenuActivate);
			}
			else if (string.Equals(clipName, MenuActivate))
			{
				Menu.GetComponent<Animator>().Play(MenuHide);
			}
			else if (string.Equals(clipName, MenuHide))
			{
				SithInterstitial.SetActive(true);
			}
		}

		protected void OnPeripheralConnected(object sender, PeripheralStateChangeEventArgs eventArgs)
		{
			if (!eventArgs.Connected)
			{
				return;
			}

			if (eventArgs.Peripheral is HmdPeripheral)
			{
				ShowErrorPanel(HmdDisconnectedPanel, false);
			}
			else if (eventArgs.Peripheral is ControllerPeripheral)
			{
				ShowErrorPanel(SaberNotSyncedPanel, false);
			}
		}

		protected void OnPeripheralDisconnected(object sender, PeripheralStateChangeEventArgs eventArgs)
		{
			if (eventArgs.Connected)
			{
				return;
			}

			if (eventArgs.Peripheral is HmdPeripheral)
			{
				ShowErrorPanel(HmdDisconnectedPanel, true);
			}
			else if (eventArgs.Peripheral is ControllerPeripheral)
			{
				ShowErrorPanel(SaberNotSyncedPanel, true);
			}
		}

		protected void OnSdkReady()
		{
			// no further initialization needed if FTUE is complete
			if (FtueDataController.IsFtueComplete(FtueType.DarkSide))
			{
				return;
			}

			// adding controller
			saberPeripheral = new ControllerPeripheral(
				VisionSDK.ControllerName,
				ControllerTransform,
				null,
				container.GetSavedSaberColorID()
			);
			Sdk.Connections.AddPeripheral(saberPeripheral);

			// adding beacon
			Sdk.Tracking.OnBeaconStateChange += OnBeaconStateChange;

			// checking beacon signal
			if (Sdk.Tracking.IsBeaconTracked)
			{
				beaconSeen = true;
			}
			else
			{
			}

			// setting input handler
			Sdk.Input.OnButtonDown += OnButtonDown;

			// adding input listeners
			Sdk.Connections.OnPeripheralStateChange += OnPeripheralConnected;
			Sdk.Connections.OnPeripheralStateChange += OnPeripheralDisconnected;

			// loading animation sequence config
			StreamingAssetsStorage loader = new StreamingAssetsStorage(Game.ForceVision, null);
			loader.LoadStreamingAssetsText(AnimationConfigFile, OnConfigLoaded, true);

			// playing the menu FTUE animation
			Menu.GetComponent<Animator>().Play(MenuFtue);
		}

		protected void OnSdkFrameUpdate(object sender, EventArgs eventArguments)
		{
		}

		#endregion

		#region ISaberController

		public bool IsTracked()
		{
			if (Sdk == null || saberPeripheral == null)
			{
				return false;
			}

			return saberPeripheral.IsTracked;
		}

		public bool IsInView()
		{
			return true;
		}

		// not getting called within 3D FTUE (and Tri said so)
		public bool IsCircleButtonDown()
		{
			return false;
		}

		// not getting called within 3D FTUE (and Tri said so)
		public bool IsActivateButtonDown()
		{
			return false;
		}

		public void ClearButtons()
		{
		}

		public void TriggerHaptics(HapticsStrength hapticsStrength, HapticsType hapticsType)
		{
			if (Sdk == null || saberPeripheral == null)
			{
				return;
			}

			int strength = 0;
			float duration = 0.0f;

			switch (hapticsStrength)
			{
				case HapticsStrength.Light:
					strength = 30;
					break;

				case HapticsStrength.Medium:
					strength = 60;
					break;

				case HapticsStrength.Strong:
					strength = 100;
					break;
			}

			switch (hapticsType)
			{
				case HapticsType.Pulse:
					duration = 0.2f;
					break;

				case HapticsType.Vibrate:
					duration = 0.4f;
					break;
			}

			if (saberPeripheral != null)
			{
				saberPeripheral.Vibrate(strength, duration);
			}
		}

		#endregion
	}
}