using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum ServiceType
	{
		Swrve = 0,
		Apteligent,
		Lenovo
	}

	[System.Serializable]
	public class ServiceConfig
	{
		#region Properties

		public string Name;
		public ServiceType Type;
		public List<PlatformConfig> Platforms;

		#endregion

		#region Class Methods

		public override string ToString ()
		{
			var log = "[ServiceConfig] { Name: " + Name + ", Type: " + Type + " }\n";
			Platforms.ForEach(platform => {
				log += platform.ToString() + "\n";
			});
			return log;
		}

		#endregion
	}
}