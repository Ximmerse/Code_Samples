using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public enum DifficultyComplete
	{
		NotComplete = 0,
		Easy = 1,
		Medium = 2,
		Hard = 3
	}

	public class PlanetNode : MonoBehaviour
	{
		public GameObject FirstCircuit;
		public GameObject SecondCircuit;
		public GameObject ThirdCircuit;
		public Image Portrait;
		public GameObject Tooltip;

		public void SetProgress(DifficultyComplete difficulty)
		{
			FirstCircuit.SetActive(false);
			SecondCircuit.SetActive(false);
			ThirdCircuit.SetActive(false);

			if (difficulty != DifficultyComplete.NotComplete && Portrait != null)
			{
				Portrait.color = Color.white;
			}

			switch (difficulty)
			{
				case DifficultyComplete.NotComplete:
					FirstCircuit.SetActive(true);
				break;

				case DifficultyComplete.Easy:
					SecondCircuit.SetActive(true);
				break;

				case DifficultyComplete.Medium:
				case DifficultyComplete.Hard:
					ThirdCircuit.SetActive(true);
				break;
			}
		}
	}
}