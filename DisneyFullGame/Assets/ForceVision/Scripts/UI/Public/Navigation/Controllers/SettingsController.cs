using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	public class SettingsController : BaseController
	{
		public SwitchController SoundIcon;
		public Slider Volume;
		public SettingsElement[] Colors;
		public SettingsAudioElement SpatializationElement;
		public GameObject[] PhoneBatteryLevels;
		public GameObject[] HmdBatteryLevels;
		public GameObject[] SaberBatteryLevels;

		private ContainerAPI container;

		public override void Init()
		{
			container = new ContainerAPI(Game);
			Volume.value = container.NativeSettings.GetVolume();
			SaberColorSelected((int)container.GetSavedSaberColorID(), true);
			UpdateBatteryLevels();
			SpatializationElement.Enabled = container.UseSpatialization();

			base.Init();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			AudioEvent.Play(AudioEventName.Settings.PopupOpen, gameObject);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			AudioEvent.Play(AudioEventName.Settings.PopUpClose, gameObject);
		}

		protected override void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			if (!ButtonIsActivateOrControl(eventArguments.Button))
			{
				return;
			}

			if (CurrentElement != null && CurrentElement.Interactable)
			{
				CurrentElement.OnClicked();
			}

			base.OnButtonUp(sender, eventArguments);
		}

		protected override void OnGazedAt(object sender, GazeEventArgs eventArguments)
		{
			base.OnGazedAt(sender, eventArguments);

			if (CurrentElement != null)
			{
				CurrentElement.OnGazedAt();
			}
		}

		protected override void OnGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (CurrentElement != null)
			{
				CurrentElement.OnGazedOff();
			}

			base.OnGazedOff(sender, eventArguments);
		}

		public void SaberColorSelected(int colorId)
		{
			SaberColorSelected(colorId, false);
		}

		public void SaberColorSelected(int colorId, bool fromInit)
		{
			if (!fromInit)
			{
				(Sdk.Connections.GetPeripheral(VisionSDK.ControllerName) as ControllerPeripheral).SetColor(colorId);
				container.SetSavedSaberColorID(colorId);

				if (Controller.OnSaberColorChanged != null)
				{
					Controller.OnSaberColorChanged.Invoke(this, new EventArgs());
				}
			}

			for (int i = 0; i < Colors.Length; i++)
			{
				Colors[i].ColorSelected(colorId);
			}
		}

		public void VolumeUp()
		{
			float newValue = Mathf.Clamp(Mathf.Floor((Volume.value + 1 * 0.1f) * 10) / 10, 0f, 1f);
			Volume.value = newValue;
			container.NativeSettings.SetVolume(Volume.value);

			SoundIcon.SetState(container.NativeSettings.IsMuted() ? SwitchState.Off : SwitchState.On);

			AudioEvent.Play(AudioEventName.Ftue.Stereo.PlusButton, gameObject);
		}

		public void VolumeDown()
		{
			float newValue = Mathf.Clamp(Mathf.Floor((Volume.value + -1 * 0.1f) * 10) / 10, 0f, 1f);
			Volume.value = newValue;
			container.NativeSettings.SetVolume(Volume.value);

			SoundIcon.SetState(container.NativeSettings.IsMuted() ? SwitchState.Off : SwitchState.On);

			AudioEvent.Play(AudioEventName.Ftue.Stereo.MinusButton, gameObject);
		}

		private void UpdateBatteryLevels()
		{
			// If off, treat as 0
			int saberLevel = 0;
			int hmdLevel = 0;

			if (Sdk != null && Sdk.Tracking != null)
			{
				if (Sdk.Connections.Peripherals.Count > 0 && Sdk.Connections.Peripherals[0].Connected)
				{
					saberLevel = Sdk.Connections.Peripherals[0].GetBatteryLevel();
				}

				if (Sdk.Tracking.Hmd.Connected)
				{
					hmdLevel = Sdk.Tracking.Hmd.GetBatteryLevel();
				}
			}

			int phoneLevel = (int)(container.NativeSettings.GetBatteryRemaining() * 100);

			// Levels
			BatteryView(PhoneBatteryLevels, phoneLevel);
			BatteryView(HmdBatteryLevels, hmdLevel);
			BatteryView(SaberBatteryLevels, saberLevel);
		}

		private void BatteryView(GameObject[] levels, int value)
		{
			if (levels.Length > 3)
			{
				levels[0].SetActive(value > 75);
				levels[1].SetActive(value > 50);
				levels[2].SetActive(value > 25);
				levels[3].SetActive(value > 5);
			}
		}
	}
}