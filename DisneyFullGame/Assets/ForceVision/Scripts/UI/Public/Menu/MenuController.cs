using UnityEngine;
using UnityEngine.SceneManagement;
using Disney.Vision.Internal;
using System.Collections;
using SG.ProtoFramework;

namespace Disney.ForceVision
{
	/// <summary>
	/// Menu node type.
	/// </summary>
	public enum MenuNodeType
	{
		Galaxy = 0,
		Planet,
		Surface,
		Pillar,
		Porg
	}

	public class MenuController : MonoBehaviour
	{
		public static bool DeepLinkOnLoad;
		public static PlanetType DeepLinkToPlanet;
		public static BonusPlanetType? DeepLinkToBonusPlanet = null;
		public static Game DeepLinkToGame;
		public static bool UnlockedPillar;

		public static Difficulty DifficultyToLoad;
		public static PillarConfig ConfigToLoad;

		public static bool ShouldHolocronAnimate = true;
		public static GameObject HolocronObjectToDestroy;

		#region Public Properties

		/// <summary>
		/// The gaze watcher.
		/// </summary>
		public GazeWatcher GazeWatcher;

		/// <summary>
		/// The galaxy.
		/// </summary>
		public GameObject Galaxy;

		/// <summary>
		/// The holocron.
		/// </summary>
		public GameObject Holocron;

		/// <summary>
		/// The holocron fx.
		/// </summary>
		public GameObject HolocronFX;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Disney.ForceVision.MenuController"/> is animating.
		/// </summary>
		/// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
		public bool Animating { get; set; }

		/// <summary>
		/// The bonus planet unlock FX
		/// </summary>
		public GameObject BonusPlanetUnlockFX;

		/// <summary>
		/// The crait display animator.
		/// </summary>
		public Animator CraitDisplayAnimator;

		/// <summary>
		/// The rotation controller.
		/// </summary>
		public InputRotation RotationController;

		public PlanetMenuNode CraitPlanetNode;

		#endregion

		#region Private Properties

		protected static bool firstTimeRun = true;

		protected MenuNode focusedNode = null;

		protected MenuNode selectedNode;
		protected Transform selectedNodeParent;

		protected MenuNode lastNode;
		protected Transform lastNodeParent;

		protected PillarMenuNode selectedPillarNode;
		protected PillarContainer pillars;

		protected GazeListener gazeListener;

		protected MenuNode previousMenuNode = null;

		protected bool isWaitingForPillarsForDeepLinkOnLoadAction;

		protected Difficulty? deepLinkDifficulty = null;

		protected AnimationEvents craitAnimationEvents;

		/// <summary>
		/// The stereo camera.
		/// </summary>
		protected StereoCamera stereoCamera
		{
			get
			{
				return GazeWatcher.Sdk.StereoCamera;
			}
		}

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

			lastNode = selectedNode = Galaxy.GetComponent<MenuNode>();

			SceneManager.sceneLoaded += OnSceneLoadedDoPillarSetup;
			SceneManager.LoadScene("Pillars", LoadSceneMode.Additive);

			if (DeepLinkOnLoad)
			{
				deepLinkDifficulty = DifficultyToLoad;
			}
		}

