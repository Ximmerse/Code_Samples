using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Disney.Vision;

namespace Disney.ForceVision
{
	public class BatteryPopup : MonoBehaviour
	{
		private enum TypeEnum
		{
			None,
			Headset,
			Saber,
			Phone
		}

		public CanvasGroup CommonPanel;
		public GameObject HeadsetPanel;
		public GameObject SaberPanel;
		public GameObject PhonePanel;

		public int[] HeadsetThreshold;
		public int[] SaberThreshold;
		public int[] PhoneThreshold;

		// perform a check every X seconds
		public float CheckFrequency = 10;

		public float DisplayTime = 2;
		public AnimationCurve FadeCurve;

		private NativeSettings native = new NativeSettings();

		private List<bool> HeadsetThresholdHit = new List<bool>();
		private List<bool> SaberThresholdHit = new List<bool>();
		private List<bool> PhoneThresholdHit = new List<bool>();

		private int[] previousLevel;

		private VisionSDK sdk;
		private float checkCountup = 0;
		private float displayCountup = 0;
		private TypeEnum currentType;

		public void Setup(VisionSDK sdk)
		{
			this.sdk = sdk;

			// unlike other UI panels, this one attaches itself to the stereo camera
			transform.parent = sdk.StereoCamera.transform;
			transform.SnapToZero();
			gameObject.SetActive(true);
			Show(TypeEnum.None);

			previousLevel = new int[System.Enum.GetNames(typeof(TypeEnum)).Length];
			for (int i = 0; i < previousLevel.Length; i++)
			{
				previousLevel[i] = 101;
			}
		}
	
		// Update is called once per frame
		private void Update()
		{
			if (sdk == null)
			{
				return;
			}

			if (currentType == TypeEnum.None)
			{
				checkCountup += Time.deltaTime;
				if (checkCountup < CheckFrequency)
				{
					return;
				}
				else
				{
					checkCountup = 0;
				}

				if (Application.isEditor)
				{
					if (Input.GetKeyDown(KeyCode.B))
					{
						ThresholdCheck(TypeEnum.Saber, 5);
					}
				}
				else
				{
					if (sdk.Connections.Peripherals.Count > 0 && sdk.Connections.Peripherals[0].Connected)
					{
						ThresholdCheck(TypeEnum.Saber, sdk.Connections.Peripherals[0].GetBatteryLevel());
					}
					if (sdk.Tracking.Hmd.Connected)
					{
						ThresholdCheck(TypeEnum.Headset, sdk.Tracking.Hmd.GetBatteryLevel());
					}
					ThresholdCheck(TypeEnum.Phone, (int)(native.GetBatteryRemaining() * 100));
				}
			}
			else
			{
				displayCountup += Time.deltaTime;
				if (displayCountup > DisplayTime)
				{
					Show(TypeEnum.None);
				}
				else
				{
					CommonPanel.alpha = FadeCurve.Evaluate(displayCountup / DisplayTime);
				}
			}
		}

		/// <summary>
		/// Check if the level is below an untriggered threshold for the specified type of device
		/// </summary>
		/// <param name="type">Type of device.</param>
		/// <param name="level">Battery level (0-100).</param>
		private void ThresholdCheck(TypeEnum type, int level)
		{
			if (type == TypeEnum.None || currentType != TypeEnum.None)
			{
				return;
			}

			if (previousLevel[(int)type] <= level)
			{
				return;
			}
			else
			{
				previousLevel[(int)type] = level;
			}

			int[] thresholdList = null;
			switch (type)
			{
				case TypeEnum.Headset:
					thresholdList = HeadsetThreshold;
					break;
				case TypeEnum.Saber:
					thresholdList = SaberThreshold;
					break;
				case TypeEnum.Phone:
					thresholdList = PhoneThreshold;
					break;
			}

			if (thresholdList == null || thresholdList.Length < 1)
			{
				return;
			}

			bool hit = false;
			for (int i = 0; i < thresholdList.Length; i++)
			{
				if (level <= thresholdList[i])
				{
					if (ThresholdHit(type, i))
					{
						hit = true;
					}
				}
			}
			if (hit)
			{
				Show(type);
			}
		}

		/// <summary>
		/// Check if the specified threshold has been hit for the specified type of device, and sets the threshold.
		/// </summary>
		/// <returns><c>true</c>, if threshold was hit, <c>false</c> otherwise.</returns>
		/// <param name="type">Type of device.</param>
		/// <param name="index">Index of threshold.</param>
		private bool ThresholdHit(TypeEnum type, int index)
		{
			if (type == TypeEnum.None)
			{
				return true;
			}

			List<bool> hitList = null;
			switch (type)
			{
				case TypeEnum.Headset:
					hitList = HeadsetThresholdHit;
					break;
				case TypeEnum.Saber:
					hitList = SaberThresholdHit;
					break;
				case TypeEnum.Phone:
					hitList = PhoneThresholdHit;
					break;
			}

			if (hitList == null)
			{
				return false;
			}

			while (hitList.Count <= index)
			{
				hitList.Add(false);
			}
			bool newHit = !hitList[index];
			hitList[index] = true;

			return newHit;
		}

		/// <summary>
		/// Show the prompt for the specified type of device, or hide this popup if type is "None".
		/// </summary>
		/// <param name="type">Type of device.</param>
		private void Show(TypeEnum type)
		{
			this.currentType = type;
			CommonPanel.gameObject.SetActive(type != TypeEnum.None);
			HeadsetPanel.SetActive(type == TypeEnum.Headset);
			SaberPanel.SetActive(type == TypeEnum.Saber);
			PhonePanel.SetActive(type == TypeEnum.Phone);
			displayCountup = 0;
		}
	}
}