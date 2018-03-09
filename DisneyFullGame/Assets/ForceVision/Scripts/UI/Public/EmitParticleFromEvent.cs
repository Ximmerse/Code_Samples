using UnityEngine;

namespace Disney.ForceVision
{
	public class EmitParticleFromEvent : MonoBehaviour
	{
		#region Constants

		public const string PI = "CwM3";

		#endregion

		#region Public Methods

		/// <summary>
		/// The particle system to trigger.
		/// </summary>
		public ParticleSystem Particle;

		/// <summary>
		/// The number to emit.
		/// </summary>
		public int NumberToEmit = 1;

		#endregion

		#region Public Methods

		/// <summary>
		/// Emit the particles.
		/// </summary>
		public void Fire()
		{
			Particle.Emit(NumberToEmit);
		}

		/// <summary>
		/// Emit the particles for small rebel soldiers
		/// </summary>
		public void FireEvent()
		{

		}

		#endregion
	}
}