using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class DroidAgent : FtueAgent 
	{
		#region Event Handlers

		public void OnAnimationComplete(DroidAnimations animationType)
		{
			AnimationComplete((int)animationType);
		}

		#endregion
	}
}