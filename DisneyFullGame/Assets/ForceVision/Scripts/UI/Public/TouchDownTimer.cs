using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class TouchDownTimer : MonoBehaviour
	{
		#region Events

		public static Action OnTouchDownTimeReached;

		#endregion

		#region Properties

		public float TouchDownTime = 3f;

		private float currentTime = 0f;
		private bool isTouchDown = false;

		#endregion

		#region MonoBehaviour

		protected void Update ()
		{
			if (Input.GetMouseButtonDown(0))
			{
				StartTimer();
			}

			if (Input.GetMouseButtonUp(0))
			{
				EndTimer();
			}

			UpdateTimer();
		}

		#endregion

		#region Class Methods

		protected void EndTimer()
		{
			isTouchDown = false;
			currentTime = 0f;
		}

		protected void StartTimer()
		{
			isTouchDown = true;
		}

		protected void UpdateTimer()
		{
			if (isTouchDown)
			{
				currentTime += Time.deltaTime;

				if (currentTime >= TouchDownTime)
				{
					if (OnTouchDownTimeReached != null)
					{
						OnTouchDownTimeReached.Invoke();
					}

					EndTimer();
				}
			}
		}

		#endregion
	}
}