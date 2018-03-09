using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Disney.ForceVision;

namespace Disney.ForceVision
{
	public class PorgMenuNode : MenuNode
	{

		#region Public Properties 

		/// <summary>
		/// The scene to launch.
		/// </summary>
		public string SceneName;

		/// <summary>
		/// The lock icon.
		/// </summary>
		public GameObject LockIcon;

		#endregion

		#region Private Properties

		/// <summary>
		/// The container.
		/// </summary>
		private ContainerAPI Container;

		#endregion

		#region Unity Methods

		private void Awake()
		{
			Container = new ContainerAPI (Game.ForceVision);

			if (Container.PlayerPrefs.PrefKeyExists(Constants.PorgUnlocked) || ContainerAPI.AllProgressionUnlocked)
			{
				LockIcon.SetActive(false);
			}
		}

		#endregion

		#region Protected Methods

		protected override void OnFocusUpdated(bool state)
		{
			base.OnFocusUpdated(state);

			if (state && OnNodeFocused != null)
			{
				OnNodeFocused(this, new MenuNodeEventArgs(NodeType, BonusPlanetType.Crait));
			}
		}

		protected override void OnSelectUpdated(bool state)
		{

			if (Container.PlayerPrefs.PrefKeyExists(Constants.PorgUnlocked) || ContainerAPI.AllProgressionUnlocked)
			{
				AudioEvent.Play("MAP_MX_TransitionStinger", gameObject);

				// Load scene on a delay to match the higher loading times of other games
				// The transition stinger has a delay to compensate, so now all matches
				Invoke ("LoadSceneDelayed", 1f);
			}
			else
			{
				// TODO should we play some animation or audio?
			}
		}

		protected void LoadSceneDelayed()
		{
			SceneManager.LoadScene (SceneName);
		}

		#endregion
	}
}