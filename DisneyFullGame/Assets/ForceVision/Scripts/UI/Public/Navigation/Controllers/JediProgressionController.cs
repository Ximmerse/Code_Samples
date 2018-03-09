using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;
using System;
using SG.ProtoFramework;
using SG.Lonestar;
using Disney.AssaultMode;
using Disney.HoloChess;
using BSG.SWARTD;

namespace Disney.ForceVision
{
	public class JediProgressionController : BaseController
	{
		[EnumMappedList(typeof(PlanetType))]
		public Image[] PlanetFills = new Image[Enum.GetNames(typeof(PlanetType)).Length];

		[EnumMappedList(typeof(PlanetType))]
		public PlanetNodes[] PlanetNodes = new PlanetNodes[Enum.GetNames(typeof(PlanetType)).Length];

		[EnumMappedList(typeof(PlanetType))]
		public MeshRenderer[] CorePlanetParts = new MeshRenderer[Enum.GetNames(typeof(PlanetType)).Length];

		[EnumMappedList(typeof(PlanetType))]
		public MeshRenderer[] CorePlanetShards = new MeshRenderer[Enum.GetNames(typeof(PlanetType)).Length];

		public Material PlanetGoldMaterial;
		public Material ShardGoldMaterial;
		public Material CoreGoldMaterial;

		public GameObject FinalBattleTooltip;

		public PillarConfig[] PillarConfigs = new PillarConfig[18];

		public void Init(bool shown)
		{
			if (Game == Game.ForceVision)
			{
				SetProgression();
			}

			base.Init();
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (eventArguments.Button != ButtonType.SaberActivate && eventArguments.Button != ButtonType.SaberControl && eventArguments.Button != ButtonType.HmdSelect)
			{
				return;
			}

			if (CurrentElement != null && CurrentElement.Interactable)
			{
				if (LastElement != null && LastElement != CurrentElement)
				{
					LastElement.OnClicked();
				}

				CurrentElement.OnClicked();
				LastElement = (LastElement == CurrentElement) ? null : CurrentElement;
			}

			base.OnButtonUp(sender, eventArguments);
		}

		/// <summary>
		/// This will set the display of the progression, this is going to be dirty because there isn't really a dynamic way
		/// to know the progression as it is kinda randomly defined and each game has their own way to getting the info.
		/// Naboo, Garel, Lothal, Hoth, Takodana, Core (Middle)
		/// </summary>
		private void SetProgression()
		{
			// Assault Mode
			int nextPlanet;
			int nextNode;
			int[] ratings = GeneratePillarRatings(out nextPlanet, out nextNode);
			int count = 0;
			int planetsUnlocked = 0;
			int planetsComplete = 0;
			bool hideCoreNodes = false;

			for (int planet = 0; planet < PlanetNodes.Length; planet++)
			{
				int easyComplete = 0;
				int mediumComplete = 0;
				int hardComplete = 0;

				for (int node = 0; node < Enum.GetNames(typeof(NodeName)).Length; node++)
				{
					easyComplete += (ratings[count] > 0) ? 1 : 0;
					mediumComplete += (ratings[count] > 1) ? 1 : 0;
					hardComplete += (ratings[count] > 2) ? 1 : 0;

					PlanetNodes[planet].SetProgress((NodeName)node, (DifficultyComplete)ratings[count]);
					count++;
				}

				if (planet != (int)PlanetType.Core)
				{
					if (mediumComplete > 0)
					{
						PlanetFills[planet].fillAmount = mediumComplete * 0.33333f;
						GameObject goldTrack = PlanetFills[planet].transform.Find("Track_Gold").gameObject;
						goldTrack.SetActive(true);
						goldTrack.GetComponent<Image>().fillAmount = PlanetFills[planet].fillAmount;
					}
					else
					{
						PlanetFills[planet].fillAmount = easyComplete * 0.333333f;
					}

					if (easyComplete == 3)
					{
						CorePlanetParts[planet].material = PlanetGoldMaterial;
						CorePlanetShards[planet].material = ShardGoldMaterial;
						planetsUnlocked++;
					}

					if (mediumComplete == 3)
					{
						planetsComplete++;
					}
				}
				else
				{
					// Beat the 2 levels leading up to the archivist
					if (easyComplete >= 2)
					{
						CorePlanetParts[(int)PlanetType.Core].gameObject.SetActive(true);

					}

					// Beat the archivist
					if (easyComplete >= 3)
					{
						hideCoreNodes = true;
						CorePlanetParts[(int)PlanetType.Core].GetComponent<MeshRenderer>().material = CoreGoldMaterial;
					}
				}

				// Debug.LogError("== " + Enum.GetNames(typeof(PlanetType))[planet] + " (E, M, H): " + easyComplete + ", " + mediumComplete + ", " + hardComplete);
			}

			if (planetsUnlocked >= 5)
			{
				if (!hideCoreNodes)
				{
					CorePlanetParts[(int)PlanetType.Core].gameObject.SetActive(true);
				}

				if (planetsComplete >= 5)
				{
					PlanetNodes[(int)PlanetType.Core].gameObject.SetActive(true);
				}
			}

			// Archivist Duel Tooltip
			if (nextPlanet == (int)PlanetType.Core && nextNode == (int)NodeName.Thrid)
			{
				FinalBattleTooltip.SetActive(true);
			}

			// All others
			else
			{
				PlanetNodes[nextPlanet].SetTooltip((NodeName)nextNode);
			}
		}

