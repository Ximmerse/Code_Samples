using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SithHolocronAnimationEvents : MonoBehaviour 
	{
		public Animator HolocronAnimator;

		public void PlayHolocronAnimation(string animName)
		{
			HolocronAnimator.Play(animName);
		}
	}
}
