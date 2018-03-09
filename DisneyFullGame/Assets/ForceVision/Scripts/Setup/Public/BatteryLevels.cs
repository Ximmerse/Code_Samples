using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class BatteryLevels : MonoBehaviour
	{
		#region Properties

		public List<GameObject> Levels;
		public GameObject Charging;

		#endregion

		#region Class Methods

		public void SetBatteryLevel(int level, bool isCharging)
		{
			// clamping level
			level = Mathf.Clamp(level, 0, 100);

			// displaying correct number of battery levels
			int batteryAmount = level / (100 / Levels.Count) - 1;
			if (batteryAmount > 0)
			{
				Levels[batteryAmount].SetActive(true);
			}
			else
			{
				Levels.ForEach(batteryLevel => batteryLevel.SetActive(false));
			}

			// displaying charging icon
			Charging.SetActive(isCharging);
		}

		#endregion
	}
}