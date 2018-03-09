using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SG.Lonestar;

namespace Disney.ForceVision
{
	public class CurvedUIController : MonoBehaviour
	{
		public Animator CurvedUIAnimator;

		[Header("Difficulty Holder")]
		public GameObject DifficultyHolder;

		[Header("Medals")]
		public GameObject CombatMedal;
		public GameObject LeadershipMedal;
		public GameObject InsightMedal;

		[Header("Left Side Mode Holders")]
		public GameObject LeftTrailMode;
		public GameObject LeftPillarMode;
		public GameObject LeftLocked;
		public GameObject LeftUnlocked;

		[Header("Right Side Mode Holders")]
		public GameObject RightTrailMode;
		public GameObject RightPillarMode;

		[Header("Difficulty Buttons")]
		public CurvedUIButton EasyButton;
		public CurvedUIButton MediumButton;
		public CurvedUIButton HardButton;
		public Text MediumButtonText;

		[Header("Difficulty Positions & Rotations")]
		public Vector3 MedBtnNormalPos;
		public Vector3 MedBtnCorePos;
		public Vector3 MedBtnNormalRot;
		public Vector3 MedBtnCoreRot;
		public Vector3 HardBtnNormalRot;
		public Vector3 HardBtnCoreRot;

		[Header("Launch Button")]
		public GameObject LaunchButton;

		[Header("Pillar Mode Items")]
		public Text LevelName;
		public Text Description;
		public Text LockedDescription;
		public GameObject AchievementOne;
		public GameObject AchievementTwo;
		public GameObject AchievementThree;
		public GameObject AchievementOneComplete;
		public GameObject AchievementTwoComplete;
		public GameObject AchievementThreeComplete;
		public Text AchievementOneDescription;
		public Text AchievementTwoDescription;
		public Text AchievementThreeDescription;

		[Header("Trail Mode Items")]
		public Text TrailName;
		public Text TrailDescription;
		public Text Lesson;

		[Header("Curved UI Backgrounds")]
		public GameObject BackgroundDefault;
		public GameObject BackgroundEasy;
		public GameObject BackgroundMedium;
		public GameObject BackgroundHard;

		public Difficulty CurrentDifficulty { get; set; }

		private CurvedUIButton currentButton;
		private PillarMenuNode[] pillars;
		private PillarMenuNode currentMenuNode;
		private SurfaceMenuNode currentSurfaceNode;
		private PlanetType currentPlanet;
		private BonusPlanetType? currentBonusPlanet = null;

		/// <summary>
		/// Setup for Trail Mode
		/// </summary>
		/// <param name="menuNode">Menu node.</param>
		/// <param name = "planet"></param>
		/// <param name = "autoSelectDifficulty"></param>
		public void Setup(SurfaceMenuNode menuNode, PlanetType planet, bool autoSelectDifficulty = true)
		{
			currentSurfaceNode = menuNode;
			currentPlanet = planet;
			currentBonusPlanet = null;
			currentMenuNode = null;
			pillars = menuNode.transform.GetComponentsInChildren<PillarMenuNode>(true);

			if (autoSelectDifficulty)
			{
				SetupDifficulty(menuNode.LaunchGame);
			}

			SetupMode(true);
			SetupGame(menuNode.LaunchGame);

			CurvedUIAnimator.gameObject.SetActive(true);
			CurvedUIAnimator.Play("bentScreen_hiddenToPrimary");

			TrailName.text = Localizer.Get("PedestalView.Title.TrialName." + planet + "." + menuNode.LaunchGame);
			TrailDescription.text = Localizer.Get("PedestalView.Description.TrialObjective." + planet + "." + menuNode.LaunchGame);

			if (menuNode.LaunchGame == Game.Duel || menuNode.LaunchGame == Game.Assault)
			{
				Lesson.text = Localizer.Get("PedestalView.Prompt.TrialLesson." + planet + "." + menuNode.LaunchGame + "." + CurrentDifficulty.ToString());
				ChangeBackground(CurrentDifficulty);
			}
			else
			{
				Lesson.text = Localizer.Get("PedestalView.Prompt.TrialLesson." + planet + "." + menuNode.LaunchGame);
				ChangeBackground(CurrentDifficulty, true);
			}
		}

