using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class FtueAgentAnimationEvent : EventArgs
	{
		#region Events

		public static EventHandler<FtueAgentAnimationEvent> OnFtueAgentAnimationComplete;

		#endregion

		#region Properties

		public FtueAgentType Agent { get; private set; }
		public int Type { get; private set; }
		public string Clip { get; private set; }

		#endregion

		#region Constructor

		public FtueAgentAnimationEvent(FtueAgentType agent, int type, string clip)
		{
			Agent = agent;
			Type = type;
			Clip = clip;
		}

		#endregion
	}
}