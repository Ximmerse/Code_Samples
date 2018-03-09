using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class AnimationEventArgs : EventArgs
	{
		#region Properties

		public string AnimationName { get; private set; }

		#endregion

		#region Constructor

		public AnimationEventArgs(string animationName)
		{
			AnimationName = animationName;
		}

		#endregion
	}
}
