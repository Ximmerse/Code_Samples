using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class Config : IApplicationConfig
	{
		#region Constants

		public const string DefaultConfigFile = "n7c4c656";

		#endregion

		#region Properties

		private EnvironmentConfig environmentConfig;

		#endregion

		#region Constructor

		public Config()
		{
			var loader = new StreamingAssetsStorage(Game.ForceVision, null);
			loader.LoadStreamingAssetsFile(DefaultConfigFile, OnConfigLoaded, true);
		}

		#endregion

		#region Class Methods

		/// <summary>
		/// Gets the platform config.
		/// </summary>
		/// <returns>The platform config.</returns>
		/// <param name="serviceType">Service type.</param>
		public PlatformConfig GetPlatformConfig(ServiceType serviceType)
		{
			// getting the service
			ServiceConfig serviceConfig = environmentConfig.Services.First(service => service.Type == serviceType);

			PlatformConfig platformConfig = serviceConfig.Platforms.First(platform => platform.Type == PlatformType.iOS);
			#if UNITY_ANDROID
			platformConfig = serviceConfig.Platforms.First(platform => platform.Type == PlatformType.Android);
			#endif

			return platformConfig;
		}

		private string GetConfigSeed()
		{
			string seed = new StringBuilder()
				.Append(AudioEvent.AN)
				.Append(MenuAudioController.TG)
				.Append(Constants.ML)
				.Append(JCSettingsManager.LC)
				.Append(Localizer.KO)
				.Append(LogRemoteSocket.HM)
				.Append(PreLaunch.RG)
				.Append(Startup.JY)
				.Append(StreamingAssetsLoader.CD)
				.Append(EmitParticleFromEvent.PI)
				.Append(RotationOverTime.EB)
				.Append(SurfaceMenuNode.TP)
				.ToString();

			return seed;
		}

		#endregion

		#region Event Handlers

		protected void OnConfigLoaded(string error, byte[] config)
		{
			// logging error
			if (!string.IsNullOrEmpty(error))
			{	
				Log.Error(string.Format("Error! {0}", error));
				return;
			}

			// decrypt the config
			byte[] key = EncryptionUtils.VerifyKeyLength(GetConfigSeed());
			string configJson = EncryptionUtils.DecryptStringFromBytes(config, key);

			// setting the application config
			ApplicationConfig applicationConfig = JsonUtility.FromJson<ApplicationConfig>(configJson);

			// checking for the current environment
			EnvironmentType environmentType = EnvironmentType.Dev;
			#if RC_BUILD
			environmentType = EnvironmentType.Prod;
			#endif

			// setting the environment config
			environmentConfig = applicationConfig.GetEnvironmentConfig(environmentType);
		}

		#endregion
	}
}