		/// <summary>
		/// Setup for Trial mode
		/// </summary>
		/// <param name="menuNode">Menu node.</param>
		/// <param name="planet"></param>
		/// <param name="autoSelectDifficulty">If set to <c>true</c> auto select difficulty.</param>
		public void Setup(SurfaceMenuNode menuNode, BonusPlanetType planet, bool autoSelectDifficulty = true)
		{
			currentSurfaceNode = menuNode;
			currentBonusPlanet = planet;
			currentMenuNode = null;
			pillars = menuNode.transform.GetComponentsInChildren<PillarMenuNode>(true);

			if (autoSelectDifficulty)
			{
				SetupDifficulty(menuNode.LaunchGame);
			}

			SetupMode(true);
			SetupGame(menuNode.LaunchGame);

			CurvedUIAnimator.gameObject.SetActive(true);
			CurvedUIAnimator.Play("bentScreen_hiddenToPrimary");

			TrailName.text = Localizer.Get("PedestalView.Title.TrialName." + planet + "." + menuNode.LaunchGame);
			TrailDescription.text = Localizer.Get("PedestalView.Description.TrialObjective." + planet + "." + menuNode.LaunchGame);

			if (menuNode.LaunchGame == Game.Duel || menuNode.LaunchGame == Game.Assault)
			{
				Lesson.text = Localizer.Get("PedestalView.Prompt.TrialLesson." + planet + "." + menuNode.LaunchGame + "." + CurrentDifficulty.ToString());
				ChangeBackground(CurrentDifficulty);
			}
			else
			{
				Lesson.text = Localizer.Get("PedestalView.Prompt.TrialLesson." + planet + "." + menuNode.LaunchGame);
				ChangeBackground(CurrentDifficulty, true);
			}
		}

