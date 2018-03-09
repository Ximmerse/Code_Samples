using UnityEngine;
using SG.ProtoFramework;
using System.Collections;

namespace Disney.ForceVision
{
	public class PlanetLineController : MonoBehaviour
	{
		public static bool BeatFirstTime = false;

		/// <summary>
		/// The Planet Lines
		/// </summary>
		[EnumMappedList(typeof(PlanetType))]
		public Animator[] PlanetLines = new Animator[System.Enum.GetNames(typeof(PlanetType)).Length];
		[EnumMappedList(typeof(BonusPlanetType))]
		public Animator[] BonusPlanetLines = new Animator[System.Enum.GetNames(typeof(BonusPlanetType)).Length];

		[Header("Special case for easy to medium")]
		public Animator TakodanaToNabooLine;

		[Header("The configs for the Duel path in order")]
		public PillarConfig[] DuelConfigs = new PillarConfig[0];

		public GameObject Explosion;

		private const string BlueExtendName = "planetLineShown";
		private const string BlueAnimateName = "planetLineExtend";
		private const string GoldExtendName = "planetLineShownGold";
		private const string GoldAnimateName = "planetLineExtendGold";

		private bool beatDuel;

		private void Start()
		{
			// If its the first time we beat this level.
			if (BeatFirstTime)
			{
				beatDuel = (MenuController.ConfigToLoad != null && MenuController.ConfigToLoad.Game == Game.Duel);

				// We beat something and are deep linking to next pillar, trigger fancy animation on the next pillar
				if (MenuController.DeepLinkOnLoad)
				{
					MenuController.UnlockedPillar = true;
				}

				// Only do this for beating a duelist for the first time
				if (MenuController.ConfigToLoad != null && MenuController.ConfigToLoad.Game == Game.Duel && MenuController.DifficultyToLoad == Difficulty.Medium)
				{
					Explosion.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Play the blue line on the specified animator.
		/// </summary>
		/// <param name="lineAnimator">Line animator.</param>
		/// <param name="animate">If set to <c>true</c> animate and play sfx.</param>
		private void PlayBlueLine(Animator lineAnimator, bool animate)
		{
			if (!lineAnimator)
			{
				return;
			}

			lineAnimator.gameObject.SetActive(true);

			if (beatDuel && animate)
			{
				lineAnimator.Play(BlueAnimateName);
			}
			else
			{
				lineAnimator.Play(BlueExtendName);
			}
		}

		public void ShowLines()
		{
			// Show Lines
			if (!ContainerAPI.AllProgressionUnlocked)
			{
				int lastUnlocked = -1;
				bool foundLocked = false;
				for (int difficulty = 1; difficulty <= 3; difficulty++)
				{
					for (int i = 0; i < DuelConfigs.Length; i++)
					{
						// Easy only has 5 planets to check.
						if (difficulty == 1 && i > 14)
						{
							continue;
						}

						if (ContainerAPI.IsLevelLocked(DuelConfigs[i], difficulty))
						{
							foundLocked = true;
							break;
						}

						lastUnlocked++;
					}

					if (foundLocked)
					{
						break;
					}
				}

				int planetCount = System.Enum.GetNames(typeof(PlanetType)).Length;
				int easyCount = (planetCount - 1) * 3;
				int mediumCount = easyCount + (planetCount * 3);

				// Easy (first 3 don't count, 5 planets (core doesn't have easy) * 3 pillars per)
				if (lastUnlocked > 2 && lastUnlocked < easyCount)
				{
					lastUnlocked = lastUnlocked - 3;
					int planet = Mathf.FloorToInt(lastUnlocked / 3.0f);
					bool firstUnlock = (BeatFirstTime && (lastUnlocked % 3 == 0));

					for (int currentPlanet = planet; currentPlanet >= 0; currentPlanet--)
					{
						PlayBlueLine(PlanetLines[currentPlanet], firstUnlock && currentPlanet == planet);
					}
				}

				// Special case of Takodana to Naboo
				else if (lastUnlocked >= easyCount && lastUnlocked < easyCount + 3 && lastUnlocked < mediumCount)
				{
					bool firstUnlock = (BeatFirstTime && (lastUnlocked % 3 == 0));

					// Enable All Blue (Except Core)
					for (int i = 0; i < PlanetLines.Length - 1; i++)
					{
						PlanetLines[i].gameObject.SetActive(true);
						PlanetLines[i].Play(BlueExtendName);
					}

					// Including the special line
					PlayBlueLine(TakodanaToNabooLine, firstUnlock);
				}

				// Medium, no line for first 3, 6 more planets * 3 pillars.
				else if (lastUnlocked >= easyCount + 3 && lastUnlocked < mediumCount)
				{
					// Enable All Blue (Except Core)
					for (int i = 0; i < PlanetLines.Length - 1; i++)
					{
						PlanetLines[i].gameObject.SetActive(true);
						PlanetLines[i].Play(BlueExtendName);
					}

					// Enable Blue for bonus planets
					BonusPlanetLines[0].gameObject.SetActive(true);
					BonusPlanetLines[0].Play(BlueExtendName);

					// Include special unless we are at the core
					if (lastUnlocked < mediumCount - 3)
					{
						TakodanaToNabooLine.gameObject.SetActive(true);
						TakodanaToNabooLine.Play(BlueExtendName);
					}

					lastUnlocked = lastUnlocked - easyCount;
					int planet = Mathf.FloorToInt(lastUnlocked / 3.0f) - 1;
					bool firstUnlock = (BeatFirstTime && (lastUnlocked % 3 == 0));

					for (int currentPlanet = planet; currentPlanet >= 0; currentPlanet--)
					{
						PlanetLines[currentPlanet].gameObject.SetActive(true);
						PlanetLines[currentPlanet].Play((firstUnlock && currentPlanet == planet) ? GoldAnimateName : GoldExtendName);
					}
				}
			}
				
			BeatFirstTime = false;
		}
	}
}