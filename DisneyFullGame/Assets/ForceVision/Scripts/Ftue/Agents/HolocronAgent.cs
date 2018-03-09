using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class HolocronAgent : FtueAgent 
	{
		#region Event Handlers

		public void OnAnimationComplete(HolocronAnimations animationType)
		{
			AnimationComplete((int)animationType);
		}

		#endregion
	}
}