using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class ToggleEventArgs : EventArgs
	{
		#region Properties

		public Toggle SelectedToggle { get; private set; }

		#endregion

		#region Constructor

		public ToggleEventArgs(Toggle selectedToggle)
		{
			SelectedToggle = selectedToggle;
		}

		#endregion
	}
}