using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class StereoToggleGroup : ToggleGroup
	{
		#region Events

		public static EventHandler<ToggleEventArgs> OnToggleChange { get; set; }

		public static EventHandler<ToggleEventArgs> OnToggleOff { get; set; }

		#endregion

		#region Properties

		protected Toggle[] Toggles
		{
			get
			{
				if (toggles == null)
				{
					GetToggles();
				}

				return toggles;
			}
		}

		private Toggle[] toggles;

		#endregion

		#region MonoBehaviour

		protected override void Awake()
		{
		}

		#endregion

		#region Class Methods

		private void GetToggles()
		{
			toggles = transform.GetComponentsInChildren<Toggle>();
			foreach(Toggle toggle in toggles)
			{
				toggle.onValueChanged.AddListener((isOn) => {
					if (!isOn)
					{
						//fired when you click to turn of off toggle
						if (OnToggleOff != null)
						{
							OnToggleOff(this, new ToggleEventArgs(toggle));
						}

						return;
					}

					UpdateGroup(toggle);

					if (OnToggleChange != null)
					{
						OnToggleChange(this, new ToggleEventArgs(toggle));
						AudioEvent.Play(AudioEventName.Ftue.Stereo.Swipe, gameObject);
					}
				});
			}
		}

		public void ResetToggles()
		{
			foreach(Toggle toggle in toggles)
			{
				toggle.interactable = false;
			}
		}

		public void SetToggleByIndex(int index)
		{
			if(index >= 0 && index < Toggles.Length)
			{
				UpdateGroup(Toggles[index]);
			}
		}

		public void SetToggleInteractable(int index, bool state)
		{
			Toggles[index].interactable = state;
		}

		private void UpdateGroup(Toggle selectedToggle)
		{
			foreach(Toggle toggle in Toggles)
			{
				toggle.isOn = (toggle == selectedToggle);
			}
		}

		#endregion
	}
}