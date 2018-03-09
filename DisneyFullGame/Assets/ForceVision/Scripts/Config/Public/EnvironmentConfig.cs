using System.Collections;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	public enum EnvironmentType
	{
		Dev = 0,
		QA,
		Prod
	}

	[System.Serializable]
	public class EnvironmentConfig
	{
		#region Properties

		public EnvironmentType Type;
		public List<ServiceConfig> Services;

		#endregion

		#region Class Methods

		public override string ToString ()
		{
			var log = "[EnvironmentConfig] { Type: " + Type + " }\n";
			Services.ForEach(service => {
				log += service.ToString() + "\n";
			});
			return log;
		}

		#endregion
	}
}