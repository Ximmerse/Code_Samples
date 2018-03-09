using UnityEngine;
using System;

namespace Disney.ForceVision
{
	public class AnimationEvents : MonoBehaviour
	{
		#region Public Properties

		/// <summary>
		/// When the animation is complete this event is invoked.
		/// </summary>
		public EventHandler<AnimationEventArgs> OnAnimationComplete;

		#endregion

		#region Public Methods

		/// <summary>
		/// Called from the animation system when the animation is complete.
		/// </summary>
		/// <param name="animationName">Animation name.</param>
		public void AnimationComplete(string animationName)
		{
			if (OnAnimationComplete != null)
			{
				OnAnimationComplete.Invoke(this, new AnimationEventArgs(animationName));
			}
		}

		/// <summary>
		/// Sets this object to inactive.
		/// </summary>
		public void SetInactive()
		{
			gameObject.SetActive(false);
		}

		#endregion
	}
}