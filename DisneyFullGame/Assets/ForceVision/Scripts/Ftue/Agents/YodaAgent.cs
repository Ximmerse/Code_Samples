using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class YodaAgent : FtueAgent 
	{
		#region Event Handlers

		public void OnAnimationComplete(YodaAnimations animationType)
		{
			AnimationComplete((int)animationType);
		}

		#endregion
	}
}