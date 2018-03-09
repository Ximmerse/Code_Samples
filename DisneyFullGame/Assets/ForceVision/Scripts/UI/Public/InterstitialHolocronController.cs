using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class InterstitialHolocronController : MonoBehaviour 
	{

		// Use this for initialization
		void Start () 
		{
			if (MenuController.HolocronObjectToDestroy)
			{
				GameObject.Destroy(MenuController.HolocronObjectToDestroy);
			}

			DontDestroyOnLoad(gameObject);

			MenuController.HolocronObjectToDestroy = gameObject;

			MenuController.ShouldHolocronAnimate = false;
		}
	}
}
