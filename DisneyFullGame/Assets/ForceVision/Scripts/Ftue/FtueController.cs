using DG.Tweening;
using Disney.Vision;
using System;
using UnityEngine;
using Disney.AssaultMode;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public enum FtueAgentType
	{
		Holocron,
		Yoda,
		Archivist,
		Droid,
		DarkSide
	}

	public enum HolocronAnimations
	{
	}

	public enum YodaAnimations
	{
		Show,
		LightsaberInstructions,
		Hide
	}

	public enum ArchivistAnimations
	{
		anim000,
		anim020,
		anim030,
		anim031,
		anim032,
		anim035,
		anim037,
		anim040,
		anim060,
		anim080,
		anim090,
		anim210,
		anim220,
		anim240,
		anim250,
		anim260,
		anim260a
	}

	public enum DroidAnimations
	{
		Show,
		Wait,
		Hide
	}

	public enum FtueInteractionType
	{
		None = 0,
		ActivationMatrix,
		AssaultModePrep,
		AssaultMode,
		Finish
	}

	public class FtueController : MonoBehaviour, ISaberController
	{
		#region Constants

		private const string HolocronColliderName = "Holocron_Rig_ROOTSHJnt";
		private const string AnimationConfigFile = "ftue-animation-sequence.json";
		private const string HolocronEnterAnimationComplete = "HolocronEnterComplete";

		private const float PlanetSelectionDelay = 7f;

		private const string AnimationClip_ArchivistSelectNaboo = "FTUE_DX_Arch_PromptSelectNaboo";

		private const string AnimationClip_Interaction1_PressActivationMatrix = "archivistContainer_032_pressActivationMatrix";
		private const string AnimationClip_Interaction1_SaberStance = "archivistContainer_031_saberStance";
		private const string AnimationClip_Interaction1_SwitchOnSaber = "archivistContainer_035_switchOnYourSaber";
		private const string AnimationClip_Interaction1_VeryGood = "archivistContainer_060_veryGood";
		private const float Interaction1_WaitTime = 5f;

		private const string AnimationClip_Interaction2_MoveToBust = "archivistContainer_090_moveToBust";
		private const string AnimationClip_Interaction2_MoveToFull = "archivistContainer_220_firstStep_moveToFull";

		#endregion

		#region Properties

		public VisionSDK Sdk;

		public GameObject Menu;
		public GameObject HolocronContainer;
		public GameObject HologramGlow;
		public GameObject Droid;
		public GameObject SaberPositionHelper;
		public GameObject FtueTooltip;
		public GameObject HmdDisconnectedPanel;
		public GameObject SaberNotSyncedPanel;
		public Transform ControllerTransform;
		public SimpleSaberStarter Saber;
		public ObjectTracker DroidTracker;
		public AnimationEvents HolocronAnimations;
		public string SceneToLoad;

		private FtueAnimationSequence sequence;
		private FtueAgent[] agents;
		private ContainerAPI container;
		private FtueData ftueData;
		private bool beaconSeen = false;
		private float interval;
		private bool isFtueGalaxyMapDisplayed;
		private FtueInteractionType currentInteraction = FtueInteractionType.None;
		private float currentTime = 0f;
		private float targetTime = 0f;
		private bool shouldTrackTime = false;
		private Action onTimerCompleteHandler;
		private ControllerPeripheral saberPeripheral;

		#endregion

		#region MonoBehaviour

		protected void Awake()
		{
			// setting agent references
			SetAgents();

			// creating container
			container = new ContainerAPI(Game.ForceVision);
		}

		protected void Destroy()
		{
			// removing listener for animation sequence events
			FtueAnimationSequence.OnFtueAnimationSequenceComplete -= OnFtueAnimationSequenceCompleteHandler;
			FtueAnimationSequence.OnFtueAnimationWaiting -= OnFtueAnimationWaiting;
			FtueAnimationSequence.OnFtueAnimationClipComplete -= OnFtueAnimationClipComplete;

			// removing listener for button down event
			Sdk.Input.OnButtonDown -= OnButtonDown;

			// removing listener for beacon state change
			Sdk.Tracking.OnBeaconStateChange -= OnBeaconStateChange;

			// removing listener for frame update
			Sdk.Heartbeat.OnFrameUpdate -= OnSdkFrameUpdate;

			// removing input listeners
			Sdk.Connections.OnPeripheralStateChange -= OnPeripheralConnected;
			Sdk.Connections.OnPeripheralStateChange -= OnPeripheralDisconnected;

			// removing listener for saber swings
			SaberEvents.OnSaberSwing -= OnSaberSwing;

			// removing listener for droid reaching target
			ObjectTracker.OnObjectReachedTargetLocation -= OnObjectReachedTargetLocation;
			ObjectTracker.OnObjectLeftTargetLocation -= OnObjectLeftTargetLocation;

			// removing listener for enemy death
			BattleDroidAI.OnEnemyDeath -= OnEnemyDeath;
		}

		protected void Start()
		{
			if (!FtueDataController.IsFtueComplete(FtueType.Intro))
			{
				AkBankManager.LoadBank("Container_DX_SB", false, false);
			}

			// setup the SDK
			Sdk.SetLogger(new Disney.ForceVision.Internal.VisionSdkLoggerProxy());

			Sdk.Heartbeat.OnFrameUpdate += OnSdkFrameUpdate;
			Sdk.StereoCamera.UseMagnetometerCorrection = false;

			Invoke("OnSdkReady", 0.1f);
		}
			
		#endregion

		#region Class Methods

		protected void AnimateHolocronDown()
		{
			// animating the holocron down
			HolocronContainer.transform.DOMove(Vector3.zero, 1f).SetEase(Ease.InCubic).OnComplete(() =>
			{
				// turning off tool tip
				FtueTooltip.SetActive(false);

				// turning on glow
				HologramGlow.SetActive(true);

				// starting animation sequence
				sequence.Play();
			});
		}

		public void Disable()
		{
			if (agents == null)
			{
				SetAgents();
			}

			foreach (FtueAgent agent in agents)
			{
				if (agent != null)
				{
					agent.gameObject.SetActive(false);
				}
			}
		}

		public void Enable()
		{
			// turning on the FTUE tooltip
			//FtueToolTip.SetActive(true);

			// turning on the FTUE gameobject
			gameObject.SetActive(true);
		}

		protected void SetAgents()
		{
			// creating array of FtueAgent instances
			agents = new FtueAgent[4];

			// TODO: mathh010 update with reference to HolocronAgent when available
			agents[0] = null;

			// getting yoda
			agents[1] = GetComponentInChildren<YodaAgent>(true);

			// getting archivist
			agents[2] = GetComponentInChildren<ArchivistAgent>(true);

			// getting droid
			agents[3] = GetComponentInChildren<DroidAgent>(true);
		}

		protected void SetupInteraction(FtueInteractionType interactionType)
		{
			// setting interaction
			currentInteraction = interactionType;

			switch (currentInteraction)
			{
				case FtueInteractionType.ActivationMatrix:
					// starting timer
					StartTimer(Interaction1_WaitTime, () =>
					{
						sequence.Play(FtueType.Intro, AnimationClip_Interaction1_SwitchOnSaber);
					});
					break;
				
				case FtueInteractionType.AssaultMode:
					// turning on droid
					Droid.SetActive(true);

					// playing audio with delay
					DOVirtual.DelayedCall(0.25f, () =>
					{
						AudioEvent.Play(AudioEventName.Ftue.Droid.Roger, gameObject);
						AudioEvent.Play(AudioEventName.Ftue.Archivist.FirstOpponent, gameObject, 0.75f);
					});
					
					// listening for saber swings
					SaberEvents.OnSaberSwing += OnSaberSwing;

					// listener for droid reaching target
					ObjectTracker.OnObjectReachedTargetLocation += OnObjectReachedTargetLocation;
					ObjectTracker.OnObjectLeftTargetLocation += OnObjectLeftTargetLocation;

					// listening for enemy death
					BattleDroidAI.OnEnemyDeath += OnEnemyDeath;

					// starting tracking
					DroidTracker.StartTracking();

					break;
			}
		}

		protected void ShowErrorPanel(GameObject errorPanel, bool isError)
		{
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

		private void StartTimer(float timerAmount, Action timerCompleteHandler)
		{
			onTimerCompleteHandler = timerCompleteHandler;
			targetTime = timerAmount;
			shouldTrackTime = true;
		}

		private void EndTimer()
		{
			onTimerCompleteHandler = null;

			currentTime = 0f;
			targetTime = 0f;

			shouldTrackTime = false;
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

					// playing audio - signal acquired
					AudioEvent.Play(AudioEventName.Ftue.Computer.SignalAcquired, gameObject);
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

			if (currentInteraction == FtueInteractionType.None)
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
						if (string.Equals(HolocronColliderName, hits[i].transform.name))
						{
							// disable the collider
							hits[i].transform.GetComponent<SphereCollider>().enabled = false;

							// performing holocron animation to origin
							AnimateHolocronDown();

							// hiding the tooltip
							FtueTooltip.SetActive(false);

							//play holocron drop sfx
							AudioEvent.Play(AudioEventName.Holocron.FTUEHolocronDrop, HolocronContainer);

							break;
						}
					}
				}
			}
			else if (currentInteraction == FtueInteractionType.ActivationMatrix)
			{
				if (eventArgs.Button == ButtonType.SaberActivate)
				{
					if (shouldTrackTime)
					{
						EndTimer();
					}

					// Make sure the saber blade is centered.
					if (eventArgs.Peripheral != null && eventArgs.Peripheral is ControllerPeripheral)
					{
						(eventArgs.Peripheral as ControllerPeripheral).Recenter(Sdk.StereoCamera);
					}

					// turning on the saber
					Saber.StartSaber();

					// hiding the saber position helper
					SaberPositionHelper.SetActive(false);

					// continuing FTUE, updating the interaction type
					sequence.Play(FtueType.Intro, AnimationClip_Interaction1_VeryGood);
					currentInteraction = FtueInteractionType.AssaultModePrep;
				}
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

			// initializing sequence
			sequence.Init(agents, gameObject);

			// setting the holocron position for initial entry
			Vector3 newPos = new Vector3(HolocronContainer.transform.position.x,
			                             Sdk.StereoCamera.transform.position.y,
			                             HolocronContainer.transform.position.z);
			HolocronContainer.transform.position = newPos;

			// playing holocron enter animation
			HolocronContainer.GetComponent<Animator>().Play("Enter");
		}

		protected void OnEnemyDeath()
		{
			// turning off saber
			DOVirtual.DelayedCall(2f, () =>
			{
				Saber.StopSaber();

				// stopping any currently playing audio for progression-related audio
				AudioEvent.Stop("", gameObject);

				// setting audio ending section
				AudioEvent.Play(AudioEventName.Ftue.MusicEnding, gameObject);

				// continuing FTUE animation sequence
				sequence.Play(FtueType.Intro, AnimationClip_Interaction2_MoveToFull);
			});
		}

		protected void OnFtueAnimationClipComplete(FtueAgentAnimationEvent eventArgs)
		{
			if (string.Equals(eventArgs.Clip, AnimationClip_Interaction1_SaberStance))
			{
				// displaying saber position helper
				SaberPositionHelper.SetActive(true);
			}
		}

		protected void OnFtueAnimationSequenceCompleteHandler(FtueType state)
		{
			// setting FTUE flag
			if (state == FtueType.Intro)
			{
				// FTUE animation sequence is complete
				FtueDataController.SetFtueComplete(FtueType.Intro);

				// setting holocron to not destroy
				DontDestroyOnLoad(Menu);
				MenuController.HolocronObjectToDestroy = Menu;

				// disabling holocron animation for Main scene
				MenuController.ShouldHolocronAnimate = false;

				// loading next scene
				SceneManager.LoadScene(SceneToLoad);
			}

			// disabling the FTUE
			Disable();
		}

		protected void OnFtueAnimationWaiting(FtueAgentAnimationEvent eventArgs)
		{
			// checking which interaction
			if (string.Equals(eventArgs.Clip, AnimationClip_Interaction1_SwitchOnSaber))
			{
				SetupInteraction(FtueInteractionType.ActivationMatrix);
			}
			else if (string.Equals(eventArgs.Clip, AnimationClip_Interaction2_MoveToBust))
			{
				SetupInteraction(FtueInteractionType.AssaultMode);
			}
		}

		public void OnHolocronEnterAnimationComplete(object sender, AnimationEventArgs eventArgs)
		{
			if (string.Equals(eventArgs.AnimationName, HolocronEnterAnimationComplete))
			{
				FtueTooltip.transform.position = HolocronContainer.transform.position - (Vector3.up * 0.125f);
				FtueTooltip.SetActive(true);

				// removing listener
				HolocronAnimations.OnAnimationComplete -= OnHolocronEnterAnimationComplete;
			}
		}

		/// <summary>
		/// Handler when the droid leaves the target
		/// </summary>
		protected void OnObjectLeftTargetLocation()
		{
		}

		protected void OnObjectReachedTargetLocation()
		{
			// NOTE: The droid often reaches the target location while the archivist is
			// still talking so delay it for a bit.
			Invoke("PlayDroidInRangeSound", 1.0f);
		}

		protected void PlayDroidInRangeSound()
		{
			AudioEvent.PlayOnce(AudioEventName.Ftue.Archivist.DroidInRange, gameObject);
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

		protected void OnSaberSwing(object sender, EventArgs eventArgs)
		{
			if (DroidTracker != null)
			{
				if (!DroidTracker.IsTargetInRange)
				{
					// NOTE: Removed this 1/19 since we were having issues with the multiple archivist
					// dialogue lines playing at the same time.
					//AudioEvent.PlayOnce(AudioEventName.Ftue.Archivist.MustBePatient, gameObject);
				}
			}
		}

		protected void OnSdkReady()
		{
			// no further initialization needed if FTUE is complete
			if (FtueDataController.IsFtueComplete(FtueType.Intro))
			{
				return;
			}

			// adding controller
			saberPeripheral = new ControllerPeripheral(VisionSDK.ControllerName,
			                                           ControllerTransform,
			                                           null,
			                                           container.GetSavedSaberColorID());
			Sdk.Connections.AddPeripheral(saberPeripheral);

			// adding beacon
			Sdk.Tracking.OnBeaconStateChange += OnBeaconStateChange;

			// checking beacon signal
			if (Sdk.Tracking.IsBeaconTracked)
			{
				beaconSeen = true;

				// playing audio - signal acquired
				AudioEvent.Play(AudioEventName.Ftue.Computer.SignalAcquired, gameObject);
			}
			else
			{
				// playing audio - searching for signal
				AudioEvent.Play(AudioEventName.Ftue.Computer.AcquireSignal, gameObject);
			}

			// setting input handler
			Sdk.Input.OnButtonDown += OnButtonDown;

			// adding input listeners
			Sdk.Connections.OnPeripheralStateChange += OnPeripheralConnected;
			Sdk.Connections.OnPeripheralStateChange += OnPeripheralDisconnected;

			// adding animation listener
			HolocronAnimations.OnAnimationComplete += OnHolocronEnterAnimationComplete;

			// loading animation sequence config
			StreamingAssetsStorage loader = new StreamingAssetsStorage(Game.ForceVision, null);
			loader.LoadStreamingAssetsText(AnimationConfigFile, OnConfigLoaded, true);

			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.MusicStart, gameObject);
		}

		protected void OnSdkFrameUpdate(object sender, EventArgs eventArguments)
		{
			if (shouldTrackTime)
			{
				currentTime += Time.deltaTime;

				if (currentTime >= targetTime)
				{
					if (onTimerCompleteHandler != null)
					{
						onTimerCompleteHandler.Invoke();
					}

					EndTimer();
				}
			}

			if (isFtueGalaxyMapDisplayed)
			{
				interval += Time.deltaTime;

				if (interval > PlanetSelectionDelay)
				{
					// playing planet selection dialog from archivist when idle
					AudioEvent.Play(AnimationClip_ArchivistSelectNaboo, HolocronContainer);

					/*
					sequence.Play(FtueType.GalaxyMap, AnimationEventName.ArchivistGoAhead);
					*/

					isFtueGalaxyMapDisplayed = false;
				}
			}
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