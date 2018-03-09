using UnityEngine;
using SG.ProtoFramework;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	public class PinController : MonoBehaviour
	{
		public static PillarConfig CommanderPinConfig = null;
		public static List<PillarConfig> GuardianPinConfig = new List<PillarConfig>();
		public static PillarConfig ConsularPinConfig = null;

		public static int GuardianPinConfigDifficulty = 1;

		public PillarConfig CommanderFinalBattle;
		public PillarConfig GuardianFinalBattle;
		public PillarConfig ConsularFinalBattle;

		[EnumMappedList(typeof(PlanetType))]
		public PinHolder[] PinHolders = new PinHolder[System.Enum.GetNames(typeof(PlanetType)).Length];

		private void Start()
		{
			PillarConfig currentConfig = null;

			GuardianPinConfig.Clear();

			// Guardian Track, this one is weird. 

			// If the hard archivist is not beating but all hard duelist are, only pin the core (if TD / Chess are beat).
			if (!ContainerAPI.IsLevelComplete(GuardianFinalBattle, 3) && ContainerAPI.IsMedalUnlocked(MedalType.AdvancedCombat))
			{
				if (ContainerAPI.IsMedalUnlocked(MedalType.Insight) && ContainerAPI.IsMedalUnlocked(MedalType.Leadership))
				{
					// We need to know the given pillar too..
					currentConfig = GuardianFinalBattle;

					// We only need to check the last 3 in this execption.
					for (int i = 0; i < 3; i++)
					{
						if (!ContainerAPI.IsLevelLocked(currentConfig, 3))
						{
							GuardianPinConfig.Add(currentConfig);
							GuardianPinConfigDifficulty = 3;
							break;
						}

						currentConfig = currentConfig.PreviousConfig;
					}

					// Show the Planet Pin
					PinHolders[(int)PlanetType.Core].ShowPin(PinType.Guardian);
				}
			}

			// If you beat archivist on medium, we show pins for all planets that you haven't beat the duelist on
			else if (ContainerAPI.IsLevelComplete(GuardianFinalBattle, 2))
			{
				currentConfig = GuardianFinalBattle;
				while (currentConfig != null)
				{
					// Only Duelist that are not on the core, another exception.
					if (currentConfig.Game == Game.Duel && !ContainerAPI.IsLevelComplete(currentConfig, 3) && currentConfig.Planet != PlanetType.Core)
					{
						// Save for later, More exceptions, need to check back 3 pillars here too....
						PillarConfig second = currentConfig.PreviousConfig;
						PillarConfig first = (second != null) ? second.PreviousConfig : null;

						if (first != null && !ContainerAPI.IsLevelComplete(first, 3))
						{
							GuardianPinConfig.Add(first);
						}
						else if (second != null && !ContainerAPI.IsLevelComplete(second, 3))
						{
							GuardianPinConfig.Add(second);
						}
						else
						{
							GuardianPinConfig.Add(currentConfig);
						}

						GuardianPinConfigDifficulty = 3;

						// We show the pin?
						PinHolders[(int)currentConfig.Planet].ShowPin(PinType.Guardian);
					}

					currentConfig = currentConfig.PreviousConfig;
				}
			}

			// If you havent...
			else
			{
				// Check medium progress then easy.
				for (int difficulty = 2; difficulty > 0; difficulty--)
				{
					currentConfig = GuardianFinalBattle;
					while (currentConfig != null)
					{
						if (!ContainerAPI.IsLevelLocked(currentConfig, difficulty))
						{
							// Save for later
							GuardianPinConfig.Add(currentConfig);
							GuardianPinConfigDifficulty = difficulty;

							// We show the pin?
							PinHolders[(int)currentConfig.Planet].ShowPin(PinType.Guardian);
							difficulty = 0;
							break;
						}

						currentConfig = currentConfig.PreviousConfig;
					}
				}
			}

			// Commander Track (Easy Only)
			currentConfig = CommanderFinalBattle;
			while (currentConfig != null)
			{
				// First one not locked. 
				if (!ContainerAPI.IsLevelLocked(currentConfig, 1))
				{
					// Last Level is unlocked but not beat
					if (currentConfig == CommanderFinalBattle)
					{
						if (!ContainerAPI.IsLevelComplete(currentConfig, 1))
						{
							PinHolders[(int)PlanetType.Core].ShowPin(PinType.Commander);
							CommanderPinConfig = currentConfig;
						}
						break;
					}

					// Save for later
					CommanderPinConfig = currentConfig;

					// We show the pin?
					PinHolders[(int)currentConfig.Planet].ShowPin(PinType.Commander);
					break;
				}

				currentConfig = currentConfig.PreviousConfig;
			}

			// Consular Track (Easy Only)
			currentConfig = ConsularFinalBattle;
			while (currentConfig != null)
			{
				// First one not locked. 
				if (!ContainerAPI.IsLevelLocked(currentConfig, 1))
				{
					// Last Level is unlocked
					if (currentConfig == ConsularFinalBattle)
					{
						// but not beat
						if (!ContainerAPI.IsLevelComplete(currentConfig, 1))
						{
							PinHolders[(int)PlanetType.Core].ShowPin(PinType.Consular);
							ConsularPinConfig = currentConfig;
						}
						break;
					}

					// Save for later
					ConsularPinConfig = currentConfig;

					// We show the pin?
					PinHolders[(int)currentConfig.Planet].ShowPin(PinType.Consular);
					break;
				}

				currentConfig = currentConfig.PreviousConfig;
			}
		}
	}
}