using UnityEngine;
using UnityEngine.SceneManagement;
using Disney.Vision.Internal;
using System.Collections;
using SG.ProtoFramework;

namespace Disney.ForceVision
{
	public class DarkSideMenuController : MenuController
	{
		public PillarMenuNode[] DarkSidePillars;
		public CurvedUIController CurvedUIController;

		#region Private Properties
		#endregion

		#region Unity Methods

		private void Awake()
		{
			Time.timeScale = 1f;
		}

		private void Start()
		{
			gazeListener = new GazeListener(new [] { typeof(MenuNode), typeof(ReferenceNode), typeof(CurvedUIButton) },
				OnItemGazedAt,
				OnItemGazedOff);
			GazeWatcher.AddListener(gazeListener);
			GazeWatcher.RaycastEnabled = true;

////			lastNode = selectedNode = Galaxy.GetComponent<MenuNode>();
			lastNode = selectedNode = null;

//			SceneManager.sceneLoaded += OnSceneLoadedDoPillarSetup;
//			SceneManager.LoadScene("Pillars", LoadSceneMode.Additive);

//			if (DeepLinkOnLoad)
//			{
//				deepLinkDifficulty = DifficultyToLoad;
//			}
		}

		private void OnDestroy()
		{
			GazeWatcher.RemoveListener(gazeListener);

			if (HolocronObjectToDestroy != null)
			{
				// removing the holocron from 3dFtue
				Destroy(HolocronObjectToDestroy);

				// disabling the animation flag
				ShouldHolocronAnimate = true;
			}
		}

