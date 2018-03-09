using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SaberSwapper : MonoBehaviour 
	{
		public GameObject StaticSaber;
		public GameObject HandSaber;

		public void SaberSwap(int state)
		{
			if (state == 0)
			{
				StaticSaber.SetActive(false);
				HandSaber.SetActive(true);
			}

			if (state == 1)
			{
				StaticSaber.SetActive(true);
				HandSaber.SetActive(false);
			}
		}
	}
}
