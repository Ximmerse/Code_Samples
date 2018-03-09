using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class ArchivistAgent : FtueAgent 
	{
		public GameObject saberBladeLeft;
		public GameObject saberBladeLeftGlow;
		public GameObject saberBladeRight;
		public GameObject saberBladeRightGlow;

		#region Event Handlers

		public void OnAnimationComplete(ArchivistAnimations animationType)
		{
			AnimationComplete((int)animationType);
		}

		public void OnShowSaber(string leftOrRight)
		{
			if (leftOrRight == "left")
			{
				saberBladeLeft.SetActive(true);
				saberBladeLeftGlow.SetActive(true);
			}

			if (leftOrRight == "right")
			{
				saberBladeRight.SetActive(true);
				saberBladeRightGlow.SetActive(true);
			}
		}

		public void OnHideSaber(string leftOrRight)
		{
			if (leftOrRight == "left")
			{
				saberBladeLeft.SetActive(false);
				saberBladeLeftGlow.SetActive(false);
			}

			if (leftOrRight == "right")
			{
				saberBladeRight.SetActive(false);
				saberBladeRightGlow.SetActive(false);
			}
		}

		#endregion
	}
}