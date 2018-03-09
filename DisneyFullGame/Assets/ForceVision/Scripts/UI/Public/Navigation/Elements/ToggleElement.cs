using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Toggle))]
	public class ToggleElement : NavigationElement
	{
		[Header("The GameObject to Toggle Active / InActive")]
		public GameObject Target;

		public Toggle Toggle;

		public GameObject Normal;
		public GameObject Gaze;

		public UnityEvent GazeOn;
		public UnityEvent GazeOff;

		public override void OnClicked()
		{
			Target.SetActive(!Target.activeSelf);
			Toggle.isOn = Target.activeSelf;
			Normal.SetActive(!Target.activeSelf);
			Gaze.SetActive(Target.activeSelf);
		}

		public override void OnGazedAt()
		{
			if (Toggle.isOn)
			{
				return;
			}

			Normal.SetActive(false);
			Gaze.SetActive(true);
			GazeOn.Invoke();
		}

		public override void OnGazedOff()
		{
			if (Toggle.isOn)
			{
				return;
			}

			Normal.SetActive(true);
			Gaze.SetActive(false);
			GazeOff.Invoke();
		}
	}
}