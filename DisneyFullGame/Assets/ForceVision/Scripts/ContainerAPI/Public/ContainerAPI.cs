using System;
using System.Collections.Generic;
using Disney.ForceVision;
using Disney.Vision;
using UnityEngine;
using UnityEngine.SceneManagement;
using SG.Lonestar;
using BSG.SWARTD;
using Disney.AssaultMode;
using Disney.HoloChess;
using System.Linq;

namespace Disney.ForceVision
{
	public enum MedalType
	{
		Combat,
		Insight,
		Leadership,
		AdvancedCombat,
		Mastery
	}

	public enum JediRank
	{
		Initiate,
		Padawan,
		Knight,
		Master
	}

	public class ContainerAPI : IDisposable
	{
		/// <summary>
		/// All progression unlocked. Should only be settable to true in non-retail builds
		/// </summary>
		#if IS_DEMO_BUILD || !RC_BUILD
		public static bool AllProgressionUnlocked = false;
		#else
		public static const bool AllProgressionUnlocked = false;
		#endif

		#region Public Systems

		/// <summary>
		/// Native Bridge, for events from Native.
		/// </summary>
		/// <value>The native bridge.</value>
		public NativeBridge NativeBridge { get; private set; }

		/// <summary>
		/// Access into Streaming Assets for a given Game
		/// </summary>
		/// <value>The streaming assets.</value>
		public IStreamingAssetsStorage StreamingAssets { get; private set; }

		/// <summary>
		/// Access into Persistent Data for a given Game.
		/// </summary>
		/// <value>The persistent data.</value>
		public IPersistentDataStorage PersistentData { get; private set; }

		/// <summary>
		/// Gets the cdn asset loader API.
		/// </summary>
		/// <value>The cdn asset loader API.</value>
		public Disney.ForceVision.Internal.CdnAssetLoaderApi cdnAssetLoaderApi { get; private set; }

		/// <summary>
		/// Access into Player Prefs for a given Game.
		/// </summary>
		/// <value>The player prefs.</value>
		public IPlayerPrefsStorage PlayerPrefs { get; private set; }

		/// <summary>
		/// </summary>
		/// Native Settings, used to get and set information in native.
		/// <value>The native settings.</value>
		public INativeSettings NativeSettings { get; private set; }

		/// <summary>
		/// The name of the kylo saber.
		/// </summary>
		public string KyloSaberName = "DIS_SAB_REY_01";

		/// <summary>
		/// Playing dark side.
		/// </summary>
		public bool PlayingDarkSide = false;

		/// <summary>
		/// Playing dark side.
		/// </summary>
		public bool PlayingDarkSideWithWrongSaber = false;

		#endregion

		#region Private Properties

		private const string ProgressionFile = "progression.json";
		private static bool disposed = false;
		private static AssaultAPI assaultApi;
		private static DuelAPI duelApi;
		private static TDAPI towerApi;
		private static HolochessAPI chessApi;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.ContainerAPI"/> class.
		/// </summary>
		/// <param name="game">Game.</param>
		public ContainerAPI(Game game)
		{
			// We need a MonoBehaviour for Native to call into as well for Streaming Asset loading.
			GameObject bridge = GameObject.Find("ForceVisionNativeBridge") ?? new GameObject("ForceVisionNativeBridge",
			                                                                                 typeof(NativeBridge));
			NativeBridge = bridge.GetComponent<NativeBridge>();

			// Data Storage / Loading
			StreamingAssets = new StreamingAssetsStorage(game, NativeBridge);
			PersistentData = new PersistentDataStorage(game);
			PlayerPrefs = new PlayerPrefsStorage(game);
			cdnAssetLoaderApi = new Disney.ForceVision.Internal.CdnAssetLoaderApi(NativeBridge);

			// Native Settings
			NativeSettings = new NativeSettings();

			#if UNITY_ANDROID && !UNITY_EDITOR
			NativeBridge.StartLowMemoryPoll(NativeSettings);
			#endif

		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Disney.ForceVision.ContainerAPI"/> is reclaimed by garbage collection.
		/// </summary>
		~ContainerAPI()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="Disney.Vision.VisionSDK"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Disney.Vision.VisionSDK"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Disney.Vision.VisionSDK"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Disney.Vision.VisionSDK"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Disney.Vision.VisionSDK"/> was occupying.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		protected virtual void Dispose(bool disposing)
		{
			// Already been here once.
			if (disposed)
			{
				return;
			}

			// This is only when manually calling dispose(), used to cleanup managed objects (things the GC knows about)
			if (disposing)
			{
				if (NativeBridge != null)
				{
					NativeBridge.Destroy();

					UnityEngine.Object.Destroy(NativeBridge.gameObject);
					NativeBridge = null;
				}
			}

			// Cleanup any unmanaged objects (things like events, file handles, connections) here
			StreamingAssets = null;
			PersistentData = null;
			PlayerPrefs = null;

			// Done
			disposed = true;
		}

		#endregion

		#region Public Methods

		public ColorID GetSavedSaberColorID()
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);

