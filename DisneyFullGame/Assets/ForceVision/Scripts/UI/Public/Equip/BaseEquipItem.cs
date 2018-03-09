using UnityEngine;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Collider))]
	public class BaseEquipItem : MonoBehaviour
	{
		public virtual void GazedAt()
		{
		}

		public virtual void GazedOff()
		{
		}

		public virtual void Clicked()
		{
		}
	}
}