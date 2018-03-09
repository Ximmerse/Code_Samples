using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Disney.Vision;
using SG.Lonestar;

namespace Disney.ForceVision
{
	public class SettingsElement : NavigationElement
	{
		public UnityEvent Clicked;
		public UnityEvent GazeOn;
		public UnityEvent GazeOff;

		public int ColorId;

		public GameObject Selected;

		public GameObject Locked;

		public Image Image;

		public void Init()
		{
			if (Locked == null)
			{
				return;
			}

			switch ((ColorID)ColorId)
			{
				case ColorID.GREEN:
					Locked.SetActive(ContainerAPI.GetJediRank() < JediRank.Padawan);
				break;

				case ColorID.PURPLE:
					Locked.SetActive(!ContainerAPI.IsMedalUnlocked(MedalType.Mastery));
				break;

				case ColorID.BLUE:
					Locked.SetActive(false);
				break;
			}
		}

		public override void OnClicked()
		{
			if (Locked == null || !Locked.activeSelf)
			{
				if (Selected == null || !Selected.activeSelf)
				{
					Clicked.Invoke();
				}
			}
		}

		public override void OnGazedAt()
		{
			if (Locked != null && Locked.activeSelf)
			{
				return;
			}

			GazeOn.Invoke();
		}

		public override void OnGazedOff()
		{
			GazeOff.Invoke();
		}

		public void ColorSelected(int colorId)
		{
			Init();

			if (Locked != null && Locked.activeSelf)
			{
				return;
			}

			Color color = Image.color;
			color.a = (ColorId == colorId) ? 1.0f : 0.25f;
			Image.color = color;
			Selected.SetActive(ColorId == colorId);
		}
	}
}