using UnityEngine;
using SG.ProtoFramework;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	public class PillarContainer : MonoBehaviour
	{
		#region Public Inspector Properties

		public CurvedUIController UIController;

		/// <summary>
		/// The planet animators.
		/// </summary>
		[EnumMappedList(typeof(PlanetType))]
		public Animator[] PlanetAnimators = new Animator[System.Enum.GetNames(typeof(PlanetType)).Length];

		/// <summary>
		/// The bonus planet animators.
		/// </summary>
		[EnumMappedList(typeof(BonusPlanetType))]
		public Animator[] BonusPlanetAnimators = new Animator[System.Enum.GetNames(typeof(BonusPlanetType)).Length];

		/// <summary>
		/// The second level pins for duel.
		/// </summary>
		[EnumMappedList(typeof(PlanetType))]
		public GameObject[] DuelPins = new GameObject[System.Enum.GetNames(typeof(PlanetType)).Length];

		/// <summary>
		/// The second level pins for tower.
		/// </summary>
		[EnumMappedList(typeof(PlanetType))]
		public GameObject[] TowerPins = new GameObject[System.Enum.GetNames(typeof(PlanetType)).Length];

		/// <summary>
		/// The second level pins for chess.
		/// </summary>
		[EnumMappedList(typeof(PlanetType))]
		public GameObject[] ChessPins = new GameObject[System.Enum.GetNames(typeof(PlanetType)).Length];

		/// <summary>
		/// The porg unlock FX.
		/// </summary>
		public GameObject PorgUnlockFX;

		#endregion

		private Transform targetTransform;
		private bool animateToFacePlayer;
		private float animationTime;

		#region Unity Methods

		/// <summary>
		/// Start of this instance.
		/// </summary>
		private void Start()
		{
			for (int i = 0; i < PlanetAnimators.Length; i++)
			{
				PlanetAnimators[i].gameObject.SetActive(false);
			}
		}

		protected void Update()
		{
			if (animateToFacePlayer)
			{
				Vector3 position = targetTransform.position;
				position.y = 0.0f;
				transform.LookAt(position);

				animationTime += Time.deltaTime;

				if (animationTime > 2.0f)
				{
					animateToFacePlayer = false;
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Faces the player.
		/// </summary>
		/// <param name="player">Player.</param>
		public void FacePlayer(Transform player)
		{
			targetTransform = player;
			animateToFacePlayer = true;
		}

		/// <summary>
		/// Gets the planet animator.
		/// </summary>
		/// <returns>The planet animator.</returns>
		/// <param name="planet">Planet.</param>
		public Animator GetPlanetAnimator(PlanetType planet)
		{
			Animator animator = PlanetAnimators[(int)planet];
			animator.gameObject.SetActive(true);
			return animator;
		}

		/// <summary>
		/// Gets the bonus planet animator.
		/// </summary>
		/// <returns>The planet animator.</returns>
		/// <param name="planet">Planet.</param>
		public Animator GetPlanetAnimator(BonusPlanetType planet)
		{
			Animator animator = BonusPlanetAnimators[(int)planet];
			animator.gameObject.SetActive(true);
			return animator;
		}

		/// <summary>
		/// Checks to show second level pin.
		/// </summary>
		/// <param name="planet">Planet.</param>
		public void CheckToShowSecondLevelPin(PlanetType planet)
		{
			UpdateSecondLevelPin(planet, PinController.GuardianPinConfig, Game.Duel, true);
			UpdateSecondLevelPin(planet, new List<PillarConfig> { PinController.CommanderPinConfig }, Game.TowerDefense, true);
			UpdateSecondLevelPin(planet, new List<PillarConfig> { PinController.ConsularPinConfig }, Game.HoloChess, true);
		}

		public void CheckToShowSecondLevelPin(BonusPlanetType planet)
		{
		}

		/// <summary>
		/// Checks to hide second level pin.
		/// </summary>
		/// <param name="planet">Planet.</param>
		public void CheckToHideSecondLevelPin(PlanetType planet)
		{
			UpdateSecondLevelPin(planet, PinController.GuardianPinConfig, Game.Duel, false);
			UpdateSecondLevelPin(planet, new List<PillarConfig> { PinController.CommanderPinConfig }, Game.TowerDefense, false);
			UpdateSecondLevelPin(planet, new List<PillarConfig> { PinController.ConsularPinConfig }, Game.HoloChess, false);
		}

		#endregion

		#region Private methods

		private void UpdateSecondLevelPin(PlanetType planet, List<PillarConfig> configs, Game game, bool state)
		{
			foreach (PillarConfig config in configs)
			{
				if (config != null && config.Planet == planet)
				{
					switch (game)
					{
						case Game.Duel:
							DuelPins[(int)planet].SetActive(state);
						break;

						case Game.HoloChess:
							ChessPins[(int)planet].SetActive(state);
						break;

						case Game.TowerDefense:
							TowerPins[(int)planet].SetActive(state);
						break;
					}
				}
			}
		}

		#endregion
	}
}