		public void Setup(PillarMenuNode menuNode, Difficulty? difficulty = null)
		{
			currentMenuNode = menuNode;

			if (difficulty != null)
			{
				CurrentDifficulty = (Difficulty)difficulty;

				EasyButton.Selected = MediumButton.Selected = HardButton.Selected = false;

				switch (CurrentDifficulty)
				{
					case Difficulty.Medium:
						MediumButton.Selected = true;
					break;

					case Difficulty.Hard:
						HardButton.Selected = true;
					break;

					default:
						EasyButton.Selected = true;
					break;
				}

				UpdatePillars();
			}

			if (menuNode.Config.Game == Game.Duel || menuNode.Config.Game == Game.Assault)
			{
				ChangeBackground(CurrentDifficulty);
			}
			else
			{
				ChangeBackground(CurrentDifficulty, true);
			}

			SetupMode(false);
			SetupGame(menuNode.Config.Game);

			if (menuNode.Locked)
			{
				switch (menuNode.Config.Game)
				{
					case Game.Assault:
					case Game.Duel:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PillarLockedDuel, gameObject);
						break;

					case Game.HoloChess:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PillarLockedHoloChess, gameObject);
						break;

					case Game.TowerDefense:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PillarLockedTowerDefense, gameObject);
						break;
				}
				AudioEvent.Play(AudioEventName.GalaxyMap.NodeLocked, gameObject);
			}

			LeftLocked.SetActive(menuNode.Locked);
			LeftUnlocked.SetActive(!menuNode.Locked);
			RightPillarMode.SetActive(!menuNode.Locked);

			string configString = menuNode.Config.GetTokenString();

			// Pillar Selected Mode
			LevelName.text = Localizer.Get("PedestalView.Title.LevelName." + configString);
			Description.text = Localizer.Get("PedestalView.Description.LevelObjective." + configString);
			LockedDescription.text = Localizer.Get("PedestalView.Prompt.LevelLocked." + configString);

			// DarkSide does not have achievements
			if (menuNode.Config.Planet != PlanetType.DarkSide)
			{
				// Achivements
				int achievements = ContainerAPI.GetLevelAchievements(menuNode.Config, (int)CurrentDifficulty);

				AchievementOneComplete.SetActive(achievements > 0);
				AchievementTwoComplete.SetActive(achievements > 1);
				AchievementThreeComplete.SetActive(achievements > 2);

				SetAchievementText(menuNode, configString);
			}
		}

		/// <summary>
		/// Setup for DarkSide Mode
		/// </summary>
		/// <param name="autoSelectDifficulty">If set to <c>true</c> auto select difficulty.</param>
		public void Setup(PillarMenuNode[] darkSidePillars, bool autoSelectDifficulty = true)
		{
			currentSurfaceNode = null;
			currentMenuNode = null;
			pillars = darkSidePillars;

			if (autoSelectDifficulty)
			{
				SetupDifficulty(Game.Duel);
			}

			SetupMode(true);
			SetupGame(Game.Duel);

			CurvedUIAnimator.gameObject.SetActive(true);
			CurvedUIAnimator.Play("bentScreen_hiddenToPrimary");
			ChangeBackground(CurrentDifficulty);
		}

		public void Hide()
		{
			CurvedUIAnimator.Play("bentScreen_primaryToHidden");
		}

		public bool GazeClick()
		{
			if (currentButton == null)
			{
				return false;
			}

			if (currentButton.Locked)
			{
				return false;
			}

			EasyButton.Selected = false;
			MediumButton.Selected = false;
			HardButton.Selected = false;

			CurrentDifficulty = currentButton.Difficulty;
			currentButton.Selected = true;

			switch (currentButton.Difficulty)
			{
				case Difficulty.Easy:
					AudioEvent.Play(AudioEventName.Combat.Encounter1, gameObject);
					break;
				case Difficulty.Medium:
					AudioEvent.Play(AudioEventName.Combat.Encounter2, gameObject);
					break;
				case Difficulty.Hard:
					AudioEvent.Play(AudioEventName.Combat.Encounter3, gameObject);
					break;
			}

			// Update all 3 pillars based on new difficulty selection
			UpdatePillars();

			// Update the CurvedUI based on difficulty change
			if (currentMenuNode != null)
			{
				Setup(currentMenuNode);
			}
			else if (currentSurfaceNode != null)
			{
				if (currentBonusPlanet == null)
				{
					Setup(currentSurfaceNode, currentPlanet, false);
				}
				else
				{
					Setup(currentSurfaceNode, currentBonusPlanet.Value, false);
				}
			}

			return true;
		}

		public void GazedAt(CurvedUIButton button)
		{
			currentButton = button;
			currentButton.GazedAt();
		}

		public void GazedOff(CurvedUIButton button)
		{
			button.GazedOff();
			currentButton = null;
		}

		private void SetupDifficulty(Game game)
		{
			// Need to find all the pillars and figure out the highest unlocked difficulty.
			CurrentDifficulty = Difficulty.Easy;
			EasyButton.Locked = false;
			DifficultyHolder.SetActive(game == Game.Assault || game == Game.Duel);

			// Not in a duel or assault
			if (!DifficultyHolder.activeSelf)
			{
				UpdatePillars();
				return;
			}

			// Lock medium and hard until we know and deselect them all
			HardButton.Locked = MediumButton.Locked = true;
			HardButton.Selected = MediumButton.Selected = EasyButton.Selected = false;
			EasyButton.SetDefault();
			MediumButton.SetDefault();
			HardButton.SetDefault();

			DuelAPI duelApi = ContainerAPI.GetDuelApi();
			PlanetType planet = PlanetType.Naboo;

			// This will unlock Medium and Hard if possible
			for (int i = 0; i < pillars.Length; i++)
			{
				planet = pillars[i].Config.Planet;

				// Special exception for hard duelists
				if (pillars[i].Config.PillarNumber == 1 && duelApi.Progress.HasCompleted(DuelAPI.Duelist.Archivist, 2) && pillars[i].Config.Planet != PlanetType.Core)
				{
					MediumButton.Locked = false;
					HardButton.Locked = false;
				}

				// Normal Path
				else if (pillars[i].Config.PreviousConfig != null)
				{
					if (MediumButton.Locked)
					{
						MediumButton.Locked = !ContainerAPI.IsLevelComplete(pillars[i].Config.PreviousConfig, (int)Difficulty.Medium);
					}

					if (HardButton.Locked)
					{
						HardButton.Locked = !ContainerAPI.IsLevelComplete(pillars[i].Config.PreviousConfig, (int)Difficulty.Hard);
					}
				}

				// First Levels
				else
				{
					if (planet == PlanetType.DarkSide)
					{
						if (MediumButton.Locked)
						{
							MediumButton.Locked = (ContainerAPI.AllProgressionUnlocked) ? false : !duelApi.Progress.HasCompleted(DuelAPI.Duelist.Rey, 1);
						}

						if (HardButton.Locked)
						{
							HardButton.Locked = (ContainerAPI.AllProgressionUnlocked) ? false : !duelApi.Progress.HasCompleted(DuelAPI.Duelist.Rey, 2);
						}
					}
					else
					{
						if (MediumButton.Locked)
						{
							MediumButton.Locked = (ContainerAPI.AllProgressionUnlocked) ? false : !duelApi.Progress.HasCompleted(DuelAPI.Duelist.KyloRen,
							                                                                                                  1);
						}

						if (HardButton.Locked)
						{
							HardButton.Locked = (ContainerAPI.AllProgressionUnlocked) ? false : !duelApi.Progress.HasCompleted(DuelAPI.Duelist.Archivist,
							                                                                                                2);
						}
					}
				}

				// Special exception for core duel track
				if (pillars[i].Config.Planet == PlanetType.Core && (pillars[i].Config.Game == Game.Duel || pillars[i].Config.Game == Game.Assault))
				{
					if (!ContainerAPI.IsMedalUnlocked(MedalType.Insight) || !ContainerAPI.IsMedalUnlocked(MedalType.Leadership) || !ContainerAPI.IsMedalUnlocked(MedalType.AdvancedCombat))
					{
						HardButton.Locked = true;
					}
				}
			}

			// If the planet is Core, there are only two difficulties, Medium and Hard. Medium is called Easy to the Player
			EasyButton.gameObject.SetActive(planet != PlanetType.Core);
			ButtonOrientation(planet != PlanetType.Core);
			MediumButtonText.text = Localizer.Get((planet != PlanetType.Core) ? "LightsaberDuel.Difficulty.Medium" : "LightsaberDuel.Difficulty.Easy");

			// Hard or Medium unlocked?
			if (!HardButton.Locked)
			{
				CurrentDifficulty = Difficulty.Hard;
				HardButton.Selected = true;
			}
			else if (!MediumButton.Locked)
			{
				CurrentDifficulty = Difficulty.Medium;
				MediumButton.Selected = true;
			}
			else if (!EasyButton.Locked)
			{
				EasyButton.Selected = true;
			}

			// Update all 3 pillars based on new difficulty selection
			UpdatePillars();
		}

		private void UpdatePillars()
		{
			for (int i = 0; i < pillars.Length; i++)
			{
				pillars[i].UpdateState(CurrentDifficulty);
			}
		}

		private void SetupMode(bool isTrailMode)
		{
			LeftTrailMode.SetActive(isTrailMode);
			RightTrailMode.SetActive(isTrailMode);

			LeftPillarMode.SetActive(!isTrailMode);
			RightPillarMode.SetActive(!isTrailMode);
		}

		private void SetupGame(Game game)
		{
			CombatMedal.SetActive(game == Game.Assault || game == Game.Duel);
			LeadershipMedal.SetActive(game == Game.TowerDefense);
			InsightMedal.SetActive(game == Game.HoloChess);
		}

		private Difficulty GetDifficultyToCheck(PillarConfig config)
		{
			switch (config.Game)
			{
				case Game.Assault:
					return (config.Planet == PlanetType.Core) ? Difficulty.Medium : Difficulty.Easy;

				case Game.Duel:
					return (config.Duelist == DuelAPI.Duelist.Archivist) ? Difficulty.Medium : Difficulty.Easy;

				default:
					return Difficulty.Easy;
			}
		}

		private void SetAchievementText(PillarMenuNode menuNode, string configString)
		{
			switch (menuNode.Config.Game)
			{
				case Game.Duel:
					AchievementTwo.SetActive(true);
					AchievementThree.SetActive(true);

					int par2 = DuelRating.DuelistPar(menuNode.Config.Duelist, CurrentDifficulty, 2);
					int par3 = DuelRating.DuelistPar(menuNode.Config.Duelist, CurrentDifficulty, 3);

					AchievementOneDescription.text = Localizer.Get("PedestalView.Label.Performance.One." + menuNode.Config.Game);
					AchievementTwoDescription.text = Localizer.Get("PedestalView.Label.Performance.Two." + menuNode.Config.Game)
						.Replace("$par", par2.ToString());
					AchievementThreeDescription.text = Localizer.Get("PedestalView.Label.Performance.Three." + menuNode.Config.Game)
						.Replace("$par", par3.ToString());

					break;

				case Game.Assault:
					AchievementTwo.SetActive(true);
					AchievementThree.SetActive(true);

					AchievementOneDescription.text = Localizer.Get("PedestalView.Label.Performance.One." + menuNode.Config.Game);
					AchievementTwoDescription.text = Localizer.Get("PedestalView.Label.Performance.Two." + configString);
					AchievementThreeDescription.text = Localizer.Get("PedestalView.Label.Performance.Three." + configString);

					break;

				case Game.HoloChess:
					AchievementTwo.SetActive(true);

					AchievementOneDescription.text = Localizer.Get("PedestalView.Label.Performance.One.HoloChess");
					AchievementTwoDescription.text = Localizer.Get("PedestalView.Label.Performance.Two." + configString);

					if ((menuNode.Config.Planet == PlanetType.Lothal && menuNode.Config.PillarNumber == 3) ||
					    menuNode.Config.Planet == PlanetType.Hoth ||
					    menuNode.Config.Planet == PlanetType.Takodana ||
					    menuNode.Config.Planet == PlanetType.Core)
					{
						AchievementThree.SetActive(false);
					}
					else
					{
						AchievementThree.SetActive(true);
						AchievementThreeDescription.text = Localizer.Get("PedestalView.Label.Performance.Three." + configString);
					}
					
					break;

				case Game.TowerDefense:
					AchievementOneDescription.text = Localizer.Get("PedestalView.Label.Performance.One.TowerDefense");

					// If the planet is Naboo or Core, there's only one achievement
					if (menuNode.Config.Planet == PlanetType.Naboo || menuNode.Config.Planet == PlanetType.Core)
					{
						AchievementTwo.SetActive(false);
					}
					else
					{
						AchievementTwo.SetActive(true);
						AchievementTwoDescription.text = BSG.SWARTD.TDAPI.GetInstance().GetTextFor2ndObjective(menuNode.Config.Battle);
					}

					AchievementThree.SetActive(false);

					break;
			}
		}

		private void ChangeBackground(Difficulty difficulty, bool setDefault = false)
		{
			BackgroundDefault.SetActive(setDefault);
			BackgroundEasy.SetActive(difficulty == Difficulty.Easy && !setDefault);
			BackgroundMedium.SetActive(difficulty == Difficulty.Medium && !setDefault);
			BackgroundHard.SetActive(difficulty == Difficulty.Hard && !setDefault);
		}

		private void ButtonOrientation(bool normalOrientation)
		{
			if (normalOrientation)
			{
				MediumButton.transform.localPosition = MedBtnNormalPos;
				MediumButton.transform.localEulerAngles = MedBtnNormalRot;
				HardButton.transform.localEulerAngles = HardBtnNormalRot;
			}
			else
			{
				MediumButton.transform.localPosition = MedBtnCorePos;
				MediumButton.transform.localEulerAngles = MedBtnCoreRot;
				HardButton.transform.localEulerAngles = HardBtnCoreRot;
			}
		}
	}
}