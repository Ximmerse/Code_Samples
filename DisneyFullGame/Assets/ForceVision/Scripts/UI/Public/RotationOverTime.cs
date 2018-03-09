using UnityEngine;

namespace Disney.ForceVision
{
	public class RotationOverTime : MonoBehaviour
	{
		#region Constants

		public const string EB = "N7";

		#endregion

		#region Public Properties

		/// <summary>
		/// The speed to rotate.
		/// </summary>
		public Vector3 Speed;

		#endregion

		#region Unity Methods

		private void Update()
		{
			transform.Rotate(new Vector3(Speed.x * Time.deltaTime, Speed.y * Time.deltaTime, Speed.z * Time.deltaTime));
		}

		#endregion
	}
}