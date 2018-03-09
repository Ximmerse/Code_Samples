using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Disney.ForceVision
{
	public class OptionsElement : NavigationElement
	{
		public UnityEvent Clicked;
		public UnityEvent GazeOn;
		public UnityEvent GazeOff;

		public override void OnClicked()
		{
			Clicked.Invoke();
		}

		public override void OnGazedAt()
		{
			GazeOn.Invoke();
		}

		public override void OnGazedOff()
		{
			GazeOff.Invoke();
		}
	}
}