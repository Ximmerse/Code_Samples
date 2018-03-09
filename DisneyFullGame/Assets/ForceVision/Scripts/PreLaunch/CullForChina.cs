using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class CullForChina : MonoBehaviour
	{

		void Awake()
		{
#if SKU_CHINA
			gameObject.SetActive(false);
			DestroyImmediate(gameObject);
#endif
		}
	}
}
