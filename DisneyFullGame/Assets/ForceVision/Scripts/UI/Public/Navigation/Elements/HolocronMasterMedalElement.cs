using UnityEngine;

namespace Disney.ForceVision
{
	public class HolocronMasterMedalElement : NavigationElement
	{
		#region Public Properties

		[Header("Medal Type")]
		public MedalType Type;

		[Header("The GameObjects to toggle based on completness")]
		public GameObject Missing;
		public GameObject Normal;

		[Header("Holocron Mastery Menu")]
		public AnimationEvents MenuAnimEvent;

		#endregion

		#region Private Properties

		private Animator animator;
		private Transform parent;
		private int siblingIndex;
		private CanvasGroup canvas;
		private bool allowGaze;

		#endregion

		#region Unity Methods

		public void Start()
		{
			siblingIndex = transform.GetSiblingIndex();
			parent = transform.parent;
			canvas = gameObject.GetComponentInParent<CanvasGroup>();
			Normal.SetActive(false);
			allowGaze = false;

			if (ContainerAPI.IsMedalUnlocked(Type))
			{
				if (Type == MedalType.Combat && ContainerAPI.IsMedalUnlocked(MedalType.AdvancedCombat))
				{
					gameObject.SetActive(false);
				}

				if (Missing != null)
				{
					Missing.SetActive(false);
				}

				if (Normal != null)
				{
					Normal.SetActive(true);
				}
				animator = Normal.GetComponent<Animator>();
			}
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

		public override void OnGazedAt()
		{
			if (animator != null && allowGaze)
			{
				animator.Play("medal_onGaze");

				// Pop out and reparent and adjust alpha.
				if (MenuAnimEvent)
				{
					transform.SetParent(MenuAnimEvent.transform, true);
				}
				canvas.alpha = 0.25f;
			}
		}

		public override void OnGazedOff()
		{
			if (animator != null && allowGaze)
			{
				// Pop in and reparent and adjust alpha.
				transform.SetParent(parent, true);
				transform.SetSiblingIndex(siblingIndex);
				canvas.alpha = 1.0f;

				animator.Play("medal_onGazeOut");
			}
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