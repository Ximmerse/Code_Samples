using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	/// <summary>
	/// Audio event name constants.
	/// </summary>
	public static class AudioEventName
	{
		public static class Menu
		{
			/// <summary>
			/// The open holocron master event.
			/// </summary>
			public const string OpenHolocronMaster	= "STORY_DX_Arch_Wisdom_ProgressionUI";

			/// <summary>
			/// The holocron mastery first time.
			/// </summary>
			public const string HolocronMasteryFirstTime = "MAP_DX_Arch_Progression_002_510";
		}

		public static class Holocron
		{
			/// <summary>
			/// The first time appears event.
			/// </summary>
			public const string FirstTimeAppears = "FTUE_DX_Arch_MapIntro_000_300";

			/// <summary>
			/// The holocron appears event.
			/// </summary>
			public const string Appears = "MAP_SFX_Holocron_Appears";

			/// <summary>
			/// The holocron opens event.
			/// </summary>
			public const string Opens = "MAP_SFX_Holocron_Open";

			/// <summary>
			/// All spin idle event for the holocron.
			/// </summary>
			public const string AllSpinIdle = "MAP_SFX_Holocron_Anim_AllSpinIdle";

			/// <summary>
			/// All spin intro event for the holocron.
			/// </summary>
			public const string AllSpinIntro = "MAP_SFX_Holocron_Anim_AllSpinIntro";

			/// <summary>
			/// The closed idle event for the holocron.
			/// </summary>
			public const string ClosedIdle = "MAP_SFX_Holocron_Anim_ClosedIdle";

			/// <summary>
			/// The corner expand event for the holocron.
			/// </summary>
			public const string CornerExpand = "MAP_SFX_Holocron_Anim_CornerExpand";

			/// <summary>
			/// The corner expand idle event for the holocron.
			/// </summary>
			public const string CornerExpandIdle	= "MAP_SFX_Holocron_Anim_CornerExpandIdle";

			/// <summary>
			/// The corner return event for the holocron.
			/// </summary>
			public const string CornerReturn = "MAP_SFX_Holocron_Anim_CornerReturn";

			/// <summary>
			/// The corner spin idle event for the holocron.
			/// </summary>
			public const string CornerSpinIdle = "MAP_SFX_Holocron_Anim_CornerSpinIdle";

			/// <summary>
			/// The corner spin stop event for the holocron.
			/// </summary>
			public const string CornerSpinStop = "MAP_SFX_Holocron_Anim_CornerSpinStop";

			/// <summary>
			/// The corner stop idle event for the holocron.
			/// </summary>
			public const string CornerStopIdle = "MAP_SFX_Holocron_Anim_CornerStopIdle";

			/// <summary>
			/// The corner turn event for the holocron.
			/// </summary>
			public const string CornerTurn = "MAP_SFX_Holocron_Anim_CornerTurn";

			/// <summary>
			/// The enter event for the holocron.
			/// </summary>
			public const string Enter = "MAP_SFX_Holocron_Anim_Enter";

			///<summary>
			/// The holcron is pressed and drops for the FTUE
			/// </summary>
			public const string FTUEHolocronDrop = "MAP_UI_Holocron_FTUE_Pressed";
		}

		public static class GalaxyMap
		{
			/// <summary>
			/// The appears event for the galaxy map.
			/// </summary>
			public const string Appears = "MAP_SFX_GalaxyMap_TurnOn";

			/// <summary>
			/// SFX for unlocking things
			/// </summary>
			public const string PlanetUnlock = "MAP_SFX_PlanetUnlock";
			public const string PillarUnlock = "MAP_SFX_PillarUnlock";
			public const string MedalReward = "MAP_SFX_MedalReward";

			/// <summary>
			/// The node highlighted event for the galaxy map.
			/// </summary>
			public const string NodeHighlighted = "MAP_UI_GalaxyMap_NodeHighlight";

			/// <summary>
			/// The node selected event for the galaxy map.
			/// </summary>
			public const string NodeSelected = "MAP_UI_GalaxyMap_NodeSelect";

			/// <summary>
			/// The zoom event for the galaxy map.
			/// </summary>
			public const string Zoom = "MAP_UI_GalaxyMap_Zoom";

			/// <summary>
			/// The challenge opens event for the galaxy map.
			/// </summary>
			public const string ChallengeOpens = "MAP_UI_GalaxyMap_ChallengeOpens";

			/// <summary>
			/// The select game event for the galaxy map.
			/// </summary>
			public const string SelectGame = "MAP_UI_GalaxyMap_SelectGame";

			/// <summary>
			/// The pillar pop up event for the galaxy map.
			/// </summary>
			public const string PillarPopUp = "MAP_UI_GalaxyMap_PillarPopUp";

			/// <summary>
			/// The pillar / planet locked event for the galaxy map.
			/// </summary>
			public const string NodeLocked = "MAP_UI_GalaxyMap_NodeLocked";

			/// <summary>
			/// When an activity is selected on the planet view with the 3 trial pillars.
			/// </summary>
			public const string SelectActivity = "MAP_UI_GalaxyMap_SelectActivity";

			/// <summary>
			/// When a level within a trial is selected.
			/// </summary>
			public const string BeginActivity = "MAP_UI_GalaxyMap_BeginActivity";

			/// <summary>
			/// When an activity is launched.
			/// </summary>
			public const string LaunchActivity = "MAP_UI_GalaxyMap_LaunchActivity";

			/// <summary>
			/// When the user gaze back to the planet view and selects to go back.
			/// </summary>
			public const string SelectBack = "MAP_UI_GalaxyMap_SelectBack";

			/// <summary>
			/// The transition stinger event for the galaxy map.
			/// </summary>
			public const string TransitionStinger	= "MAP_MX_TransitionStinger";

			/// <summary>
			/// The intro music event for the galaxy map.
			/// </summary>
			public const string IntroMusic = "MAP_MX_Music";

			/// <summary>
			/// The player is new and has unlocked Crait.
			/// </summary>
			public const string UnlockCraitNewUser = "MAP_DX_Arch_Crait_000_000";

			/// <summary>
			/// The unlock Crait sound for everyone.
			/// </summary>
			public const string UnlockCraitEveryone = "MAP_DX_Arch_Crait_000_001";

			/// <summary>
			/// The first time that the player selects Crait.
			/// </summary>
			public const string SelectCraitFirstTime = "MAP_DX_Arch_Crait_000_002";

			/// <summary>
			/// The player has selected Crait.
			/// </summary>
			public const string SelectCrait = "MAP_DX_Arch_Crait_000_003";
		}

		public static class Progression
		{
			public const string CompleteCraitDuelistAndCompleteStrategy = "archivistInterstitial_unlockCrait";
			public const string CompleteCraitDuelistAndNotCompleteStrategy = "MAP_DX_Arch_Crait_000_010";
			public const string CompleteCraitDuelistHard = "";
			public const string CompleteCraitDuelistMedium = "MAP_DX_Arch_Crait_000_008";
			public const string CompleteCraitStrategyAndNotCompleteDuelist = "MAP_DX_Arch_Crait_000_011";
		}

		public static class ProgressionUI
		{
			public const string SettingsButton = "MAP_UI_ProgressionUI_SettingsButton";
			public const string HolocronMasteryButton = "MAP_UI_ProgressionUI_HolocronMasteryButton";
			public const string GoToTrial = "MAP_UI_ProgressionUI_GoToTrial_Press";
		}

		public static class Settings
		{
			public const string PopupOpen = "MAP_UI_Settings_PopUp_Open";
			public const string PopUpClose = "MAP_UI_Settings_PopUp_Close";
			public const string Resume = "MAP_UI_Settings_Resume";
			public const string Restart = "MAP_UI_Settings_Restart";
			public const string Quit = "MAP_UI_Settings_Quit";
			public const string Toggle3dOn = "MAP_UI_3D_Toggle_On";
			public const string Toggle3dOff = "MAP_UI_3D_Toggle_Off";
		}

		public static class Combat
		{
			public const string Encounter1 = "MAP_UI_Combat_FirstEncounter_Select";
			public const string Encounter2 = "MAP_UI_Combat_SecondEncounter_Select";
			public const string Encounter3 = "MAP_UI_Combat_ThirdEncounter_Select";
		}

		public static class Archivist
		{
			public static class GalaxyMap
			{
				public const string PlanetLockedGeneral = "MAP_DX_Arch_Gate_Locked_General";
				public const string PlanetLockedChess = "MAP_DX_Arch_Trial_Unavailable_Holochess";
				public const string PlanetLockedBoth = "MAP_DX_Arch_Trial_Unavailable_General";
				public const string PlanetLockedTowerDefense = "MAP_DX_Arch_Trial_Unavailable_Strategy";

				public const string FunFactPlanet = "MAP_DX_Arch_FunFact_#planet#";
				public const string UnlockPlanet = "MAP_DX_Arch_UnlockPlanet_#planet#";
				public const string PillarLockedDuel = "MAP_DX_Arch_Gate_Locked_Duel";
				public const string PillarLockedTowerDefense = "MAP_DX_Arch_Gate_Locked_TowerDefense";
				public const string PillarLockedHoloChess = "MAP_DX_Arch_Gate_Locked_HoloChess";

				public const string GalaxyLoadRankInitiate = "MAP_DX_Arch_Opening";
				public const string GalaxyLoadRankPadawan = "MAP_DX_Arch_Opening_Padawans";
				public const string GalaxyLoadRankJediKnight = "MAP_DX_Arch_Opening_JediKnights";
				public const string GalaxyLoadRankJediMaster = "MAP_DX_Arch_Opening_JediMasters";

				public static string[,] DuelPillarUnlock = new string[,] { 

					// Tutorial
					{ "", "", "", "", "", "", "", "" },

					// Easy
 					{					
						"", 
						"STORY_DX_Combat_Easy_900_250",
						"STORY_DX_Combat_Easy_900_260",
						"STORY_DX_Combat_Easy_900_270",
						"STORY_DX_Arch_Combat_Easy_003_270",
						"STORY_DX_Arch_Combat_Easy_003_310",
						"",
						""
					}, 

					// Medium
 					{
						"",
						"STORY_DX_Arch_Combat_Medium_003_360",
						"STORY_DX_Arch_Combat_Medium_003_380",
						"STORY_DX_Arch_Combat_Medium_003_400",
						"STORY_DX_Arch_Combat_Medium_003_420",
						"STORY_DX_Arch_Combat_Medium_003_440",
						"",
						""
					},

					// Hard
					{ "", "", "", "", "", "", "", "" }
				};
			}
		}

		public static class Ftue
		{
			public static class Computer
			{
				public const string AcquireSignal = "FTUE_DX_Com_AquireSignal";
				public const string SignalAcquired = "FTUE_DX_Com_SignalAquired";
			}

			public static class Yoda
			{
				public const string OpenHolocron = "FTUE_DX_Yoda_OpenHolocron";
				public const string ShowSaber = "FTUE_DX_Yoda_ShowSaber";
				public const string ActivateSaber = "FTUE_DX_Yoda_ActivateSaber";
				public const string ActivateSaberIdle = "FTUE_DX_Yoda_ActivateSaberIdle";
			}

			public static class Fx
			{
			}

			public static class Archivist
			{
				public const string FirstOpponent = "FTUE_DX_Arch_Simulation_000_110";
				public const string MustBePatient = "FTUE_DX_Arch_Simulation_000_140";
				public const string DroidInRange = "FTUE_DX_Arch_Simulation_000_150";
				public const string QuiteNatural = "FTUE_DX_Arch_Simulation_000_190";
				public const string GoodBlock = "FTUE_DX_Arch_GoodBlock";
				public const string BlockDestroySlash = "FTUE_DX_Arch_BlockDestroySlash";
				public const string Intro = "FTUE_DX_Arch_Intro";
				public const string FirstHold = "FTUE_DX_Arch_FirstHold";
				public const string Form1 = "FTUE_DX_Arch_Form1";
				public const string UserHitsDroid = "FTUE_DX_Arch_UserHitsDroid";
				public const string GoodHit = "FTUE_DX_Arch_GoodHit";
				public const string HitIdle = "FTUE_DX_Arch_HitIdle";
				public const string MorePractice = "FTUE_DX_Arch_MorePractice";
				public const string HitDuringPractice = "FTUE_DX_Arch_HitDuringPractice";
				public const string HitDemoComplete = "FTUE_DX_Arch_HitDemoComplete";
				public const string BlockIntro = "FTUE_DX_Arch_BlockIntro";
				public const string BlockNoHit = "FTUE_DX_Arch_BlockNoHit";
				public const string BlockHit = "FTUE_DX_Arch_BlockHit";
				public const string BlockFail = "FTUE_DX_Arch_BlockFail";
				public const string BlockIntructsionsComplete = "FTUE_DX_Arch_BlockInstructionComplete";
				public const string BlockTrainingContinued = "FTUE_DX_Arch_BlockTrainingContinued";
				public const string BlockDestroyDeflect = "FTUE_DX_Arch_BlockDestroyDeflect";
				public const string BadBlock = "FTUE_DX_Arch_BadBlock";
				public const string SaberTrainingComplete = "FTUE_DX_Arch_SaberTrainingComplete";
				public const string HolocronTraining = "FTUE_DX_Arch_HolocronTraining";
				public const string PromptSelectNaboo = "FTUE_DX_Arch_MapIntro_000_310";
				public const string PromptSelectNabooIdle = "FTUE_DX_Arch_MapIntro_000_320";
			}

			public static class Stereo
			{
				public const string MenuStart = "MIR_UI_Menu_Start";
				public const string CheckLaunch = "MIR_UI_Check_Launch";
				public const string SearchSaber = "MIR_UI_SearchSaber";
				public const string ZoomIn = "MIR_UI_ZoomIn";
				public const string ExitButton = "MIR_UI_Exit_Button";
				public const string MinusButton = "MIR_UI_Minus_Button";
				public const string PlusButton = "MIR_UI_Plus_Button";
				public const string SubmenuButton = "MIR_UI_SubMenu_Button";
				public const string Swipe = "MIR_UI_Swipe";
				public const string ConnectionNotFound = "MIR_UI_ConnectionNotFound";
				public const string Calibration = "MIR_UI_Calibration";
				public const string CalibrationSuccess = "MIR_UI_Calibration_Success";
				public const string BackgroundMusicStart = "SplashFTUE_MX_Splash_Start";
				public const string BackgroundMusicStop = "SplashFTUE_MX_Splash_Stop";
				public const string BackgroundMusicPause = "SplashFTUE_MX_Splash_Pause";
				public const string BackgroundMusicResume = "SplashFTUE_MX_Splash_Resume";
			}

			public static class Droid
			{
				public const string Roger = "AM_DX_BattleDroid_RogerRoger";
			}

			public const string MusicStart = "FTUE_MX_Play";
			public const string MusicStop = "FTUE_MX_Stop";
			public const string MusicEnding = "FTUE_MX_EndingSection";
		}
	}
}