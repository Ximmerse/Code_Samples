using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class DebugMenuElement : NavigationElement
	{
		public UnityEvent Clicked;

		public override void OnClicked()
		{
			if (Clicked != null)
			{
				Clicked.Invoke();
			}
		}

		public override void OnGazedAt()
		{
			GetComponent<Image>().color = Color.green;
		}

		public override void OnGazedOff()
		{
			GetComponent<Image>().color = Color.white;
		}
	}
}