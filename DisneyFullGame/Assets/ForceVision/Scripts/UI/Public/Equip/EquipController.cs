using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;
using Disney.ForceVision.Internal;
using System;
using Disney.AssaultMode;
using SG.Lonestar;
using SG.Lonestar.Inventory;

namespace Disney.ForceVision
{
	public class EquipController : MonoBehaviour
	{
		public bool UsePrediction = true;
		public GazeWatcher GazeWatcher;
		public Text DuelistTitle;
		public Text DifficultyText;
		public Text PromptText;
		public SimpleCameraFader CameraFader;
		public MainNavigationController MainNav;
		public GameObject Loadout;

		public InventoryItem ForcePower { get; set; }

		public InventoryItem PassivePower1 { get; set; }

		public InventoryItem PassivePower2 { get; set; }

		public VisionSDK Sdk;

		private ContainerAPI container;
		private GazeListener gazeListener;
		private BaseEquipItem currentItem;
		private bool leaving = false;

		private void Start()
		{
			// Our Container API
			container = new ContainerAPI(Game.ForceVision);
			container.NativeBridge.OnLowMemory += OnLowMemory;

			if (MenuController.ConfigToLoad != null)
			{
				DuelistTitle.text = Localizer.Get("PedestalView.Title.LevelName." + MenuController.ConfigToLoad.GetTokenString()).Replace(Environment.NewLine,
				                                                                                                                          " ");

				// Medium on Core is called Easy. Otherwise, assign difficulty like normal
				if (MenuController.ConfigToLoad.Planet == PlanetType.Core && MenuController.DifficultyToLoad == Difficulty.Medium)
				{
					DifficultyText.text = Localizer.Get("General.Name." + PlanetType.Core.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty.Easy");
				}
				else if (MenuController.ConfigToLoad.IsBonusPlanet)
				{
					DifficultyText.text = Localizer.Get("General.Name." + MenuController.ConfigToLoad.BonusPlanet.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty." + MenuController.DifficultyToLoad.ToString());
				}
				else
				{
					DifficultyText.text = Localizer.Get("General.Name." + MenuController.ConfigToLoad.Planet.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty." + MenuController.DifficultyToLoad.ToString());
				}

				PromptText.text = MenuController.ConfigToLoad.Game == Game.Assault ? Localizer.Get("LightsaberDuel.Prompt.SelectForcePower") : Localizer.Get("LightsaberDuel.Prompt.SelectForcePowers");
			}

			// Setup the SDK
			Sdk.SetLogger(new VisionSdkLoggerProxy());
			OnSDKReady();

			Sdk.StereoCamera.UseMagnetometerCorrection = false;

			gazeListener = new GazeListener(new [] { typeof(BaseEquipItem) }, OnItemGazedAt, OnItemGazedOff);
			GazeWatcher.AddListener(gazeListener);
		}

		private void OnDestroy()
		{
			// Remove Input Events
			Sdk.Input.OnButtonUp -= OnButtonUp;

			MainNav.OnMenuShown -= OnMenuShown;
			MainNav.OnMenuHidden -= OnMenuHidden;
			MainNav.OnQuitButton -= OnMenuQuit;
			MainNav.OnRestartButton -= OnMenuRestart;

			// Kill SDK
			Sdk = null;

			// Kill the Container
			container.NativeBridge.OnLowMemory -= OnLowMemory;
			container.Dispose();
			container = null;

			GazeWatcher.RemoveListener(gazeListener);
		}

		public void LaunchGame(AssaultAPI assault)
		{
			leaving = true;

			CameraFader.Fade(Constants.SceneFadeOutTime, () =>
			{
				switch (MenuController.ConfigToLoad.Game)
				{
					case Game.Assault:
						if (MenuController.ConfigToLoad.IsBonusPlanet)
						{
							assault.StartAssaultMode(MenuController.ConfigToLoad.BonusPlanet,
							                         MenuController.ConfigToLoad.PillarNumber,
							                         (int)MenuController.DifficultyToLoad);
						}
						else
						{
							assault.StartAssaultMode(MenuController.ConfigToLoad.Planet,
							                         MenuController.ConfigToLoad.PillarNumber,
							                         (int)MenuController.DifficultyToLoad);
						}
						break;

					case Game.Duel:
						ContainerAPI.GetDuelApi().Launch(MenuController.ConfigToLoad.Duelist, (int)MenuController.DifficultyToLoad);
						break;
				}
			});
		}

		private void OnSDKReady()
		{
			if (MenuController.ConfigToLoad)
			{
				MainNav.Game = MenuController.ConfigToLoad.Game;
			}
			else
			{
				MainNav.Game = Game.Duel;
			}
			MainNav.Setup(Sdk, false);
			MainNav.OnMenuShown += OnMenuShown;
			MainNav.OnMenuHidden += OnMenuHidden;
			MainNav.OnQuitButton += OnMenuQuit;
			MainNav.OnRestartButton += OnMenuRestart;

			// Add the audio Listener
			Sdk.StereoCamera.gameObject.AddComponent<AkAudioListener>();

			// Setup Controllers
			ControllerPeripheral controller = new ControllerPeripheral(VisionSDK.ControllerName, null, null, container.GetSavedSaberColorID());
			controller.UsePositionPrediction = UsePrediction;
			Sdk.Connections.AddPeripheral(controller);

			// Input Events
			Sdk.Input.OnButtonUp += OnButtonUp;
		}

		private void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (leaving)
			{
				return;
			}

			if (currentItem != null && (eventArguments.Button == ButtonType.SaberActivate || eventArguments.Button == ButtonType.HmdSelect || eventArguments.Button == ButtonType.SaberControl))
			{
				currentItem.Clicked();
			}
		}

		private void OnItemGazedAt(object sender, GazeEventArgs eventArguments)
		{
			currentItem = eventArguments.Hit.GetComponent<BaseEquipItem>();
			currentItem.GazedAt();
		}

		private void OnItemGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (currentItem != null)
			{
				currentItem.GazedOff();
			}
		}

		private void OnLowMemory(object sender, EventArgs eventArguments)
		{
			Log.Debug("Low Memory Warning");
		}

		private void OnMenuShown(object sender, EventArgs eventArguments)
		{
			Loadout.SetActive(false);
		}

		private void OnMenuHidden(object sender, EventArgs eventArguments)
		{
			Loadout.SetActive(true);
		}

		private void OnMenuQuit(object sender, EventArgs eventArguments)
		{
			leaving = true;
			CameraFader.Fade(Constants.SceneFadeOutTime, () => container.LoadNextScene(false, true));
		}

		private void OnMenuRestart(object sender, EventArgs eventArguments)
		{
			MainNav.ToggleVisiblity(false);
		}
	}
}