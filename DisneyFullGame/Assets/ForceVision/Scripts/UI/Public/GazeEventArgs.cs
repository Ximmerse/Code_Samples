using System;
using UnityEngine;

namespace Disney.ForceVision
{
	public class GazeEventArgs : EventArgs
	{
		public Transform Hit;

		public GazeEventArgs(Transform hit)
		{
			Hit = hit;
		}
	}
}