		private int[] GeneratePillarRatings(out int nextPlanet, out int nextNode)
		{
			AssaultAPI assaultApi = new AssaultAPI();
			DuelAPI duelApi = ContainerAPI.GetDuelApi();

			int[] results = new int[PillarConfigs.Length];
			int nextEasyConfig = -1;
			int nextMediumConfig = -1;

			for (int i = 0; i < PillarConfigs.Length; i++)
			{
				if (PillarConfigs[i].Game == Game.Duel)
				{
					int rating = 0;

					for (int difficulty = 1; difficulty <= (int)DifficultyComplete.Hard; difficulty++)
					{
						if (duelApi.Progress.HasCompleted(PillarConfigs[i].Duelist, difficulty))
						{
							rating++;
						}
					}

					results[i] = rating;
				}
				else if (PillarConfigs[i].Game == Game.Assault)
				{
					int rating = 0;

					for (int difficulty = 1; difficulty <= (int)DifficultyComplete.Hard; difficulty++)
					{
						if (assaultApi.RatingForStage(PillarConfigs[i].Planet, PillarConfigs[i].PillarNumber, difficulty) > 0)
						{
							rating++;
						}
					}

					results[i] = rating;
				}

				// Next Level Not beat on easy
				if (results[i] == 0 && nextEasyConfig == -1)
				{
					nextEasyConfig = i;
				}

				// Next level not beat on medium
				if (results[i] == 1)
				{
					nextMediumConfig = i;
				}

				// Debug.LogWarning("= " + PillarConfigs[i].Game + ": [" + i + "]" + results[i]);
			}

			if (nextEasyConfig != -1)
			{
				nextPlanet = (int)Mathf.Floor((float)nextEasyConfig / Enum.GetNames(typeof(NodeName)).Length);
				nextNode = nextEasyConfig % Enum.GetNames(typeof(NodeName)).Length;
			}
			else
			{
				nextPlanet = (int)Mathf.Floor((float)nextMediumConfig / Enum.GetNames(typeof(NodeName)).Length);
				nextNode = nextMediumConfig % Enum.GetNames(typeof(NodeName)).Length;

				// Debug.LogError("Next Medium Config: " + nextPlanet + " | " + nextNode + PillarConfigs[nextMediumConfig].Game);
			}

			return results;
		}
	}
}