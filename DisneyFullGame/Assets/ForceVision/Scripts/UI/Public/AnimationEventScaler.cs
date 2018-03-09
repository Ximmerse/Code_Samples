using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class AnimationEventScaler : MonoBehaviour 
	{
		/// <summary>
		/// The object for the animation to trigger scale lerp
		/// </summary>
		public Transform ObjectToScale;

		/// <summary>
		/// The duration of the lerp between start and target scale
		/// </summary>
		private const float LerpTime = 1f;

		public void ScaleObject(float scale)
		{
			StartCoroutine(BeginScaleObject(scale, ObjectToScale));
		}

		private IEnumerator BeginScaleObject(float scale, Transform obj)
		{
			float progress = 0;
			Vector3 targetScale = new Vector3(scale, scale, scale);
			Vector3 inititalScale = obj.localScale;

			while (progress <= 1) 
			{
				obj.localScale = Vector3.Lerp(inititalScale, targetScale, progress);
				progress += Time.deltaTime * LerpTime;
				yield return null;
			}

			obj.localScale = targetScale;
		}
	}
}
