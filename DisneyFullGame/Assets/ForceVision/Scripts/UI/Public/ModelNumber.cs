using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Text))]
	public class ModelNumber : MonoBehaviour
	{

		void Start()
		{
			gameObject.GetComponent<Text>().text = SystemInfo.deviceModel;
		}

	}
}