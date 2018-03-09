using UnityEngine;
using Disney.Vision;

namespace Disney.ForceVision
{
	public class SaberHoldHere : MonoBehaviour
	{
		public VisionSDK Sdk;
		public float SaberDistanceFromCamera = 0.5f;

		private void Update()
		{
			Vector3 cameraPosition = Sdk.StereoCamera.transform.position;

			// Find a point a certain distance away from the camera in the
			// direction of the beacon.
			Vector3 saberHoldPosition = Vector3.MoveTowards(cameraPosition, Vector3.zero, SaberDistanceFromCamera);

			// Place it there.
			transform.position = saberHoldPosition;

			// Look towards the beacon (0, 0, 0) in the XZ direction.
			transform.LookAt(new Vector3(0.0f, transform.position.y, 0.0f));
		}
	}
}