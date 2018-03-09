using UnityEngine;
using System;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	public class MenuNode : MonoBehaviour
	{
		#region Event Handlers

		public static EventHandler<MenuNodeEventArgs> OnNodeSelected;
		public static EventHandler<MenuNodeEventArgs> OnNodeFocused;
		public static EventHandler<MenuNodeEventArgs> OnNodeIdle;

		#endregion

		#region Constants

		private const float NodeIdleTime = 7f;

		#endregion

		#region Public Properties 

		/// <summary>
		/// The type of the node.
		/// </summary>
		public MenuNodeType NodeType;

		/// <summary>
		/// The selected indicator.
		/// </summary>
		[Header("Used for gaze selection, active when gazed at, inactive when not.")]
		public GameObject SelectedIndicator;

		/// <summary>
		/// The animator.
		/// </summary>
		public Animator Animator;

		/// <summary>
		/// The root game object if not this one.
		/// </summary>
		public GameObject RootGameObject;

		/// <summary>
		/// When an animation is compelte broadcast an event.
		/// </summary>
		public Action<object, AnimationEventArgs> OnAnimationComplete;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Disney.ForceVision.MenuNode"/> is selected.
		/// </summary>
		/// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
				OnSelectUpdated(selected);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Disney.ForceVision.MenuNode"/> is focused.
		/// </summary>
		/// <value><c>true</c> if focused; otherwise, <c>false</c>.</value>
		public bool Focused 
		{
			get
			{
				return focused;
			}

			set
			{
				focused = value;
				OnFocusUpdated(focused);
			}
		}

		#endregion

		#region Private Properties

		private bool focused = false;
		private AnimationEvents animationEvents;
		private bool selected = false;
		private float idleTime = 0f;
		private bool idleEventDispatched = false;

		#endregion

		#region Unity Methods

		protected virtual void Start()
		{
			animationEvents = Animator.GetComponent<AnimationEvents>();

			if (animationEvents != null)
			{
				animationEvents.OnAnimationComplete += AnimationCompleted;
			}
		}

		protected void FixedUpdate()
		{
			if (Focused)
			{
				idleTime += Time.fixedDeltaTime;
				if (idleTime >= NodeIdleTime && !idleEventDispatched)
				{
					OnIdleTimeReached();
					idleEventDispatched = true;
				}
			}
		}

		protected void OnDestroy()
		{
			if (animationEvents != null)
			{
				animationEvents.OnAnimationComplete -= AnimationCompleted;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the root transform for this node.
		/// </summary>
		/// <returns>The root transform.</returns>
		public Transform GetRootTransform()
		{
			return (RootGameObject != null) ? RootGameObject.transform : transform;
		}

		#endregion

		#region Private Methods

		private void AnimationCompleted(object sender, AnimationEventArgs eventArguments)
		{
			if (OnAnimationComplete != null)
			{
				OnAnimationComplete.Invoke(sender, eventArguments);
			}
		}

		#endregion

		#region Protected Methods 

		protected virtual void OnFocusUpdated(bool state)
		{
			if (SelectedIndicator != null)
			{
				SelectedIndicator.SetActive(state);
			}

			// reset the idle time and event flag when losing focus
			if (!state)
			{
				idleTime = 0f;
				idleEventDispatched = false;
			}
		}

		/// <summary>
		/// Raises the idle updated event.
		/// Marked as virtual since MenuNode is a MonoBehaviour, and you can not declare MonoBehaviour as abstract
		/// </summary>
		protected virtual void OnIdleTimeReached()
		{
		}

		/// <summary>
		/// Raises the select updated event.
		/// Marked as virtual since MenuNode is a MonoBehaviour, and you can not declare MonoBehaviour as abstract
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		protected virtual void OnSelectUpdated(bool state)
		{
		}

		#endregion
	}
}