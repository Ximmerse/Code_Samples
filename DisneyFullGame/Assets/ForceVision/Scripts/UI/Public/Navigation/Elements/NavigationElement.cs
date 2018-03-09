using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Collider))]
	public class NavigationElement : MonoBehaviour
	{
		[HideInInspector]
		public bool Interactable = true;

		public virtual void OnClicked()
		{
		}

		public virtual void OnGazedAt()
		{
		}

		public virtual void OnGazedOff()
		{
		}
	}
}