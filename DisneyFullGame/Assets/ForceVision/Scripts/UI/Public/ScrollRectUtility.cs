using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(ScrollRect))]

	/// <summary>
	/// A class of helpful functions for game objects with a ScrollRect
	/// </summary>
	public class ScrollRectUtility : MonoBehaviour
	{
		public bool ResetScrollOnDisable;

		private ScrollRect scrollRect;

		void Start()
		{
			scrollRect = GetComponent<ScrollRect>();
		}

		void OnDisable()
		{
			if (ResetScrollOnDisable)
			{
				if (scrollRect.vertical)
				{
					scrollRect.verticalNormalizedPosition = 1;
				}

				if (scrollRect.horizontal)
				{
					scrollRect.horizontalNormalizedPosition = 0;
				}
			}
		}
	}
}
