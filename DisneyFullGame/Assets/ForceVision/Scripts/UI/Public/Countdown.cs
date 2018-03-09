using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	public class Countdown : MonoBehaviour
	{
		#region Events

		public static EventHandler<CountdownEventArgs> OnCountdownEvent;

		#endregion

		#region Properties

		public float CountdownAmount;
		[Range(0,1f)]
		public float CountdownInterval;

		private float timeRemaining;
		private float timeElapsed;
		private bool shouldCountdown;

		#endregion

		#region MonoBehaviour

		protected void OnEnable()
		{
			timeRemaining = CountdownAmount;
			timeElapsed = 0;

			shouldCountdown = true;

			if (OnCountdownEvent != null)
			{
				OnCountdownEvent(this, new CountdownEventArgs((int)timeRemaining, CountdownEventType.Start));
			}
		}

		protected void OnDisable()
		{
			shouldCountdown = false;
		}

		protected void Update()
		{
			if (shouldCountdown && timeRemaining > 0)
			{
				timeElapsed += Time.deltaTime;
				if (timeElapsed >= CountdownInterval)
				{
					timeRemaining -= CountdownInterval;
					timeElapsed = 0;

					if (OnCountdownEvent != null)
					{
						if (timeRemaining > 0)
						{
							OnCountdownEvent(this, new CountdownEventArgs((int)timeRemaining, CountdownEventType.Interval));
						}
						else if (timeRemaining <= 0)
						{
							OnCountdownEvent(this, new CountdownEventArgs(0, CountdownEventType.Finish));
						}
					}
				}
			}
		}

		#endregion
	}
}