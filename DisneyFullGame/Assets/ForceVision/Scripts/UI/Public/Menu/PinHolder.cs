using UnityEngine;
using SG.ProtoFramework;
using System;

namespace Disney.ForceVision
{
	// This can't go above 3 because it is also used as an array index.
	public enum PinType
	{
		Commander = 0,
		Guardian = 1,
		Consular = 2
	}

	public class PinHolder : MonoBehaviour
	{
		[EnumMappedList(typeof(PinType))]
		public GameObject[] Pins = new GameObject[Enum.GetNames(typeof(PinType)).Length];

		public void ShowPin(PinType type)
		{
			Pins[(int)type].SetActive(true);
		}
	}
}