		private void OnSceneLoadedDoPillarSetup(Scene scene, LoadSceneMode mode)
		{
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Setup this instance.
		/// </summary>
		public override void Setup()
		{
			// Show the Holocron and Animate the Galaxy
//			Animating = true;
//
//			if (DeepLinkOnLoad)
//			{
//				HolocronFX.SetActive(true);
//				PlayStartupAudio();
//			}
//			else
//			{
//				AudioEvent.Play(AudioEventName.Holocron.Enter, Holocron);
//
//				// animating in the holocron if FTUE is complete
//				// TODO: update HolocronFX location scene, so that it displays properly
//				if (ShouldHolocronAnimate)
//				{
//					Holocron.GetComponent<Animator>().Play("Enter");
//
//					// TODO: mathh010 get Enter animation complete event instead
//					Invoke("DelayedShowGalaxy", 3.25f);
//				}
//				// showing the galaxy right away (since holocron is not animating in)
//				else
//				{
//					// Show Galaxy
//					selectedNode.OnAnimationComplete += AnimationComplete;
//					selectedNode.Animator.Play("galaxy_hiddenToPrimary");
//					PlayStartupAudio();
//				}
//			}

			CurvedUIController.Setup(DarkSidePillars);
		}

		public override void BackSelected()
		{
			if (previousMenuNode == null)
			{
				return;
			}

			focusedNode = previousMenuNode;
			NodeSelected();
		}

		public override void NodeSelected(MenuNode node)
		{
			// selected is galaxy
////			selectedNode = Galaxy.GetComponent<MenuNode>();

			// focused node is planet
			focusedNode = node;

			NodeSelected();
		}

		/// <summary>
		/// Select the current focused node.
		/// </summary>
		public override void NodeSelected()
		{
			// Nothing selected, or we are animating, or you selected something in the same view as you're aleady in.
			if (Animating || focusedNode == null)
			{
				return;
			}

			// Clicked the same pillar twice
			if (selectedPillarNode != null && !selectedPillarNode.Locked && focusedNode == selectedPillarNode)
			{
				DeepLinkOnLoad = true;
				DifficultyToLoad = CurvedUIController.CurrentDifficulty;
				GetComponent<Animator>().Play("menuFull_hide");
				AudioEvent.Play(AudioEventName.GalaxyMap.LaunchActivity, gameObject);
				StartCoroutine(LaunchGame());
				return;
			}

			if (selectedPillarNode != null)
			{
				selectedPillarNode.Selected = false;
				selectedPillarNode = null;
			}

			// Pillar Selected, show the screen.
			if (focusedNode.NodeType == MenuNodeType.Pillar)
			{
				selectedPillarNode = (PillarMenuNode)focusedNode;
				selectedPillarNode.Selected = true;

				CurvedUIController.Setup(selectedPillarNode);

				selectedPillarNode.AnimatePillar();

				AudioEvent.Play(AudioEventName.GalaxyMap.BeginActivity, gameObject);

				return;
			}
		}

		#endregion

		#region Private Methods

		private void PlayStartupAudio()
		{
			AudioEvent.Play(AudioEventName.Holocron.CornerSpinIdle, Holocron);

			// Play everytime after the first time load
			ContainerAPI container = new ContainerAPI(Game.ForceVision);

			firstTimeRun = false;
			container.PlayerPrefs.SetPrefInt(Constants.GalaxyLoadedPlayerPrefKey, 1);
		}

		private IEnumerator LaunchGame()
		{
			yield return new WaitForSeconds(Constants.GalaxyHideTime);
			selectedPillarNode.Launch();
		}

		private void OnItemGazedAt(object sender, GazeEventArgs eventArguments)
		{
			if (eventArguments.Hit.GetComponent<CurvedUIButton>() != null)
			{
				CurvedUIController.GazedAt(eventArguments.Hit.GetComponent<CurvedUIButton>());
				return;
			}

			MenuNode hitNode = eventArguments.Hit.GetComponent<MenuNode>();

			// Maybe we hit a reference node
			if (hitNode == null)
			{
				ReferenceNode reference = eventArguments.Hit.GetComponent<ReferenceNode>();
				hitNode = (reference != null) ? reference.Node : null;
			}

			// Still nothing
			if (hitNode == null)
			{
				return;
			}

			MenuNode node = null;

			node = hitNode;

			// If the node changed
			if (node != null && node != focusedNode)
			{
				// De-select the old one, if one.
				if (focusedNode != null)
				{
					focusedNode.Focused = false;
				}

				// Change the old one and select it
				focusedNode = node;
				focusedNode.Focused = true;
			}
		}

		private void OnItemGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (eventArguments.Hit.GetComponent<CurvedUIButton>() != null)
			{
				CurvedUIController.GazedOff(eventArguments.Hit.GetComponent<CurvedUIButton>());
				return;
			}

			// Nothing is looked at.
			if (GazeWatcher.CurrentGazedItems.Count <= 0 && focusedNode != null)
			{
				focusedNode.Focused = false;
				focusedNode = null;
			}
		}

		private IEnumerator CheckUntilPillarsForDeepLinkOnLoad()
		{
			while (pillars == null)
			{
				yield return new WaitForEndOfFrame();
			} 
			isWaitingForPillarsForDeepLinkOnLoadAction = false;
			DelayedDeepLinkOnLoadAction();
		}

		private void DelayedDeepLinkOnLoadAction()
		{

			// Reset
			DeepLinkOnLoad = false;
			ConfigToLoad = null;
		}

		private void AnimationComplete(object sender, AnimationEventArgs eventArgs)
		{

			lastNode.OnAnimationComplete -= AnimationComplete;
			selectedNode.OnAnimationComplete -= AnimationComplete;

			// Reattach after animations
			if (selectedNode.NodeType == MenuNodeType.Galaxy && lastNode.NodeType == MenuNodeType.Planet)
			{
				lastNode.GetRootTransform().SetParent(lastNodeParent, true);
			}

			// displaying holocron effects (if needed)
			for(int i = 0; i < HolocronFX.transform.childCount; i++)
			{
				Transform child = HolocronFX.transform.GetChild(i);
				if (!child.gameObject.activeSelf)
				{
					child.gameObject.SetActive(true);
				}
			}

			Animating = false;
		}

		private string GetGameName(Game game)
		{
			switch (game) 
			{
				case Game.Duel:
					return	"Guardian";
				case Game.TowerDefense:
					return "Commander";
				case Game.HoloChess:
					return "Consular";
				default:
					return "";
			}
		}

		#endregion
	}
}