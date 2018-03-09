using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Disney.ForceVision
{
	[System.Serializable]
	public class ApplicationConfig
	{
		#region Properties

		public string Version;
		public List<EnvironmentConfig> Environments;

		#endregion

		#region Class Methods

		/// <summary>
		/// Gets the environment config.
		/// </summary>
		/// <returns>The environment config.</returns>
		/// <param name="environmentType">Environment type.</param>
		public EnvironmentConfig GetEnvironmentConfig(EnvironmentType environmentType)
		{
			return Environments.First(environment => environment.Type == environmentType);
		}

		#endregion
	}
}