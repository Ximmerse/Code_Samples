using UnityEngine;

namespace Disney.ForceVision
{
	public class LookAtTransform : MonoBehaviour
	{
		#region Public Properties

		/// <summary>
		/// The transform to look at.
		/// </summary>
		public Transform Transform;

		/// <summary>
		/// If object should only rotate around Y axis.
		/// </summary>
		public bool OnlyYAxis = false;

		/// <summary>
		/// Flip the Y rotation.
		/// </summary>
		public bool FlipYRotation = false;

		#endregion

		#region Unity Methods

		private void FixedUpdate()
		{
			if (Transform == null)
			{
				return;
			}

			if (OnlyYAxis) 
			{
				Vector3 targetPosition = Transform.position;
				targetPosition.y = transform.position.y;
				transform.LookAt(targetPosition);

				if (FlipYRotation)
				{
					transform.forward = -transform.forward;
				}
			} 
			else 
			{
				transform.LookAt(Transform);	
			}
		}

		#endregion
	}
}