using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class ObjectTracker : MonoBehaviour
	{
		#region Events

		public static Action OnObjectReachedTargetLocation;
		public static Action OnObjectLeftTargetLocation;

		#endregion

		#region Properties

		public Transform ObjectToTrack;
		public Transform TargetLocation;
		[Range(0,5)]
		public float DistanceTolerance;

		public bool IsTargetInRange
		{
			get
			{
				return isAtTargetLocation;
			}
		}

		private bool shouldTrack = false;
		private bool isAtTargetLocation = false;

		#endregion

		#region MonoBehaviour

		protected void Update ()
		{
			if (shouldTrack && ObjectToTrack != null && TargetLocation != null)
			{
				float distance = Vector3.Distance(ObjectToTrack.position, TargetLocation.position);

				if (distance <= DistanceTolerance)
				{
					if (!isAtTargetLocation && OnObjectReachedTargetLocation != null)
					{
						OnObjectReachedTargetLocation.Invoke();
					}

					isAtTargetLocation = true;
				}
				else
				{
					if (isAtTargetLocation && OnObjectLeftTargetLocation != null)
					{
						OnObjectLeftTargetLocation.Invoke();
					}

					isAtTargetLocation = false;
				}
			}
		}

		#endregion

		#region Class Methods

		public void StartTracking()
		{
			shouldTrack = true;
		}

		public void StopTracking()
		{
			shouldTrack = false;
		}

		#endregion
	}
}