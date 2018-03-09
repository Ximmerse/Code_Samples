using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Text))]

	public class VersionNumber : MonoBehaviour
	{
		void Start()
		{
			GetComponent<Text>().text = "v" + Application.version;
		}
	}
}
