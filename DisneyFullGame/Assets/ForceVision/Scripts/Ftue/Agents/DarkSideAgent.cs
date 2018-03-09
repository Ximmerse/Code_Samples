using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum DarkSideAnimationType
	{
		DisturbsHolocron,
		LookAtYourself,
		StrikeNow,
		StrikeAgain,
		Impressive,
		TeachYou,
		Complete
	}

	public class DarkSideAgent : FtueAgent 
	{
		#region Event Handlers

		/// <summary>
		/// Raises the animation complete event.
		/// </summary>
		/// <param name="animationType">Animation type.</param>
		public void OnAnimationComplete(DarkSideAnimationType animationType)
		{
			AnimationComplete((int)animationType);
		}

		#endregion
	}
}