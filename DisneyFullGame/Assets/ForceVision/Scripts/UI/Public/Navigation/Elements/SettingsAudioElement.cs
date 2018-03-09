using UnityEngine.Events;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SettingsAudioElement : NavigationElement
	{
		public UnityEvent GazeOn;
		public UnityEvent GazeOff;

		public MainNavigationController Controller;

		public bool Enabled
		{
			get
			{
				return enabled;
			}

			set
			{
				enabled = value;
				Selected.SetActive(enabled);
			}
		}

		public GameObject Selected;

		new private bool enabled;

		public override void OnClicked()
		{
			Enabled = !Enabled;
			Controller.UpdateSpatializationSetting(Enabled);
			if (Enabled)
			{
				AudioEvent.Play(AudioEventName.Settings.Toggle3dOn, gameObject);
			}
			else
			{
				AudioEvent.Play(AudioEventName.Settings.Toggle3dOff, gameObject);
			}
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