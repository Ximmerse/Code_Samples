using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public enum PlanetType
	{
		Naboo,
		Garel,
		Lothal,
		Hoth,
		Takodana,
		Core,
		DarkSide
	}

	public enum BonusPlanetType
	{
		Crait
	}

	public class PlanetMenuNode : MenuNode
	{
		#region Public Properties

		/// <summary>
		/// The surface animator.
		/// </summary>
		public Animator SurfaceAnimator;

		public float FaceSpeed = 2.0f;
		public PlanetType Planet;
		public Image LockTint;
		public bool IsBonusPlanet = false;
		public BonusPlanetType BonusPlanet;

		[Header("Previous Configs for Audio Events")]
		public PillarConfig PreviousTDConfig;
		public PillarConfig PreviousChessConfig;
		public GameObject PreviousPlanetEffect;

		#if IS_DEMO_BUILD
		public bool Locked
		{
			get
			{
				if (ContainerAPI.AllProgressionUnlocked || Planet == PlanetType.Naboo)
				{
					return false;
				}
				return true;
			}

			private set
			{ 
				if (Planet != PlanetType.Naboo)
				{
					value = false;
				}
			}
		}
		
		#else
		public bool Locked { get; private set; }
		#endif

		#endregion

		#region Private Properties

		private bool animateToFacePlayer = false;
		private Vector3 targetPosition;
		private Transform rotationHolder;
		private const float EndThreshold = 5.0f;
		private GameObject lockedGameObject;
		private Color lockedColor = new Color(0.05f, 0.73f, 0.95f);
		private Color unlockedColor = new Color(1.0f, 0.73f, 0.24f);

		#endregion

		#region Unity Methods

		protected void Awake()
		{
			if (IsBonusPlanet && BonusPlanet == BonusPlanetType.Crait)
			{
				Locked = ContainerAPI.IsPlanetLocked(PlanetType.Hoth);
			}
			else
			{
				Locked = ContainerAPI.IsPlanetLocked(Planet);
			}
		}

		protected override void Start()
		{
			rotationHolder = SelectedIndicator.transform.parent;
			lockedGameObject = LockTint.transform.GetChild(0).gameObject;

			UpdateLockedState();

			base.Start();
		}

		protected void Update()
		{
			if (animateToFacePlayer)
			{
				Vector3 lookPosition = targetPosition - rotationHolder.position;
				lookPosition.y = 0;
				Quaternion rotation = Quaternion.LookRotation(lookPosition);
				rotationHolder.rotation = Quaternion.Slerp(rotationHolder.rotation, rotation, Time.deltaTime * FaceSpeed);

				if (Mathf.Abs(Quaternion.Angle(rotationHolder.rotation, rotation)) < EndThreshold)
				{
					animateToFacePlayer = false;
				}
			}
		}

		#endregion

		#region Public Methods

		public void FacePlayer(Transform player)
		{
			targetPosition = player.position;
			animateToFacePlayer = true;
		}

		#endregion

		#region Protected Methods

		protected override void OnFocusUpdated(bool state)
		{
			base.OnFocusUpdated(state);

			if (state && OnNodeFocused != null)
			{
				if (!IsBonusPlanet)
				{
					OnNodeFocused(this, new MenuNodeEventArgs(NodeType, Planet));
				}
				else
				{
					OnNodeFocused(this, new MenuNodeEventArgs(NodeType, BonusPlanet));
				}
			}

			UpdateLockedState();
		}

		protected override void OnIdleTimeReached()
		{
			if (OnNodeIdle != null)
			{
				if (!IsBonusPlanet)
				{
					OnNodeIdle(this, new MenuNodeEventArgs(NodeType, Planet));
				}
				else
				{
					OnNodeIdle(this, new MenuNodeEventArgs(NodeType, BonusPlanet));
				}
			}
		}

		protected override void OnSelectUpdated(bool state)
		{
			if (state && OnNodeSelected != null)
			{
				if (!IsBonusPlanet)
				{
					OnNodeSelected(this, new MenuNodeEventArgs(NodeType, Planet));
				}
				else
				{
					OnNodeSelected(this, new MenuNodeEventArgs(NodeType, BonusPlanet));
				}
			}
		}

		#endregion

		#region Private Methods

		private void UpdateLockedState()
		{
			LockTint.color = (Locked) ? lockedColor : unlockedColor;
			lockedGameObject.SetActive(Locked);
		}

		#endregion
	}
}