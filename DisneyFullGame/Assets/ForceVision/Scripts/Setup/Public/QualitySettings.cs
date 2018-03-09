using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using Disney.Vision;

namespace Disney.ForceVision
{
	public enum Quality
	{
		Low = 0,
		Medium = 1,
		High = 2
	}

	public class QualitySettings : MonoBehaviour
	{
		public Quality CurrentQuality = Quality.High;

		public GameObject[] Bars;
		public Toggle[] Labels;

		private PersistentDataStorage storage;
		private QualityController qualityController;

		private void Start()
		{
			VisionSDK sdk = GameObject.FindObjectOfType<VisionSDK>();

			qualityController = new QualityController(sdk);

			CurrentQuality = qualityController.GetQuality();
			UpdateQualityBar((int)CurrentQuality);
		}

		public void OnClose()
		{
			qualityController.ApplyQuality();
		}

		public void OnPlusButtonDown()
		{
			UpdateQuality(true);
		}

		public void OnMinusButtonDown()
		{
			UpdateQuality(false);
		}

		private void UpdateQuality(bool Increase)
		{
			int currentValue = (int)CurrentQuality;
			int last = (int)Enum.GetValues(typeof(Quality)).Cast<Quality>().Last();
			int first = (int)Enum.GetValues(typeof(Quality)).Cast<Quality>().First();
			if (Increase == true)
			{
				if (currentValue < last)
				{
					currentValue++;

					// playing sound
					AudioEvent.Play(AudioEventName.Ftue.Stereo.PlusButton, gameObject);
				}
			}
			else
			{
				if (currentValue > first)
				{
					currentValue--;

					// playing sound
					AudioEvent.Play(AudioEventName.Ftue.Stereo.MinusButton, gameObject);
				}
			}
			CurrentQuality = (Quality)currentValue;

			UpdateQualityBar(currentValue);

			qualityController.SaveQuality(currentValue);
		}

		private void UpdateQualityBar(int currentValue)
		{
			for (int i = 0; i < Bars.Length; i++)
			{
				Bars[i].SetActive(i <= currentValue);
				Labels[i].isOn = (i == currentValue);
			}
		}

	}
}