			// Default if never set.
			ColorID color = ColorID.BLUE;

			if (playerPrefs.PrefKeyExists(Constants.SaberColorPlayerPrefKey))
			{
				color = (ColorID)playerPrefs.GetPrefInt(Constants.SaberColorPlayerPrefKey);
			}

			return color;
		}

		public bool UseSpatialization()
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);

			if (playerPrefs.PrefKeyExists(Constants.UseSpatialization))
			{
				return playerPrefs.GetPrefInt(Constants.UseSpatialization) == 1;
			}

			return false;
		}

		public void SetSpatialization(bool enabled)
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			playerPrefs.SetPrefInt(Constants.UseSpatialization, (enabled) ? 1 : 0);
		}

		public static bool UseProfileSelection()
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);

			if (playerPrefs.PrefKeyExists(Constants.UseProfileSelection))
			{
				return playerPrefs.GetPrefInt(Constants.UseProfileSelection) == 1;
			}

			return false;
		}

		public static void SaveProfileSelectionEnabled(bool enabled)
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			playerPrefs.SetPrefInt(Constants.UseProfileSelection, (enabled) ? 1 : 0);
		}

		#if IS_DEMO_BUILD
		public static void LoadDemoOptions()
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			if (playerPrefs.PrefKeyExists(Constants.AllProgressionUnlocked))
			{
				AllProgressionUnlocked = playerPrefs.GetPrefInt(Constants.AllProgressionUnlocked) == 1;
			}
			if (playerPrefs.PrefKeyExists(Constants.GoProMode))
			{
				Disney.Vision.Internal.XimmerseTracker.UseGoProCameras = playerPrefs.GetPrefInt(Constants.GoProMode) == 1;
			}
		}

		public static void SaveAllProgressionUnlocked(bool enabled)
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			playerPrefs.SetPrefInt(Constants.AllProgressionUnlocked, (enabled) ? 1 : 0);
		}

		public static void SaveGoProMode(bool enabled)
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			playerPrefs.SetPrefInt(Constants.GoProMode, (enabled) ? 1 : 0);
		}
		#endif

		public void SetSavedSaberColorID(int colorID)
		{
			PlayerPrefsStorage playerPrefs = new PlayerPrefsStorage(Game.ForceVision);
			playerPrefs.SetPrefInt(Constants.SaberColorPlayerPrefKey, colorID);
		}

		public static bool IsMedalUnlocked(MedalType medal)
		{
			if (AllProgressionUnlocked)
			{
				return true;
			}

			SG.Lonestar.Inventory.BattleProgress progress = GetDuelApi().Progress;

			switch (medal)
			{
			// All Duelists on Hard (Not Archivist)
				case MedalType.AdvancedCombat:
					bool maul = progress.HasCompleted(DuelAPI.Duelist.DarthMaul, 3);
					bool sis = progress.HasCompleted(DuelAPI.Duelist.SeventhSister, 3);
					bool inq = progress.HasCompleted(DuelAPI.Duelist.GrandInquisitor, 3);
					bool vader = progress.HasCompleted(DuelAPI.Duelist.DarthVader, 3);
					bool ren = progress.HasCompleted(DuelAPI.Duelist.KyloRen, 3);
					return (maul && sis && inq && vader && ren);

			// Defeat Archivist on Med.
				case MedalType.Combat:
					return progress.HasCompleted(DuelAPI.Duelist.Archivist, 2);

			// Beat Holochess on the Core planet, 3rd pillar
				case MedalType.Insight:
					return GetChessApi().RatingForStage(PlanetType.Core, 3) > 0;

			// Only 1 level for TD
				case MedalType.Leadership:
					return GetTDAPI().IsBattleUnlocked(TDAPI.Battles.Final_1);

			// Beat the archivist on hard
				case MedalType.Mastery:
					return progress.HasCompleted(DuelAPI.Duelist.Archivist, 3);
			}

			return false;
		}

		public static JediRank GetJediRank()
		{
			if (IsMedalUnlocked(MedalType.Mastery))
			{
				return JediRank.Master;
			}

			SG.Lonestar.Inventory.BattleProgress progress = GetDuelApi().Progress;

			if (progress.HasCompleted(DuelAPI.Duelist.Archivist, 2))
			{
				return JediRank.Knight;
			}

			if (progress.HasCompleted(DuelAPI.Duelist.KyloRen, 1))
			{
				return JediRank.Padawan;
			}

			return JediRank.Initiate;
		}

		public static bool IsPlanetLocked(PlanetType planet)
		{
			if (AllProgressionUnlocked)
			{
				return false;
			}

			SG.Lonestar.Inventory.BattleProgress progress = GetDuelApi().Progress;

			// TODO: I don't like having this is code, but we don't have a better way just yet.
			switch (planet)
			{
				case PlanetType.Naboo:
					return false;

				case PlanetType.Garel:
					return !progress.HasCompleted(DuelAPI.Duelist.DarthMaul, 1);

				case PlanetType.Lothal:
					return !progress.HasCompleted(DuelAPI.Duelist.SeventhSister, 1);

				case PlanetType.Hoth:
					return !progress.HasCompleted(DuelAPI.Duelist.GrandInquisitor, 1);

				case PlanetType.Takodana:
					return !progress.HasCompleted(DuelAPI.Duelist.DarthVader, 1);

				case PlanetType.Core:
					return !progress.HasCompleted(DuelAPI.Duelist.KyloRen, 2);
			}

			return false;
		}

		public static bool IsLevelLocked(PillarConfig config, int difficulty)
		{
			if (AllProgressionUnlocked)
			{
				return false;
			}

			// If null, check difficulty
			if (config.PreviousConfig == null)
			{
				// Easy is unlocked so is holochess and tower defense.
				if (difficulty == 1 || config.Game == Game.HoloChess || config.Game == Game.TowerDefense)
				{
					return false;
				}

				// Only Assault can get here.
				if (config.Game == Game.Assault)
				{
					return !GetDuelApi().Progress.HasCompleted(DuelAPI.Duelist.KyloRen, difficulty - 1);
				}

				return false;
			}

			// Hard Core Duel Track Exception, must have all 3 medals
			if (difficulty == 3 && config.Planet == PlanetType.Core && config.Game == Game.Assault && config.PillarNumber == 1)
			{
				return !(IsMedalUnlocked(MedalType.Insight) && IsMedalUnlocked(MedalType.Leadership) && IsMedalUnlocked(MedalType.AdvancedCombat));
			}

			return !IsLevelComplete(config.PreviousConfig, difficulty);
		}

		public static bool IsLevelComplete(PillarConfig config, int difficulty)
		{
			if (AllProgressionUnlocked)
			{
				return true;
			}

			switch (config.Game)
			{
				case Game.Assault:
					return (GetAssaultApi().RatingForStage(config, difficulty) > 0);

				case Game.Duel:
					return GetDuelApi().Progress.HasCompleted(config.Duelist, difficulty);

				case Game.HoloChess:
					return (GetChessApi().RatingForStage(config) > 0);

				case Game.TowerDefense:
					return GetTDAPI().DidEarnEmblem(config.Battle, TDAPI.Emblems.Victory);
			}

			return true;
		}

		public static int GetLevelAchievements(PillarConfig config, int difficulty)
		{
			switch (config.Game)
			{
			// 1 = win, 2 = hits under 15, 3 = hits under 10
				case Game.Assault:
					return GetAssaultApi().RatingForStage(config, difficulty);

				case Game.Duel:
					return DuelRating.RatingForStage(config, difficulty);

			// 1 = primary, 2 = secondary
				case Game.TowerDefense:
					if (GetTDAPI().DidEarnEmblem(config.Battle, TDAPI.Emblems.Secondary))
					{
						return 2;
					}

					if (GetTDAPI().DidEarnEmblem(config.Battle, TDAPI.Emblems.Victory))
					{
						return 1;
					}
					break;

				case Game.HoloChess:
					return GetChessApi().RatingForStage(config);
			}

			return 0;
		}

		public void LoadNextScene(bool pillarView, bool? victory = null)
		{
			bool interstitial = (ArchivistInterstitialController.Triggers.Count > 0);

			// Random Loss Event for Duel when no other events.
			if (victory == false && !interstitial && MenuAudioController.Triggers.Count == 0 && MenuController.DeepLinkToGame == Game.Duel)
			{
				MenuAudioController.Triggers.Add(Constants.RandomDuelLoss);
			}

			MenuController.DeepLinkOnLoad = pillarView;

			// No Progression, skip it all
			if (AllProgressionUnlocked)
			{
				ArchivistInterstitialController.Triggers.Clear();
				MenuAudioController.Triggers.Clear();
				ProxyReload.SceneToLoad = "Main";
				SceneManager.LoadScene("ReloadProxyScene");
				return;
			}

			if (!interstitial)
			{
				AkSoundEngine.StopAll();
			}

			ProxyReload.SceneToLoad = (interstitial) ? "ArchivistInterstitial" : (PlayingDarkSide ? "DarkSideMain" : "Main");
			SceneManager.LoadScene("ReloadProxyScene");
		}

		/// <summary>
		/// Sets the "beaten" flag for a level.
		/// </summary>
		/// <param name="pillarConfig">Pillar config.</param>
		/// <param name="difficulty">Difficulty.</param>
		/// <param name="hitsTaken">Hits taken.</param>
		public static void SetLevelBeaten(PillarConfig pillarConfig, int difficulty = 1, int hitsTaken = 0)
		{
			PersistentDataStorage data = new PersistentDataStorage(Game.ForceVision);
			ProgressionData progressionData = GetProgressionData(data);

			progressionData.LevelsBeat.Add(new ProgressionLevel(pillarConfig, difficulty, hitsTaken));
			data.SaveSerialized(ProgressionFile, progressionData);
		}

		public void FinishLevel(bool victory)
		{
			// Clear Triggers
			ArchivistInterstitialController.Triggers.Clear();
			MenuAudioController.Triggers.Clear();

			// If this is the first time you won this level
			bool firstTimeBeat = false;
			bool earnedInsight = false;
			bool earnedLeadership = false;
			bool earnedAdvancedCombat = false;

			// Level we just played
			PillarConfig lastPlayedLevel = MenuController.ConfigToLoad;
			Difficulty lastPlayedDifficulty = MenuController.DifficultyToLoad;
			PersistentDataStorage data = new PersistentDataStorage(Game.ForceVision);
			ProgressionData progressionData = GetProgressionData(data);

			// Did not come from a game
			if (lastPlayedLevel == null || progressionData == null)
			{
				LoadNextScene(false, false);
				return;
			}

			// Just earned a medal
			if (!progressionData.InsightMedalUnlocked && IsMedalUnlocked(MedalType.Insight))
			{
				earnedInsight = true;
				progressionData.InsightMedalUnlocked = true;
			}

			if (!progressionData.LeadershipMedalUnlocked && IsMedalUnlocked(MedalType.Leadership))
			{
				earnedLeadership = true;
				progressionData.LeadershipMedalUnlocked = true;
			}

			if (!progressionData.CombatUnlocked && IsMedalUnlocked(MedalType.Combat))
			{
				progressionData.CombatUnlocked = true;
			}

			if (!progressionData.AdvancedCombatUnlocked && IsMedalUnlocked(MedalType.AdvancedCombat))
			{
				earnedAdvancedCombat = true;
				progressionData.AdvancedCombatUnlocked = true;
			}

			if (!progressionData.MasteryUnlocked && IsMedalUnlocked(MedalType.Mastery))
			{
				progressionData.MasteryUnlocked = true;
			}

			data.SaveSerialized(ProgressionFile, progressionData);

			// Check for first time.
			if (victory)
			{
				ProgressionLevel level = progressionData.LevelsBeat.Find(check => (check.LevelHash == lastPlayedLevel.GetHashCode() && (int)lastPlayedDifficulty == check.Difficulty));
				int hitsTaken = 0;
				if (lastPlayedLevel.Game == Game.Duel && GetDuelApi().LastResult != null)
				{
					hitsTaken = GetDuelApi().LastResult.HitsTaken;
					Log.Debug("LastResult found. Setting hitsTaken to " + hitsTaken.ToString());
				}
				else
				{
					Log.Debug("LastResult NOT found. Setting hitsTaken to zero");
				}

				if (level == null)
				{
					SetLevelBeaten(lastPlayedLevel, (int)lastPlayedDifficulty, hitsTaken);
					firstTimeBeat = true;
					if (lastPlayedLevel.IsBonusPlanet)
					{
						PlanetLineController.BeatFirstTime = false;
						MenuController.UnlockedPillar = true;
					}
					else
					{
						PlanetLineController.BeatFirstTime = true;
					}
				}

				// You did better this time (DUEL only)
				else if (level.HitsTaken > hitsTaken)
				{
					progressionData.LevelsBeat.First(item => item == level).HitsTaken = hitsTaken;
					data.SaveSerialized(ProgressionFile, progressionData);
				}
			}

			#region LOSS TRIGGERS

			// Defeat Archivist and strike down (loss)
			if (lastPlayedDifficulty == Difficulty.Hard && lastPlayedLevel.Duelist == DuelAPI.Duelist.Archivist && !victory)
			{
				ProgressionLevel level = progressionData.LevelsLost.Find(check => (check.LevelHash == lastPlayedLevel.GetHashCode() && (int)lastPlayedDifficulty == check.Difficulty));

				if (GetDuelApi().LastResult != null && GetDuelApi().LastResult.DuelEnd == DuelAPI.DuelEndCondition.ObjectiveFail && level == null)
				{
					progressionData.LevelsLost.Add(new ProgressionLevel(lastPlayedLevel, (int)lastPlayedDifficulty));
					data.SaveSerialized(ProgressionFile, progressionData);

					// Animation Trigger: Defeat Archivist on hard  and strike down
					MenuAudioController.Triggers.Add(Constants.LoseCoreDuelHard);

					LoadNextScene(true, victory);
					return;
				}
			}

			// First Loss based on Config Values
			if (!victory && !string.IsNullOrEmpty(lastPlayedLevel.LostVOTrigger[(int)lastPlayedDifficulty - 1]))
			{
				ProgressionLevel level = progressionData.LevelsLost.Find(check => (check.LevelHash == lastPlayedLevel.GetHashCode() && (int)lastPlayedDifficulty == check.Difficulty));

				// First Time Loss
				if (level == null)
				{
					// Save it
					progressionData.LevelsLost.Add(new ProgressionLevel(lastPlayedLevel, (int)lastPlayedDifficulty));
					data.SaveSerialized(ProgressionFile, progressionData);

					// Audio Trigger:
					/*
					Config: first loss on pillar 1 of assault (easy, medium, and hard), chess, tower for all planets
					Config: First Time win first Strategy battle on Naboo
					Config: First time win first Holochess match on Naboo
					Config: First time defeat last assault mode on each planet (easy and medium)	
					*/
					MenuAudioController.Triggers.Add(lastPlayedLevel.LostVOTrigger[(int)lastPlayedDifficulty - 1]);

					LoadNextScene(true, victory);
					return;
				}
			}

			#endregion

			// Always go back to where you came if you lose
			if (!victory)
			{
				LoadNextScene(true, victory);
				return;
			}

			// Not first time, either go back to where you came, or the next planet
			if (!firstTimeBeat)
			{
				LoadNextScene(true, victory);
				return;
			}

			// Error..
			if (lastPlayedLevel.Interstitial.Length < (int)lastPlayedDifficulty)
			{
				LoadNextScene(false, victory);
				return;
			}

			// They unlocked green for the first time
			if (lastPlayedLevel.Game == Game.Duel && lastPlayedDifficulty == Difficulty.Easy && lastPlayedLevel.Duelist == DuelAPI.Duelist.KyloRen)
			{
				SetSavedSaberColorID((int)ColorID.GREEN);
			}

			// Hard Archivist Let Live
			if (lastPlayedDifficulty == Difficulty.Hard && lastPlayedLevel.Duelist == DuelAPI.Duelist.Archivist)
			{
				SetSavedSaberColorID((int)ColorID.PURPLE);

				// Animation Trigger: Defeat Archivist on hard and let live
				ArchivistInterstitialController.Triggers.Add(Constants.WinCoreDuelHard);

				LoadNextScene(false, victory);
				return;
			}

			bool earnAllTrailMedals = false;

			// Beat the LAST duelist on Hard
			if (earnedAdvancedCombat)
			{
				// Animation Trigger: Config: Defeat Naboo, Garal, Lothel, Hoth, Takodana Duelist on hard
				string audioEvent = lastPlayedLevel.InterstitialTrigger[(int)lastPlayedDifficulty - 1];
				ArchivistInterstitialController.Triggers.Add(audioEvent);

				// Animation Trigger: Earn Advanced Combat Medal
				ArchivistInterstitialController.Triggers.Add(Constants.EarnAdvancedCombat);

				// This will always stack 2, 3 if 2 other medals are unlocked already
				if (IsMedalUnlocked(MedalType.Insight) && IsMedalUnlocked(MedalType.Leadership))
				{
					// Animation Trigger: Earned advanced combat, leadership, Insight medals
					ArchivistInterstitialController.Triggers.Add(Constants.EarnAllTrailMedals);

					// Stack 3 animations, Duelist Beat, Medal Earned, Dark Archivist Unlocked
					progressionData.DarkArchivistUnlocked = true;
					data.SaveSerialized(ProgressionFile, progressionData);
				}

				LoadNextScene(false, victory);
				return;
			}

			// Unlocked Dark Archivist
			if ((earnedLeadership || earnedInsight) && !progressionData.DarkArchivistUnlocked)
			{
				if (IsMedalUnlocked(MedalType.Insight) && IsMedalUnlocked(MedalType.Leadership) && IsMedalUnlocked(MedalType.AdvancedCombat))
				{
					// Animation Trigger: Earned advanced combat, leadership, Insight medals
					earnAllTrailMedals = true;

					progressionData.DarkArchivistUnlocked = true;
					data.SaveSerialized(ProgressionFile, progressionData);
				}
			}

			// Animation Trigger
			/*
				Config: Defeat Ren on easy
				Config: Defeat Archivist on med
				Config: Defeat Core Holochess
				Config: Defeat Core Tower Defense
				Config: Defeat Naboo, Garal, Lothel, Hoth, Takodana Duelist on hard
			*/
			if (lastPlayedLevel.Interstitial[(int)lastPlayedDifficulty - 1])
			{
				string audioEvent = lastPlayedLevel.InterstitialTrigger[(int)lastPlayedDifficulty - 1];
				ArchivistInterstitialController.Triggers.Add(audioEvent);
			}

			// Audio Trigger
			/*
			Config: Defeat Maul Easy
			Config: Win Naboo Holochess Trial
			Config: Win Naboo Strategy Trial
			Config: Defeat Sister on easy
			Config: Win Garel Holochess Trial
			Config: Win Garel Strategy Trial
			Config: Defeat Inquisitor on easy	
			Config: Win Lothal Holochess Trial
			Config: Win Lothal Strategy Trial
			Config: Defeat Vader on easy	
			Config: Win Hoth Holochess Trial
			Config: Win Hoth Strategy Trial
			Config: Win Takodana Holochess
			Config: Win Takodana Strategy
			Config: Defeat Maul on Med	
			Config: Defeat Sister on Med
			Config: Defeat Inquisitor on med	
			Config: Defeat Vader on med	
			Config: Defeat Ren on med
			*/
			else if (!string.IsNullOrEmpty(lastPlayedLevel.InterstitialTrigger[(int)lastPlayedDifficulty - 1]))
			{
				// checking for duel games on bonus planet
				if (lastPlayedLevel.IsBonusPlanet)
				{
					bool hasWonStrategy = towerApi.HasWonBattle(TDAPI.Battles.Crait_3);
					if (lastPlayedLevel.Game == Game.Duel)
					{
						// player has not completed strategy
						if (!hasWonStrategy)
						{
							if (lastPlayedDifficulty == Difficulty.Easy)
							{
								MenuAudioController.Triggers.Add(AudioEventName.Progression.CompleteCraitDuelistAndNotCompleteStrategy);
							}
							else if (lastPlayedDifficulty == Difficulty.Medium)
							{
								MenuAudioController.Triggers.Add(AudioEventName.Progression.CompleteCraitDuelistMedium);
							}
							else if (lastPlayedDifficulty == Difficulty.Hard)
							{
								MenuAudioController.Triggers.Add(AudioEventName.Progression.CompleteCraitDuelistHard);
							}
						}
						// player has completed strategy
						else
						{
							if (lastPlayedDifficulty == Difficulty.Easy)
							{
								ArchivistInterstitialController.Triggers.Add(AudioEventName.Progression.CompleteCraitDuelistAndCompleteStrategy);
							}
						}
					}
					else if (lastPlayedLevel.Game == Game.TowerDefense)
					{
						// checking if duelist complete (easy)
						bool hasWonDuelist = duelApi.Progress.HasCompleted(DuelAPI.Duelist.PraetorianGuards, 1);

						if (hasWonStrategy && !hasWonDuelist)
						{
							MenuAudioController.Triggers.Add(AudioEventName.Progression.CompleteCraitStrategyAndNotCompleteDuelist);
						}
						else if (hasWonStrategy && hasWonDuelist)
						{
							ArchivistInterstitialController.Triggers.Add(AudioEventName.Progression.CompleteCraitDuelistAndCompleteStrategy);
						}
					}
					else if (lastPlayedLevel.Game == Game.Assault && lastPlayedLevel.PillarNumber == 2)
					{
						MenuAudioController.Triggers.Add(lastPlayedLevel.InterstitialTrigger[(int)lastPlayedDifficulty - 1]);
					}
				}
				else
				{
					string audioEvent = lastPlayedLevel.InterstitialTrigger[(int)lastPlayedDifficulty - 1];
					MenuAudioController.Triggers.Add(audioEvent);
				}
			}

			// Make it last
			if (earnAllTrailMedals)
			{
				ArchivistInterstitialController.Triggers.Add(Constants.EarnAllTrailMedals);
			}

			if (lastPlayedLevel.IsBonusPlanet && lastPlayedLevel.Game == Game.Duel)
			{
				LoadNextScene(true, victory);
			}
			else
			{
				LoadNextScene((lastPlayedLevel.PillarNumber < 3), victory);
			}
		}

		public static ProgressionData GetProgressionData(PersistentDataStorage data)
		{
			ProgressionData progressionData;

			if (data.FileExists(ProgressionFile))
			{
				progressionData = data.LoadSerialized<ProgressionData>(ProgressionFile);
			}
			else
			{
				progressionData = new ProgressionData();
			}

			return progressionData;
		}

		public Quality GetQuality()
		{
			QualityController qualityController = new QualityController(GameObject.FindObjectOfType<VisionSDK>());
			return qualityController.GetQuality();
		}

		#endregion

		#region Private Methods

		public static DuelAPI GetDuelApi()
		{
			if (duelApi == null)
			{
				duelApi = new DuelAPI();
			}

			return duelApi;
		}

		private static TDAPI GetTDAPI()
		{
			if (towerApi == null)
			{
				towerApi = TDAPI.GetInstance();
			}

			return towerApi;
		}

		private static HolochessAPI GetChessApi()
		{
			if (chessApi == null)
			{
				chessApi = new HolochessAPI();
			}

			return chessApi;
		}

		private static AssaultAPI GetAssaultApi()
		{
			if (assaultApi == null)
			{
				assaultApi = new AssaultAPI();
			}

			return assaultApi;
		}

		#endregion
	}
}