		private void OnDestroy()
		{
			GazeWatcher.RemoveListener(gazeListener);

			if (craitAnimationEvents != null)
			{
				craitAnimationEvents.OnAnimationComplete -= OnCraitAnimationEventHandler;
			}

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
			// Check scene name in case other sub-scenes are loaded.
			if (scene.name == "Pillars")
			{
				SceneManager.sceneLoaded -= OnSceneLoadedDoPillarSetup;

				// Reparent
				GameObject pillarContainer = GameObject.Find("PillarsContainer");
				pillarContainer.transform.SetParent(Galaxy.transform.parent, true);

				LookAtTransform[] lookAts = pillarContainer.GetComponentsInChildren<LookAtTransform>(true);
				foreach (LookAtTransform lookAt in lookAts)
				{
					lookAt.Transform = stereoCamera.transform;
				}
				pillars = pillarContainer.GetComponent<PillarContainer>();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Setup this instance.
		/// </summary>
		public virtual void Setup()
		{
			// Show the Holocron and Animate the Galaxy
			Animating = true;

			if (DeepLinkOnLoad)
			{
				// Galaxy
				Galaxy.GetComponent<MenuNode>().OnAnimationComplete += AnimationComplete;
				Galaxy.GetComponent<Animator>().enabled = true;
				Galaxy.GetComponent<Animator>().Play("galaxy_hiddenToSetHeight");
				HolocronFX.SetActive(true);
				PlayStartupAudio();
			}
			else
			{
				AudioEvent.Play(AudioEventName.Holocron.Enter, Holocron);

				// animating in the holocron if FTUE is complete
				// TODO: update HolocronFX location scene, so that it displays properly
				if (ShouldHolocronAnimate)
				{
					Holocron.GetComponent<Animator>().Play("Enter");

					// TODO: mathh010 get Enter animation complete event instead
					Invoke("DelayedShowGalaxy", 3.25f);
				}
				// showing the galaxy right away (since holocron is not animating in)
				else
				{
					// Show Galaxy
					selectedNode.OnAnimationComplete += AnimationComplete;
					selectedNode.Animator.Play("galaxy_hiddenToPrimary");
					PlayStartupAudio();
				}
			}
		}

		private void DelayedShowGalaxy()
		{
			// Show Holocron FX
			HolocronFX.SetActive(true);

			PlayStartupAudio();

			// Show Galaxy
			selectedNode.OnAnimationComplete += AnimationComplete;
			selectedNode.Animator.Play("galaxy_hiddenToPrimary");
		}

		public virtual void BackSelected()
		{
			if (previousMenuNode == null)
			{
				return;
			}

			focusedNode = previousMenuNode;
			NodeSelected();
		}

		public virtual void NodeSelected(MenuNode node)
		{
			// selected is galaxy
			selectedNode = Galaxy.GetComponent<MenuNode>();

			// focused node is planet
			focusedNode = node;

			NodeSelected();
		}

		/// <summary>
		/// Select the current focused node.
		/// </summary>
		public virtual void NodeSelected()
		{
			// Difficulty Button
			if (pillars != null && pillars.UIController.GazeClick())
			{
				return;
			}

			// Nothing selected, or we are animating, or you selected something in the same view as you're aleady in.
			if (Animating || focusedNode == null || (focusedNode != null && focusedNode.NodeType == selectedNode.NodeType))
			{
				return;
			}

			// Locked Planet
			if (focusedNode is PlanetMenuNode && (focusedNode as PlanetMenuNode).Locked)
			{
				// Locked Planet Audio (4 cases)
				PlanetMenuNode planetNode = focusedNode as PlanetMenuNode;

				bool tdComplete = ContainerAPI.IsLevelComplete(planetNode.PreviousTDConfig, 1);
				bool chessComplete = ContainerAPI.IsLevelComplete(planetNode.PreviousChessConfig, 1);
				bool playAnimation = true;

				if (tdComplete && chessComplete)
				{
					AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PlanetLockedBoth, Holocron);
				}
				else if (tdComplete)
				{
					AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PlanetLockedTowerDefense, Holocron);
				}
				else if (chessComplete)
				{
					AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PlanetLockedChess, Holocron);
				}
				else
				{
					AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.PlanetLockedGeneral, Holocron);
					playAnimation = false;
				}

				if (playAnimation)
				{
					GameObject lockedEffect = (focusedNode as PlanetMenuNode).PreviousPlanetEffect;
					if (lockedEffect != null)
					{
						lockedEffect.SetActive(true);
					}
				}

				AudioEvent.Play(AudioEventName.GalaxyMap.NodeLocked, gameObject);

