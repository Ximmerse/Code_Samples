using UnityEngine;

namespace Disney.ForceVision
{
	public class HolochessEvents : MonoBehaviour
	{
		public Animator RigController;
		public Disney.HoloChess.HolochessOpponentBehavior Opponent;

		/// <summary>
		/// Plays archivist dialog
		/// </summary>
		/// <param name="token">Token.</param>
		public void PlaySound(string token)
		{
			Opponent.PlayDialog(token);
		}

		public void PlayRigAnimation(string animation)
		{
			RigController.Play(animation);
		}

		public void TriggerRigAnimation(string trigger)
		{
			RigController.SetTrigger(trigger);
		}

		public void OnComplete()
		{
			// Can send something on complete if we want, but
			// for now, just here to signify when to end the
			// container animation.
		}
	}
}