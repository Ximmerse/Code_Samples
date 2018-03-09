using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum Difficulty
	{
		Easy = 1,
		Medium = 2,
		Hard = 3
	}

	public class CurvedUIButton : MonoBehaviour
	{
		public Difficulty Difficulty;

		public GameObject DefaultState;

		public GameObject GazeState;

		public GameObject SelectedState;

		public GameObject LockedState;

		public bool Locked 
		{
			get
			{
				return locked;
			}
			set
			{
				locked = value;

				DefaultState.SetActive(!locked);
				LockedState.SetActive(locked);
			}
		}

		public bool Selected
		{
			get
			{
				return selected;
			}

			set
			{
				selected = value;

				if (!Locked)
				{
					SelectedState.SetActive(selected);
					DefaultState.SetActive(!selected);
				}

				if (selected)
				{
					AudioEvent.Play("MAP_UI_GalaxyMap_BeginActivity", gameObject);
				}
			}
		}

		private bool locked = false;
		private bool selected = false;

		public void GazedAt()
		{
			if (Locked || selected)
			{
				return;
			}

			AudioEvent.Play("MAP_UI_Combat_FirstEncounter_Select", gameObject);

			DefaultState.SetActive(false);
			SelectedState.SetActive(false);
			GazeState.SetActive(true);
		}

		public void GazedOff()
		{
			if (Locked)
			{
				return;
			}

			DefaultState.SetActive(!selected);
			SelectedState.SetActive(selected);
			GazeState.SetActive(false);
		}

		public void SetDefault()
		{
			DefaultState.SetActive(true);
			SelectedState.SetActive(false);
			GazeState.SetActive(false);
		}
	}
}