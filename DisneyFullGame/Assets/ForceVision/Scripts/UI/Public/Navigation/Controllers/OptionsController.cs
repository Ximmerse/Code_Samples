using System;
using Disney.Vision;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class OptionsController : BaseController
	{
		public ToggleElement OptionsButton;
		public Text LevelName;
		public Text PlanetName;
		const string PorgIslandToken = "PedestalView.Title.LevelName.PorgIsland";

		void Start()
		{
			if (MenuController.ConfigToLoad == null)
			{
				LevelName.text = "";
				PlanetName.text = "";

				// Hack: No config, so check if it is porg island level and set names appropriately -- John Alvarado
				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "PorgIsland")
				{
					if (Localizer.Has(PorgIslandToken))
					{
						LevelName.text = Localizer.Get(PorgIslandToken).Replace(Environment.NewLine, " ");
					}
					PlanetName.text = Localizer.Get("General.Name.Crait");
				}

				return;
			}

			LevelName.text = Localizer.Get("PedestalView.Title.LevelName." + MenuController.ConfigToLoad.GetTokenString()).Replace(Environment.NewLine, " ");

			if (MenuController.ConfigToLoad.Game == Game.Duel || MenuController.ConfigToLoad.Game == Game.Assault)
			{
				// Medium on Core is called Easy. Otherwise, assign difficulty like normal
				if (MenuController.ConfigToLoad.Planet == PlanetType.Core && MenuController.DifficultyToLoad == Difficulty.Medium)
				{
					PlanetName.text = Localizer.Get("General.Name." + PlanetType.Core.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty.Easy");
				}
				else if (MenuController.ConfigToLoad.IsBonusPlanet)
				{
					PlanetName.text = Localizer.Get("General.Name." + MenuController.ConfigToLoad.BonusPlanet.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty." + MenuController.DifficultyToLoad.ToString());
				}
				else
				{
					PlanetName.text = Localizer.Get("General.Name." + MenuController.ConfigToLoad.Planet.ToString()) + ", " + Localizer.Get("LightsaberDuel.Difficulty." + MenuController.DifficultyToLoad.ToString());
				}
			}
			else
			{
				PlanetName.text = Localizer.Get("General.Name." + ((MenuController.ConfigToLoad.IsBonusPlanet) ? MenuController.ConfigToLoad.BonusPlanet.ToString() : MenuController.ConfigToLoad.Planet.ToString()));
			}
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if(!ButtonIsActivateOrControl(eventArguments.Button))
			{
				return;
			}

			if (CurrentElement != null && CurrentElement.Interactable)
			{
				CurrentElement.OnClicked();
			}

			base.OnButtonUp(sender, eventArguments);
		}

		protected override void OnGazedAt(object sender, GazeEventArgs eventArguments)
		{
			base.OnGazedAt(sender, eventArguments);

			if (CurrentElement != null)
			{
				CurrentElement.OnGazedAt();
			}
		}

		protected override void OnGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (CurrentElement != null)
			{
				CurrentElement.OnGazedOff();
			}

			base.OnGazedOff(sender, eventArguments);
		}

		public void ResumeClicked()
		{
			if (Controller.OnResumeButton != null)
			{
				Controller.OnResumeButton.Invoke(this, new EventArgs());
			}

			AudioEvent.Play(AudioEventName.Settings.Resume, gameObject);
			Controller.ToggleVisiblity(false);
		}

		public void QuitClicked()
		{
			if (Controller.OnQuitButton != null)
			{
				Controller.OnQuitButton.Invoke(this, new EventArgs());
			}

			AudioEvent.Play(AudioEventName.Settings.Quit, gameObject);
			Controller.ToggleVisiblity(false);
		}

		public void RestartClicked()
		{
			if (Controller.OnRestartButton != null)
			{
				Controller.OnRestartButton.Invoke(this, new EventArgs());
			}

			AudioEvent.Play(AudioEventName.Settings.Restart, gameObject);
			Controller.ToggleVisiblity(false);
		}
	}
}