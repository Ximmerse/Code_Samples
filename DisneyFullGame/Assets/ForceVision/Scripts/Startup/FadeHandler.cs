using UnityEngine;

namespace Disney.ForceVision
{
	/// <summary>
	/// Monobehaviour to handle fading for galaxy map.
	/// </summary>
	public class FadeHandler : FadeReactorAbstract
	{
		private Animator menuAnimator;

		/// <summary>
		/// Setup the specified controller and animator.
		/// </summary>
		/// <param name="controller">Controller.</param>
		/// <param name="animator">Animator.</param>
		public void Setup(MainNavigationController controller, Animator animator)
		{
			menuAnimator = animator;
			Setup(controller);
		}

		/// <summary>
		/// Start fading in / out.
		/// </summary>
		/// <param name="toBlack">If set to <c>true</c> to black.</param>
		protected override void StartFading(bool toBlack)
		{
			if (!menuAnimator)
			{
				return;
			}

			if (toBlack)
			{
				menuAnimator.Play("menuFull_hide");
			}
			else
			{
				menuAnimator.Play("menuFull_show");
			}
		}
	}
}

