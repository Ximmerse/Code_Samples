using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	// Animation event to trigger display of progression lines
	public class LineDisplayListener : MonoBehaviour 
	{
		/// <summary>
		/// The line controller to call ShowLines function
		/// </summary>
		public PlanetLineController LineController;

		public void ShowProgressionLines()
		{
			LineController.ShowLines();
		}

	}
}
