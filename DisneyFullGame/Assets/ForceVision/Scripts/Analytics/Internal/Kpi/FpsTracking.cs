using UnityEngine;
using System.Linq;

namespace Disney.ForceVision.Internal
{
	public class FpsTracking : MonoBehaviour
	{
		[Header("Number of fps measurements to average.")]
		public static int SampleCount = 90;

		private float[] fps = new float[SampleCount];
		private int fpsIndex = 0;

		void Update()
		{
			fps[fpsIndex] = 1.0f / Time.unscaledDeltaTime;
			fpsIndex++;
			if (fpsIndex > fps.Length - 1)
			{
				fpsIndex = 0;
			}
		}

		public float GetFps()
		{
			return fps.Sum() / fps.Length;
		}

	}
}