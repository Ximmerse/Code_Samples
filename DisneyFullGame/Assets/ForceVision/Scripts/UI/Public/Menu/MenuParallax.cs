using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class MenuParallax : MonoBehaviour 
	{

		public float ParallaxAmount = 1.0f;
		public float ParallaxTester = 0f;
		public List<Transform> PositiveTransforms = new List<Transform>();
		public List<Transform> NegativeTransforms = new List<Transform>();

		void Start() 
		{
			Input.gyro.enabled = true;
		}

		void Update() 
		{
			// Transform all objects to move positively
			for (int i = 0; i < PositiveTransforms.Count; i++)
			{
				// Move between 0 and ParallaxAmount, evenly divided
				float distanceFrac = -ParallaxAmount / PositiveTransforms.Count * (PositiveTransforms.Count / (i + 1));
				float gyro = GetNegativeRotation(Input.gyro.attitude.eulerAngles.x + ParallaxTester);
				Transform imageTransform = PositiveTransforms[i];
				imageTransform.localPosition = new Vector3(distanceFrac * gyro, imageTransform.localPosition.y, imageTransform.localPosition.z);
			}

			// Transform all objects to move negatively
			for (int i = 0; i < NegativeTransforms.Count; i++)
			{
				// Move between 0 and -ParallaxAmount, evenly divided
				float distanceFrac = ParallaxAmount / NegativeTransforms.Count * (NegativeTransforms.Count / (i + 1));
				float gyro = GetNegativeRotation(Input.gyro.attitude.eulerAngles.x + ParallaxTester);
				Transform imageTransform = NegativeTransforms[i];
				imageTransform.localPosition = new Vector3(distanceFrac * gyro, imageTransform.localPosition.y, imageTransform.localPosition.z);
			}
		}

		private float GetNegativeRotation(float input)
		{
			float output = input;
			if (input > 180f)
			{
				output = input - 360;
			}

			return output;
		}
	}
}
