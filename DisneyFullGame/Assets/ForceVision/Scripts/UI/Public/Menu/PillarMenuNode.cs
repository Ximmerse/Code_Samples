using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SG.Lonestar;
using BSG.SWARTD;
using Disney.HoloChess;
using Disney.AssaultMode;

namespace Disney.ForceVision
{
	public class PillarMenuNode : MenuNode
	{
		// 0 = Don't skip, 1 = skip with win, 2 = skip with loss.
		public static int SkipLevelType = 0;

		#region Public Properties

		/// <summary>
		/// Animators for sub objects of pillars.
		/// </summary>
		public List<Animator> SubAnimators = new List<Animator>();

		/// <summary>
		/// The config.
		/// </summary>
		public PillarConfig Config;

		/// <summary>
		/// The pedestal model.
		/// </summary>
		public GameObject PedestalModel;

		/// <summary>
		/// Lock Object
		/// </summary>
		public GameObject LockObject;

		/// <summary>
		/// Pin Object
		/// </summary>
		public GameObject PinObject;

		/// <summary>
		/// The gaze object.
		/// </summary>
		public GameObject SelectedObject;

		/// <summary>
		/// The gaze object.
		/// </summary>
		public GameObject GazedObject;

		/// <summary>
		/// The tint parent.
		/// </summary>
		public GameObject TintParent;

		/// <summary>
		/// The first unlock effect.
		/// </summary>
		public GameObject FirstUnlockEffect;

		/// <summary>
		/// If this pillar is locked or not
		/// </summary>
		/// <value><c>true</c> if locked; otherwise, <c>false</c>.</value>
		public bool Locked { get; set; }

		#endregion

		#region Private Properties

		private Color lockedColor = new Color(0.05f, 0.73f, 0.95f);
		private Color unlockedColor = new Color(1.0f, 0.73f, 0.24f);

		#endregion

		#region Public Methods

		/// <summary>
		/// Launch the game!
		/// </summary>
		public void Launch()
		{
			// Stop ALl before loading into a game
			AkSoundEngine.StopAll();

			// Audio - MAP_UI_GalaxyMap_SelectGame - playing selected game audio
			AudioEvent.Play(AudioEventName.GalaxyMap.SelectGame, gameObject);
			AudioEvent.Play(AudioEventName.GalaxyMap.TransitionStinger, gameObject);

			//track scene load time 
			KpiTracking.TrackSceneLoadTime();

			MenuController.ConfigToLoad = Config;

			if (SkipLevelType > 0)
			{
				if (SkipLevelType == 1)
				{
					switch (Config.Game)
					{
						case Game.Duel:
							DuelAPI api = ContainerAPI.GetDuelApi();
							for (int i = 0; i < api.Progress.Battles.Length; i++)
							{
								if (api.Progress.Battles[i].DuelistIdentifier == Config.Duelist)
								{
									api.Inventory.ForcePowers.GiveAllItems();
									api.Inventory.ForcePowers.SaveToDisk();
									api.Inventory.PassiveAbilities.GiveAllItems();
									api.Inventory.PassiveAbilities.SaveToDisk();
									api.Progress.SetVictory(api.Progress.Battles[i], (int)MenuController.DifficultyToLoad);
									api.Progress.SaveToDisk();
									break;
								}
							}
							break;

						case Game.Assault:
							AssaultAPI assaultApi = new AssaultAPI();
							if (Config.IsBonusPlanet)
							{
								assaultApi.SetRatingForStage(Config.BonusPlanet, Config.PillarNumber, (int)MenuController.DifficultyToLoad, 1);
							}
							else
							{
								assaultApi.SetRatingForStage(Config.Planet, Config.PillarNumber, (int)MenuController.DifficultyToLoad, 1);
							}
							break;

						case Game.HoloChess:
							(new HolochessAPI()).CompleteLevel(Config, true);
							break;

						case Game.TowerDefense:
							TDAPI.GetInstance().DebugWinBattle(Config.Battle);
							break;
					}
				}

				(new ContainerAPI(Game.ForceVision)).FinishLevel(SkipLevelType == 1);

				return;
			}

			ForceVisionAnalytics.LogGameStart();

			switch (Config.Game)
			{
				case Game.Duel:
				case Game.Assault:
					DuelAPI duelApi = ContainerAPI.GetDuelApi();
					int force = duelApi.Inventory.ForcePowers.GetOwnedItems().Count;
					int passive = duelApi.Inventory.PassiveAbilities.GetOwnedItems().Count;

					// They have powers to equip
					if (force > 0 || passive > 0)
					{
						SceneManager.LoadScene("Equip");
					}
					else
					{
						if (Config.Game == Game.Assault)
						{
							AssaultAPI assaultApi = new AssaultAPI();
							if (MenuController.ConfigToLoad.IsBonusPlanet)
							{
								assaultApi.StartAssaultMode(MenuController.ConfigToLoad.BonusPlanet, MenuController.ConfigToLoad.PillarNumber, (int)MenuController.DifficultyToLoad);
							}
							else
							{
								assaultApi.StartAssaultMode(MenuController.ConfigToLoad.Planet, MenuController.ConfigToLoad.PillarNumber, (int)MenuController.DifficultyToLoad);
							}
							/*
							assaultApi.StartAssaultMode(MenuController.ConfigToLoad.Planet,
							                            MenuController.ConfigToLoad.PillarNumber,
							                            (int)MenuController.DifficultyToLoad);
							*/
						}
						else
						{
							duelApi.Launch(MenuController.ConfigToLoad.Duelist, (int)MenuController.DifficultyToLoad);
						}
					}
					break;

				case Game.TowerDefense:
					TDAPI.GetInstance().LaunchBattle(Config.Battle);
					break;

				case Game.HoloChess:
					HolochessAPI holoChessApi = new HolochessAPI();
					holoChessApi.StartHolochess(Config);
					break;
			}
		}

