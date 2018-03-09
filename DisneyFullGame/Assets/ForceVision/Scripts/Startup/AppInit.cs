using System;
using System.Collections;
using System.Collections.Generic;
using Disney.ForceVision.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Disney.Vision;

namespace Disney.ForceVision
{
	public static class AppInit
	{
		#if !RC_BUILD
		// Socket Logger Port
		private const int DefaultLogRemotePort = 9550;

		private static LogRemote remoteLogger;
		#endif

		private static bool isInitialized = false;

		#if UNITY_ANDROID && !UNITY_EDITOR
		public static void InitApp()
		#else
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnBeforeSceneLoadRuntimeMethod()
		#endif
		{
			if (isInitialized == false)
			{
#if !ENABLE_LOGS
				Debug.unityLogger.logEnabled = false;
				Ximmerse.InputSystem.XDevicePlugin.s_isLogEnabled = false;
#endif

				//init app config
				IApplicationConfig ApplicationConfig = new Config();

#if !UNITY_EDITOR_WIN
				//init analytics
				Analytics.Init(ApplicationConfig.GetPlatformConfig(ServiceType.Swrve));

				//init Crittercism
				Crittercism.Init(ApplicationConfig.GetPlatformConfig(ServiceType.Apteligent).Id);
				Crittercism.SetLogUnhandledExceptionAsCrash(false);
#endif

#if !UNITY_EDITOR
				//init KpiTracking
				KpiTracking.InitKpiTracking();
#endif

				//init remote logging
				#if !RC_BUILD
				remoteLogger = new LogRemote(DefaultLogRemotePort);
				Log.Debug("remoteLogger = " + remoteLogger);

				SceneManager.sceneLoaded += OnSceneLoaded;

				#endif

				#if !SKU_CHINA 
				DownloadController.PurgeCpipeCache();
				#endif

				isInitialized = true;
			}
				
		}

		// make sure every scene uses our JSSettingsManager and logger
		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			VisionSDK[] sdks = GameObject.FindObjectsOfType<VisionSDK>();

			if (sdks.Length > 1)
			{
				Exception e = new Exception("multiple VisionSDKs in scene " + scene.name);
				Log.Exception(e);
			}
			foreach (VisionSDK sdk in sdks)
			{
				if (!(sdk.Settings is JCSettingsManager))
				{
					Exception e = new Exception("VisionSDK (" + sdk.name + ") Settings is not of type JCSettingsManager in scene " + scene.name);
					Log.Exception(e);
				}
				if (!(sdk.Logger is VisionSdkLoggerProxy))
				{
					Exception e = new Exception("VisionSDK (" + sdk.name + ") Logger is not of type VisionSdkLoggerProxy in scene " + scene.name);
					Log.Exception(e);
				}
			}
		}

	}
}