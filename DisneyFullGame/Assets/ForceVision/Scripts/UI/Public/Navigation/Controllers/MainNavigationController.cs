using UnityEngine;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(LookAtTransform))]
	public class MainNavigationController : MonoBehaviour
	{
		public Game Game;
		public GameObject ControllerHolder;
		public BaseController[] Controllers;
		public SaberSyncPopup SaberSyncPopup;
		public HMDConnectPopup HMDConnectPopup;
		public BatteryPopup BatteryPopup;
		public GazeWatcher GazeWatcher;

		public EventHandler<EventArgs> OnMenuShown;
		public EventHandler<EventArgs> OnMenuHidden;
		public EventHandler<EventArgs> OnSaberSyncShown;
		public EventHandler<EventArgs> OnSaberSyncHidden;
		public EventHandler<EventArgs> OnHMDConnectShown;
		public EventHandler<EventArgs> OnHMDConnectHidden;

		public EventHandler<EventArgs> OnRestartButton;
		public EventHandler<EventArgs> OnQuitButton;
		public EventHandler<EventArgs> OnResumeButton;
		public EventHandler<EventArgs> OnSaberColorChanged;

		public bool OnlyThingUsingGazeWatcher { get; set; }

		private VisionSDK sdk;

		public void Start()
		{
			ContainerAPI container = new ContainerAPI(Game.ForceVision);
			UpdateSpatializationSetting(container.UseSpatialization());
		}

		public void Setup(VisionSDK sdk, bool onlyThingUsingGazeWatcher = true)
		{
			this.sdk = sdk;

			OnlyThingUsingGazeWatcher = onlyThingUsingGazeWatcher;

			sdk.Heartbeat.OnApplicationPaused += OnApplicationPaused;
			sdk.Heartbeat.OnApplicationResumed += OnApplicationResumed;

			if (OnlyThingUsingGazeWatcher)
			{
				GazeWatcher.RaycastEnabled = false;
			}

			for (int i = 0; i < Controllers.Length; i++)
			{
				Controllers[i].Setup(sdk, GazeWatcher, Game, this);
			}

			SaberSyncPopup.Setup(sdk, this);
			HMDConnectPopup.Setup(sdk, this);

			if (!Application.isEditor)
			{
				Invoke("CheckForConnections", 0.5f);
			}

			BatteryPopup.Setup(sdk);

			GetComponent<LookAtTransform>().Transform = sdk.StereoCamera.transform;
		}

		private void CheckForConnections()
		{
			HMDConnectPopup.CheckConnection();
		}

		private void Update()
		{
			if (Application.isEditor)
			{
				// in the editor, allow toggling HMD popup via the H key
				if (Input.GetKeyDown(KeyCode.H))
				{
					HMDConnectPopup.ForceShow(true);
				}
				else if (Input.GetKeyUp(KeyCode.H))
				{
					HMDConnectPopup.ForceShow(false);
				}
			}

			// hide all controllers if HMD or saber sync popup is showing
			ControllerHolder.SetActive(!HMDConnectPopup.IsShowing() && !SaberSyncPopup.IsShowing());
		}

		/// <summary>
		/// Toggles the visiblity.
		/// </summary>
		/// <param name="show">If set to <c>true</c> show.</param>
		public void ToggleVisiblity(bool show)
		{
			for (int i = 0; i < Controllers.Length; i++)
			{
				if (Controllers[i] is NavigationController)
				{
					(Controllers[i] as NavigationController).ToggleVisiblity(show);
				}
			}
		}

		public void UpdateSpatializationSetting(bool state)
		{
			if (state)
			{
				AkSoundEngine.PostEvent("Audio_Enable_AR", gameObject);
				AkSoundEngine.PostEvent("Audio_Enable_Room", gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Audio_Bypass_AR", gameObject);
				AkSoundEngine.PostEvent("Audio_Bypass_Room", gameObject);
			}

			GameObject room = GameObject.Find("WwiseGvrAudioRoom");
			if (room != null)
			{
				room.SetActive(state);
			}

			(new ContainerAPI(Game.ForceVision)).SetSpatialization(state);
		}

		public void ClearElements()
		{
			for (int i = 0; i < Controllers.Length; i++)
			{
				if (Controllers[i] is NavigationController)
				{
					(Controllers[i] as NavigationController).ClearElement();
				}
			}
		}

		private void OnDestroy()
		{
			if (sdk != null && sdk.Heartbeat != null)
			{
				sdk.Heartbeat.OnApplicationPaused -= OnApplicationPaused;
				sdk.Heartbeat.OnApplicationResumed -= OnApplicationResumed;
			}
		}

		/// <summary>
		/// When the application is paused.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="arguments">Arguments.</param>
		private void OnApplicationPaused(object sender, EventArgs arguments)
		{
			for (int i = 0; i < Controllers.Length; i++)
			{
				if (Controllers[i] is NavigationController)
				{
					(Controllers[i] as NavigationController).Pause();
				}
			}
		}

		/// <summary>
		/// When the application is resumed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="arguments">Arguments.</param>
		private void OnApplicationResumed(object sender, EventArgs arguments)
		{
			
		}
	}
}