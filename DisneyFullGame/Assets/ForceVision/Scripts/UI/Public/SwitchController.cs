using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum SwitchState
	{
		Off = 0,
		On,
		Transition
	}

	public class SwitchController : MonoBehaviour
	{
		#region Properties

		public GameObject On;
		public GameObject Off;
		public GameObject Transition;
		public SwitchState State { get; private set; }

		#endregion

		#region Class Methods

		/// <summary>
		/// Sets the state of the SwitchController instance.
		/// </summary>
		/// <param name="state">State.</param>
		public void SetState(SwitchState state)
		{
			// saving state
			State = state;

			// updating view based on state
			if (On != null)
			{
				On.SetActive(state == SwitchState.On);
			}

			if (Off != null)
			{
				Off.SetActive(state == SwitchState.Off);
			}

			if (Transition != null)
			{
				Transition.SetActive(state == SwitchState.Transition);
			}
		}


		public void Toggle()
		{
			SwitchState newState = State == SwitchState.Off ? SwitchState.On : SwitchState.Off;

			SetState(newState);
		}

		#endregion
	}
}