				return;
			}

			// Clicked the same pillar twice
			if (selectedPillarNode != null && !selectedPillarNode.Locked && focusedNode == selectedPillarNode)
			{
				DeepLinkOnLoad = true;
				DifficultyToLoad = pillars.UIController.CurrentDifficulty;
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

				pillars.UIController.Setup(selectedPillarNode);

				selectedPillarNode.AnimatePillar();

				AudioEvent.Play(AudioEventName.GalaxyMap.BeginActivity, gameObject);

				return;
			}

			if (focusedNode != null && focusedNode is PorgMenuNode)
			{
				focusedNode.Selected = true;

				return;
			}

			// Animating and set the new current view
			Animating = true;

			// Last and Current Nodes
			lastNode = selectedNode;
			lastNodeParent = selectedNodeParent;

			selectedNode = focusedNode;
			selectedNode.Focused = false;
			focusedNode = null;

			// Selected
			lastNode.Selected = false;
			selectedNode.Selected = true;

			// Galaxy to Planet
			if (selectedNode.NodeType == MenuNodeType.Planet && lastNode.NodeType == MenuNodeType.Galaxy)
			{
				PlanetMenuNode currentPlanetNode = ((PlanetMenuNode)selectedNode);
				previousMenuNode = lastNode;
				if (currentPlanetNode.IsBonusPlanet)
				{
					DeepLinkToBonusPlanet = currentPlanetNode.BonusPlanet;
				}
				else
				{
					DeepLinkToPlanet = currentPlanetNode.Planet;
					DeepLinkToBonusPlanet = null;
				}

				// Break Planet Out
				selectedNodeParent = selectedNode.GetRootTransform().parent;
				selectedNode.GetRootTransform().SetParent(Galaxy.transform.parent, true);

				// Animate Planet
				selectedNode.Animator.Play("planet_restToPrimary");
				((PlanetMenuNode)selectedNode).SurfaceAnimator.Play("planetSurface_show");
				((PlanetMenuNode)selectedNode).FacePlayer(stereoCamera.transform);

				// Face player
				pillars.FacePlayer(stereoCamera.transform);

				// Animate Galaxy
				lastNode.OnAnimationComplete += AnimationComplete;
				lastNode.Animator.Play("galaxy_primaryToSecondary");

				// Animate New Pillars
				if (!currentPlanetNode.IsBonusPlanet)
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)selectedNode).Planet).Play("planetPillars_hiddenToPlanet");
				}
				else
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)selectedNode).BonusPlanet).Play("planetPillars_hiddenToPlanet");
				}

				// Show 3rd Pillar Pin if needed
				pillars.CheckToShowSecondLevelPin(((PlanetMenuNode)selectedNode).Planet);
			}

			// Planet to Galaxy
			else if (selectedNode.NodeType == MenuNodeType.Galaxy && lastNode.NodeType == MenuNodeType.Planet)
			{
				PlanetMenuNode lastPlanet = (PlanetMenuNode)lastNode;

				// Animate Galaxy
				previousMenuNode = null;
				selectedNode.Animator.Play("galaxy_secondaryToPrimary");

				// Animate Planet
				//disabling to potentially fix DCM-1751
				//NavigationController.IsWaitingForAnimationToComplete = true;
				lastNode.OnAnimationComplete += AnimationComplete;
				lastNode.Animator.Play("planet_primaryToRest");
				((PlanetMenuNode)lastNode).SurfaceAnimator.Play("planetSurface_hide");

				// Animate New Pillars
				if (!lastPlanet.IsBonusPlanet)
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)lastNode).Planet).Play("planetPillars_planetToHidden");

					// Hide 3rd Pillar Pin if needed
					pillars.CheckToHideSecondLevelPin(((PlanetMenuNode)lastNode).Planet);
				}
				else
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)lastNode).BonusPlanet).Play("planetPillars_planetToHidden");
					// Hide 3rd Pillar Pin if needed
					//pillars.CheckToHideSecondLevelPin(((PlanetMenuNode)lastNode).BonusPlanet);
				}
			}

			// Planet to Surface
			else if (selectedNode.NodeType == MenuNodeType.Surface && lastNode.NodeType == MenuNodeType.Planet)
			{
				PlanetMenuNode lastPlanetNode = (PlanetMenuNode)lastNode;

				previousMenuNode = lastNode;
				DeepLinkToGame = ((SurfaceMenuNode)selectedNode).LaunchGame;

				if (!lastPlanetNode.IsBonusPlanet)
				{
					pillars.UIController.Setup((SurfaceMenuNode)selectedNode, ((PlanetMenuNode)lastNode).Planet);
				}
				else
				{
					pillars.UIController.Setup((SurfaceMenuNode)selectedNode, ((PlanetMenuNode)lastNode).BonusPlanet);
				}

				Galaxy.GetComponent<Animator>().Play("galaxy_secondaryToTertiary");

				if (!lastPlanetNode.IsBonusPlanet)
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)lastNode).Planet).Play("planetPillars_planetTo" + GetGameName(((SurfaceMenuNode)selectedNode).LaunchGame));
				}
				else
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)lastNode).BonusPlanet).Play("planetPillars_planetTo" + GetGameName(((SurfaceMenuNode)selectedNode).LaunchGame));
				}

				lastNode.OnAnimationComplete += AnimationComplete;
				lastNode.Animator.Play("planet_primaryToSecondary");
				((PlanetMenuNode)lastNode).SurfaceAnimator.Play("planetSurface_hide");

				AudioEvent.Play(AudioEventName.GalaxyMap.SelectActivity, gameObject);

				// Play the ambience for the planet surface
				if (!lastPlanetNode.IsBonusPlanet)
				{
					AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + ((PlanetMenuNode)lastNode).Planet + "_Play",
					                selectedNode.gameObject);
				}
				// Play the ambience for bonus planet
				else
				{
					AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + ((PlanetMenuNode)lastNode).BonusPlanet + "_Play", selectedNode.gameObject);

					// checking for assault mode completion on easy
					if(ConfigToLoad != null && ContainerAPI.IsLevelComplete(ConfigToLoad, 1))
					{
						AudioEvent.PlayOnceEver("MAP_DX_Arch_Crait_000_004", selectedNode.gameObject);
					}
				}
			}

			// Surface to Planet
			else if (selectedNode.NodeType == MenuNodeType.Planet && lastNode.NodeType == MenuNodeType.Surface)
			{
				PlanetMenuNode selectedPlanet = (PlanetMenuNode)selectedNode;

				previousMenuNode = Galaxy.GetComponent<MenuNode>();
				pillars.UIController.Hide();

				// Hide Lock Icons
				PillarMenuNode[] pillarNodes = lastNode.gameObject.GetComponentsInChildren<PillarMenuNode>();

				for (int i = 0; i < pillarNodes.Length; i++)
				{
					pillarNodes[i].LockObject.SetActive(false);
				}

				Galaxy.GetComponent<Animator>().Play("galaxy_tertiaryToSecondary");

				if (!selectedPlanet.IsBonusPlanet)
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)selectedNode).Planet).Play("planetPillars_" + GetGameName(((SurfaceMenuNode)lastNode).LaunchGame).ToLower() + "ToPlanet");
				}
				else
				{
					pillars.GetPlanetAnimator(((PlanetMenuNode)selectedNode).BonusPlanet).Play("planetPillars_" + GetGameName(((SurfaceMenuNode)lastNode).LaunchGame).ToLower() + "ToPlanet");
				}

				selectedNode.OnAnimationComplete += AnimationComplete;
				selectedNode.Animator.Play("planet_secondaryToPrimary");
				((PlanetMenuNode)selectedNode).SurfaceAnimator.Play("planetSurface_show");
				((PlanetMenuNode)selectedNode).FacePlayer(stereoCamera.transform);

				// Show 3rd Pillar Pin if needed
				if (!selectedPlanet.IsBonusPlanet)
				{
					pillars.CheckToShowSecondLevelPin(((PlanetMenuNode)selectedNode).Planet);
				}
				else
				{
					pillars.CheckToShowSecondLevelPin(((PlanetMenuNode)selectedNode).BonusPlanet);
				}

				AudioEvent.Play(AudioEventName.GalaxyMap.SelectBack, gameObject);

				// Stop the ambience for the planet surface
				if (!selectedPlanet.IsBonusPlanet)
				{
					AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + ((PlanetMenuNode)selectedNode).Planet + "_Stop",
					                lastNode.gameObject);
				}
				else
				{
					AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + ((PlanetMenuNode)selectedNode).BonusPlanet + "_Stop",
					                lastNode.gameObject);
				}
			}
		}


		#endregion

		#region Private Methods

		private void PlayStartupAudio()
		{
			AudioEvent.Play(AudioEventName.Holocron.CornerSpinIdle, Holocron);

			// Play everytime after the first time load
			ContainerAPI container = new ContainerAPI(Game.ForceVision);

			if (firstTimeRun && container.PlayerPrefs.GetPrefInt(Constants.GalaxyLoadedPlayerPrefKey, 0) == 1)
			{
				switch (ContainerAPI.GetJediRank())
				{
					case JediRank.Initiate:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.GalaxyLoadRankInitiate, gameObject);
					break;

					case JediRank.Padawan:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.GalaxyLoadRankPadawan, gameObject);
					break;

					case JediRank.Knight:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.GalaxyLoadRankJediKnight, gameObject);
					break;

					case JediRank.Master:
						AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.GalaxyLoadRankJediMaster, gameObject);
					break;
				}
			}
			else if (container.PlayerPrefs.GetPrefInt(Constants.GalaxyLoadedPlayerPrefKey, 0) == 0)
			{
				AudioEvent.PlayOnceEver(AudioEventName.Holocron.FirstTimeAppears, Holocron);
			}

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
				pillars.UIController.GazedAt(eventArguments.Hit.GetComponent<CurvedUIButton>());
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

			// When on the galaxy, only select planets
			if (selectedNode.NodeType == MenuNodeType.Galaxy && hitNode.NodeType == MenuNodeType.Planet)
			{
				node = hitNode;
			}

			// When on a planet, can only focus on anything other then other planets
			else if (selectedNode.NodeType == MenuNodeType.Planet && hitNode.NodeType != MenuNodeType.Pillar && hitNode.NodeType != MenuNodeType.Planet)
			{
				node = hitNode;
			}

			// When on the surface, can only focus on pillar or last planet
			else if (selectedNode.NodeType == MenuNodeType.Surface && (hitNode == lastNode || hitNode.NodeType == MenuNodeType.Pillar))
			{
				node = hitNode;
			}

			// If the node changed
			if (node != null && node != focusedNode && node.NodeType != selectedNode.NodeType)
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
				pillars.UIController.GazedOff(eventArguments.Hit.GetComponent<CurvedUIButton>());
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
			if (Galaxy == null || Holocron == null)
			{
				return;
			}

			// Galaxy
			Galaxy.GetComponent<Animator>().Play("galaxy_hiddenToTertiary");

			// Planet
			GameObject planet;
			if (DeepLinkToBonusPlanet == null)
			{
				planet = Galaxy.transform.Find("Planets/" + DeepLinkToPlanet + "_rotationOffset/planetRot/planet").gameObject;
			}
			else
			{
				planet = Galaxy.transform.Find("Planets/" + DeepLinkToBonusPlanet.Value + "_rotationOffset/planetRot/planet").gameObject;
			}

			PlanetMenuNode planetNode = planet.GetComponent<PlanetMenuNode>();

			if (planetNode == null)
			{
				return;
			}

			planetNode.FacePlayer(stereoCamera.transform);
			selectedNodeParent = planetNode.GetRootTransform().parent;
			planetNode.GetRootTransform().SetParent(Galaxy.transform.parent, true);
			planetNode.Animator.Play("planet_hiddenToSecondary");

			// Pillars
			Animator animator;
			if (!planetNode.IsBonusPlanet)
			{
				animator = pillars.GetPlanetAnimator(planetNode.Planet);
			}
			else
			{
				animator = pillars.GetPlanetAnimator(planetNode.BonusPlanet);
			}
			animator.Play("planetPillars_planetTo" + GetGameName(DeepLinkToGame));
			pillars.FacePlayer(stereoCamera.transform);

			// Holocron
			Holocron.GetComponent<Animator>().Play("ClosedIdle");

			// Setup selected and last selected
			lastNode = planetNode;
			selectedNode = animator.transform.Find("Pillars_" + GetGameName(DeepLinkToGame)).GetComponentInChildren<SurfaceMenuNode>(true);

			// Ambience
			if (!planetNode.IsBonusPlanet)
			{
				AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + DeepLinkToPlanet + "_Play", selectedNode.gameObject);
			}
			else
			{
				AudioEvent.Play("MAP_SFX_GalaxyMap_Ambience_" + DeepLinkToBonusPlanet.Value + "_Play", selectedNode.gameObject);
			}

			// Find the right pillar and select it
			PillarMenuNode[] pillarNodes = selectedNode.transform.GetComponentsInChildren<PillarMenuNode>(true);

			for (int i = 0; i < pillarNodes.Length; i++)
			{
				if (pillarNodes[i].Config == ConfigToLoad)
				{
					selectedPillarNode = pillarNodes[i];
					selectedPillarNode.Selected = true;
					selectedPillarNode.AnimatePillar();
					break;
				}
			}

			// Find next pillar
			if (MenuController.UnlockedPillar)
			{
				for (int i = 0; i < pillarNodes.Length; i++)
				{
					if (pillarNodes[i].Config.PreviousConfig == selectedPillarNode.Config)
					{
						// Turn on the effect
						if (pillarNodes[i].FirstUnlockEffect != null)
						{
							pillarNodes[i].FirstUnlockEffect.SetActive(true);

							// Play the audio
							if (pillarNodes[i].Config.Game == Game.Duel)
							{
								string audioEvent = AudioEventName.Archivist.GalaxyMap.DuelPillarUnlock[(int)DifficultyToLoad,
								                                                                        (int)pillarNodes[i].Config.Duelist];

								if (!string.IsNullOrEmpty(audioEvent))
								{
									AudioEvent.Play(audioEvent, Holocron);
								}
							}
						}

						break;
					}
				}

				MenuController.UnlockedPillar = false;
			}

			// Bent Screen
			if (!planetNode.IsBonusPlanet)
			{
				pillars.UIController.Setup(selectedNode as SurfaceMenuNode, planetNode.Planet);
			}
			else
			{
				pillars.UIController.Setup(selectedNode as SurfaceMenuNode, planetNode.BonusPlanet);
			}

			if (selectedPillarNode != null)
			{
				pillars.UIController.Setup(selectedPillarNode, deepLinkDifficulty);
			}

			// Reset
			DeepLinkOnLoad = false;
			ConfigToLoad = null;
		}

		private void AnimationComplete(object sender, AnimationEventArgs eventArgs)
		{
			if (eventArgs.AnimationName.Equals("planet_primaryToRest"))
			{
				NavigationController.IsWaitingForAnimationToComplete = false;
			}

			if (DeepLinkOnLoad)
			{
				if (!isWaitingForPillarsForDeepLinkOnLoadAction)
				{
					isWaitingForPillarsForDeepLinkOnLoadAction = true;
					StartCoroutine(CheckUntilPillarsForDeepLinkOnLoad());
				}
				return;
			}

			// galaxy animation completed
			if (eventArgs.AnimationName.Equals("GalaxyMapTurnsOn"))
			{
				ContainerAPI container = new ContainerAPI(Game.ForceVision);

				// checking if player has defeated Grand Inquisitor
				bool hasDefeatedGrandInquisitor = ContainerAPI.GetDuelApi().Progress.HasCompleted(SG.Lonestar.DuelAPI.Duelist.GrandInquisitor, 1);

				// checking if Crait unlock animation has played already
				bool hasCraitUnlockAnimationPlayed = container.PlayerPrefs.PrefKeyExists(Constants.CraitUnlocked);

				if (hasDefeatedGrandInquisitor && !hasCraitUnlockAnimationPlayed)
				{
					// checking for player rank
					JediRank playerRank = ContainerAPI.GetJediRank();
					if (playerRank <= JediRank.Padawan)
					{
						AudioEvent.Play(AudioEventName.GalaxyMap.UnlockCraitNewUser, Holocron, (object in_cookie, AkCallbackType in_type, object in_info) => {
							PlayCraitUnlockAnimation();
						});
					}
					else
					{
						// playing unlock animation
						PlayCraitUnlockAnimation();
					}

					// setting player pref that Crait unlock animation has played
					container.PlayerPrefs.SetPrefInt(Constants.CraitUnlocked, 1);
				}

				// checking if player has completed Crait content
				bool hasDefeatedPraetorianGuards = ContainerAPI.GetDuelApi().Progress.HasCompleted(SG.Lonestar.DuelAPI.Duelist.PraetorianGuards, 1);
				bool hasCompletedTowerDefense = BSG.SWARTD.TDAPI.GetInstance().HasWonBattle(BSG.SWARTD.TDAPI.Battles.Crait_3);
				bool hasPorgUnlockAnimationPlayed = container.PlayerPrefs.PrefKeyExists(Constants.PorgUnlocked);
				if (hasDefeatedPraetorianGuards && hasCompletedTowerDefense && !hasPorgUnlockAnimationPlayed)
				{
					// disabling user input on node selection
					Animating = true;

					DG.Tweening.DOVirtual.DelayedCall(3f, () => {
						MenuAudioController.DidAutoSelectNode = true;

						NodeSelected(CraitPlanetNode);

						pillars.PorgUnlockFX.SetActive(true);

						// enabling user input on node selection
						Animating = false;
					});

					// saving player prefs for porg unlocking
					container.PlayerPrefs.SetPrefInt(Constants.PorgUnlocked, 1);
				}
			}

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

		private void PlayCraitUnlockAnimation()
		{
			// playing animation
			CraitDisplayAnimator.gameObject.SetActive(true);
			craitAnimationEvents = CraitDisplayAnimator.GetComponent<AnimationEvents>();
			craitAnimationEvents.OnAnimationComplete += OnCraitAnimationEventHandler;
		}

		private void OnCraitAnimationEventHandler(object sender, AnimationEventArgs eventArgs)
		{
			string eventType = eventArgs.AnimationName;

			if (string.Equals(eventType, "startIntroBonusPlanet"))
			{
				BonusPlanetUnlockFX.SetActive(true);
				RotationController.SetTargetRotation(-120f);
			}

			if (string.Equals(eventType, "startExitBonusPlanet"))
			{
				RotationController.SetTargetRotation(0f);
				craitAnimationEvents.OnAnimationComplete -= OnCraitAnimationEventHandler;
			}
		}

		#endregion
	}
}