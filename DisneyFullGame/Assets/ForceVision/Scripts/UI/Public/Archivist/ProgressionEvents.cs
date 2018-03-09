using UnityEngine;

namespace Disney.ForceVision
{
	public class ProgressionEvents : MonoBehaviour
	{

		/// <summary>
		/// Plays progression sound
		/// </summary>
		/// <param name="token">Token.</param>
		public void PlaySound(string token)
		{
			Debug.Log ("token: " + token);
			AudioEvent.Play(token, gameObject);
		}
	}
}