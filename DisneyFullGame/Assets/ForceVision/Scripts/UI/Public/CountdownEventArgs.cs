using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Disney.ForceVision
{
	public enum CountdownEventType
	{
		Start = 0,
		Interval,
		Finish
	}

	public class CountdownEventArgs : EventArgs
	{
		public int TimeRemaining { get; private set; }
		public CountdownEventType EventType { get; private set; }

		public CountdownEventArgs(int timeRemaining, CountdownEventType eventType)
		{
			TimeRemaining = timeRemaining;
			EventType = eventType;
		}
	}
}