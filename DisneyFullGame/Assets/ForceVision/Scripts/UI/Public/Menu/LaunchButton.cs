using UnityEngine;

namespace Disney.ForceVision
{
	// This class is used to identify launch buttons
	public class LaunchButton : MonoBehaviour
	{
		public GameObject Highlight;

		public void OnGaze()
		{
			Highlight.SetActive(true);
		}

		public void OnGazeOff()
		{
			Highlight.SetActive(false);
		}
	}
}