		/// <summary>
		/// Animate all the objects on the pillar.
		/// </summary>
		public void AnimatePillar()
		{
			foreach (Animator animator in SubAnimators)
			{
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("frozen"))
				{
					animator.Play("playing");
					//Play sound when pillar is clicked
					AudioEvent.Play("MAP_SFX_Pillar_" + Config.name, gameObject);
				}
			}
		}

		#endregion

		#region Protected Methods

		public void UpdateState(Difficulty difficulty)
		{
			// Exception when playing on hard (Not Core)
			if (Config.Planet != PlanetType.Core && difficulty == Difficulty.Hard && (Config.Game == Game.Duel || Config.Game == Game.Assault) && Config.PillarNumber == 1 && ContainerAPI.GetDuelApi().Progress.HasCompleted(DuelAPI.Duelist.Archivist, 2))
			{
				Locked = false;
			}
			else
			{
				Locked = (Config != null) && ContainerAPI.IsLevelLocked(Config, (int)difficulty);
			}

			// Locked
			if (LockObject != null)
			{
				LockObject.SetActive(Locked);
			}

			if (TintParent != null)
			{
				Image[] tintImages = TintParent.GetComponentsInChildren<Image>();

				for (int i = 0; i < tintImages.Length; i++)
				{
					tintImages[i].color = (Locked) ? lockedColor : unlockedColor;
				}
			}

			// The Shaders
			GameObject root = (PedestalModel != null) ? PedestalModel : gameObject;

			SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer renderer in renderers)
			{
				if (renderer == null || renderer.materials == null)
				{
					continue;
				}

				foreach (Material material in renderer.materials)
				{
					if (material.HasProperty("_DESATURATE"))
					{
						material.SetFloat("_DESATURATE", (Locked) ? 1 : 0);
						material.shader = material.shader;
					}
				}
			}

			// Pin
			if (PinObject != null)
			{
				switch (Config.Game)
				{
					case Game.Assault:
					case Game.Duel:
						if (PinController.GuardianPinConfig != null)
						{
							PinObject.SetActive(PinController.GuardianPinConfig.Contains(Config) && (int)difficulty == PinController.GuardianPinConfigDifficulty);
						}
						break;

					case Game.HoloChess:
						PinObject.SetActive(Config == PinController.ConsularPinConfig);
						break;

					case Game.TowerDefense:
						PinObject.SetActive(Config == PinController.CommanderPinConfig);
						break;
				}
			}
		}

		protected override void OnFocusUpdated(bool state)
		{
			base.OnFocusUpdated(state);

			if (!state && !Selected)
			{
				GazedObject.SetActive(false);
			}
			else
			{
				GazedObject.SetActive(true);
			}

			if (state && OnNodeFocused != null)
			{
				OnNodeFocused(this, new MenuNodeEventArgs(NodeType, Config.Game));
			}
		}

		protected override void OnIdleTimeReached()
		{
			if (OnNodeIdle != null)
			{
				OnNodeIdle(this, new MenuNodeEventArgs(NodeType, Config.Game));
			}
		}

		protected override void OnSelectUpdated(bool state)
		{
			SelectedObject.SetActive(state);

			if (!state)
			{
				GazedObject.SetActive(false);
			}

			if (state && OnNodeSelected != null)
			{
				OnNodeSelected(this, new MenuNodeEventArgs(NodeType, Config.Game));
			}
		}

		#endregion
	}
}