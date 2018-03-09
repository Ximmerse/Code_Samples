using System.Collections;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	public interface IApplicationConfig
	{
		/// <summary>
		/// Gets the platform config.
		/// </summary>
		/// <returns>The platform config.</returns>
		/// <param name="serviceType">Service type.</param>
		PlatformConfig GetPlatformConfig(ServiceType serviceType);
	}
}