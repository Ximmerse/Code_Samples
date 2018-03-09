using UnityEngine;

namespace Disney.ForceVision
{
	public class InputRotation : MonoBehaviour
	{
		#region Public Properties

		/// <summary>
		/// The object to rotate.
		/// </summary>
		public Transform ObjectToRotate;

		/// <summary>
		/// The camera to use.
		/// </summary>
		internal Transform CameraTransform;

		#endregion

		#region Private Properties

		private const float MagnitudeScale = 100.0f;
		private const float DistanceThreshold = 0.15f;

		#if UNITY_EDITOR
		private const float EditorMagnitudeAdjust = 10.0f;
		#endif

		private Vector3 lastKnownPosition = Vector3.zero;
		private Vector3 startPosition = Vector3.zero;
		private Vector3 targetRotation;

		#endregion

		#region Unity Methods

		private void Update()
		{
			ObjectToRotate.rotation = Quaternion.Slerp(ObjectToRotate.rotation, Quaternion.Euler(targetRotation), 0.2f);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts the rotation.
		/// </summary>
		/// <param name="position">Position.</param>
		public void StartRotation(Vector3 position)
		{
			lastKnownPosition = position;
			startPosition = position;
		}

		/// <summary>
		/// Ends the rotation.
		/// </summary>
		/// <returns><c>true</c>, if rotation was ended, <c>false</c> otherwise.</returns>
		public bool EndRotation()
		{
			float moved = Mathf.Abs((startPosition - lastKnownPosition).magnitude);

			#if UNITY_EDITOR
			moved /= 2.0f;
			#endif

			return (moved < DistanceThreshold);
		}

		/// <summary>
		/// Updates the rotation.
		/// </summary>
		/// <param name="position">Position.</param>
		public void UpdateRotation(Vector3 position)
		{
			if (!CameraTransform)
			{
				return;
			}

			Vector3 currentHeading = position - CameraTransform.position;
			Vector3 lastHeading = lastKnownPosition - CameraTransform.position;
			float angle = Vector3.Angle(position, lastKnownPosition);

			angle = (IsVectorLeftOfForward(lastHeading, currentHeading, CameraTransform.up)) ? angle : -angle;
			targetRotation = new Vector3(0, targetRotation.y + (angle * 2.0f), 0);
			lastKnownPosition = position;
		}

		/// <summary>
		/// Sets the target rotation.
		/// </summary>
		/// <param name="yRot">Y rot.</param>
		public void SetTargetRotation(float yRot)
		{
			targetRotation.y = yRot;
		}

		#endregion

		#region Private Methods

		private static bool IsVectorLeftOfForward(Vector3 forward, Vector3 target, Vector3 up)
		{
			Vector3 perpendicular = Vector3.Cross(forward, target);
			float direction = Vector3.Dot(perpendicular, up);
		
			return (direction < 0.0f);
		}

		#endregion
	}
}