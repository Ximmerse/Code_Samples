using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	[System.Serializable]
	public class FtueAnimationStep
	{
		#region Properties

		public FtueAgentType Agent;
		public string Clip;
		public int Delay;
		public string SoundEvent;
		public bool AutoPlay;

		#endregion
	}
}