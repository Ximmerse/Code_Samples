using UnityEngine;
using SG.Lonestar;
using BSG.SWARTD;
using Disney.AssaultMode;
using Disney.HoloChess;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public class HolocronMasterElement : NavigationElement
	{
		#region Public Properties

		[Header("The GameObjects to toggle based on completness")]
		public GameObject NotStarted;
		public GameObject InProgress;
		public GameObject Complete;

		[Header("The Configs that launches the given game / level")]
		public PillarConfig[] Configs;

		[Header("The Planet this game / level is under")]
		public PlanetType Planet;

		[Header("The Game Type for deep linking")]
		public Game Game;

		[Header("Only used for Duel and Assault")]
		public int Difficulty = 0;

		[Header("Holocron Mastery Menu")]
		public AnimationEvents MenuAnimEvent;

		#endregion

		#region Private Properties

		private Transform parent;
		private int siblingIndex;
		private CanvasGroup canvas;
		private Animator animator;
		private bool allowGaze;

		#endregion

		#region Unity Methods

		public void Start()
		{
			UpdateState();
			animator = GetComponent<Animator>();
			parent = transform.parent;
			siblingIndex = transform.GetSiblingIndex();
			canvas = gameObject.GetComponentInParent<CanvasGroup>();
			allowGaze = false;
		}

		void OnEnable()
		{
			if (MenuAnimEvent != null)
			{
				MenuAnimEvent.OnAnimationComplete += OnAnimationComplete;
			}
		}

		void OnDisable()
		{
			if (MenuAnimEvent != null)
			{
				MenuAnimEvent.OnAnimationComplete -= OnAnimationComplete;
			}

			allowGaze = false;
		}

		#endregion

		#region Public Methods

		public override void OnClicked()
		{
			#if IS_DEMO_BUILD
			Log.Debug("don't link during demo.");
			return;
			#endif

			if (NotStarted.activeSelf)
			{
				return;
			}

			// Deep Link
			MenuController.DeepLinkOnLoad = true;
			MenuController.DeepLinkToPlanet = Planet;
			MenuController.DeepLinkToBonusPlanet = null;
			MenuController.DeepLinkToGame = Game;

			// TODO: this won't actually play, because the scene immediately changes.
			// It needs to be attached to something that will persist through the scene change. -- rhg
			AudioEvent.Play(AudioEventName.ProgressionUI.GoToTrial, MenuAnimEvent.gameObject);

			(new ContainerAPI(Game.ForceVision)).LoadNextScene(true, false);
		}

		public override void OnGazedAt()
		{
			if (NotStarted.activeSelf || !allowGaze)
			{
				return;
			}

			animator.Play("holocronNode_onGaze");

			// Pop out and reparent and adjust alpha.
			if (MenuAnimEvent)
			{
				transform.SetParent(MenuAnimEvent.transform, true);
			}
			canvas.alpha = 0.25f;
		}

		public override void OnGazedOff()
		{
			if (NotStarted.activeSelf || !allowGaze)
			{
				return;
			}

			// Pop in and reparent and adjust alpha.
			transform.SetParent(parent, true);
			transform.SetSiblingIndex(siblingIndex);
			canvas.alpha = 1.0f;

			animator.Rebind();
			animator.Play("holocronNode_onGazeOut");
		}

		public void UpdateState()
		{
			if (Configs == null)
			{
				return;
			}

			bool locked = true;
			int complete = 0;
			int total = Configs.Length;

			for (int i = 0; i < total; i++)
			{
				if (i == 0)
				{
					// Exception when playing on hard (Not Core)
					if (Configs[i].Planet != PlanetType.Core && Difficulty == 3 && Configs[i].Game == Game.Assault && ContainerAPI.GetDuelApi().Progress.HasCompleted(DuelAPI.Duelist.Archivist, 2))
					{
						locked = false;
					}
					else
					{
						locked = ContainerAPI.IsLevelLocked(Configs[i], Difficulty) || ContainerAPI.IsPlanetLocked(Configs[i].Planet);
					}
				}

				if (ContainerAPI.IsLevelComplete(Configs[i], Difficulty))
				{
					complete++;
				}
			}

			NotStarted.SetActive(locked);
			InProgress.SetActive(!locked && complete < total);
			Complete.SetActive(total == complete);
		}

		#endregion

		#region Event Handlers

		protected void OnAnimationComplete(object sender, AnimationEventArgs eventArgs)
		{
			allowGaze = (eventArgs.AnimationName == AnimationEventName.HolocronMasteryMenuIntro);
		}

		#endregion
	}
}