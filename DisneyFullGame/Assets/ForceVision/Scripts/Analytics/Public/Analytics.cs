using DCPI.Platforms.SwrveManager;
using DCPI.Platforms.SwrveManager.Analytics;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Disney.ForceVision
{
	public static class Analytics
	{
		#region Constants

		private const string ANALYTICS_SERVICE_NAME = "SwrveManager";

		#endregion

		#region Properties

		private static bool isInited = false;
#if UNITY_EDITOR_WIN
		class DummyAnalytics : MonoBehaviour
		{
			public static DummyAnalytics instance;
			private void Awake()
			{
				if (instance == null)
				{
					instance = this;
					GameObject.DontDestroyOnLoad(gameObject);
				}
				else if (instance != this)
				{
					Destroy(gameObject);
				}
			}
			public void LogIAPAction(IAPAnalytics analytics) { }
			public void LogPurchaseAction(PurchaseAnalytics analytics) { }
			public void LogCurrencyGivenAction(CurrencyGivenAnalytics analytics) { }
			public void LogAction(ActionAnalytics analytics) { }
			public void LogFunnelAction(FunnelStepsAnalytics analytics) { }
			public void LogAdAction(AdActionAnalytics analytics) { }
			public void LogTestImpressionAction(TestImpressionAnalytics analytics) { }
			public void LogFailedReceiptAction(FailedReceiptAnalytics analytics) { }
			public void LogTimingAction(TimingAnalytics analytics) { }
			public void LogNavigationAction(NavigationActionAnalytics analytics) { }
			public void LogErrorAction(ErrorAnalytics analytics) { }
			public void LogGenericAction(string action) {	}
			public void LogGenericAction(string action, Dictionary<string, object> messageDetails) { }
		}

		private static DummyAnalytics instance
		{
			get
			{
				if (DummyAnalytics.instance == null)
				{
					GameObject gameObject = new GameObject();
					DummyAnalytics dummyAnalytics = gameObject.AddComponent<DummyAnalytics>();
					return dummyAnalytics;
				}
				return DummyAnalytics.instance;
			}
		}
#else


		private static SwrveManager instance
		{
			get
			{
				if (isInited == false)
				{
					return null;
				}

				return SwrveManager.instance;
			}
		}
#endif
        #endregion

        #region Class Methods

        public static void Init(PlatformConfig platformConfig)
		{
			if (platformConfig == null)
			{
				return;
			}
			if (isInited == false)
			{
				isInited = true;

				// creating SwrveManager instance
				GameObject go = (GameObject)Object.Instantiate(Resources.Load(ANALYTICS_SERVICE_NAME));
				go.name = ANALYTICS_SERVICE_NAME;

				// setting config for push notifications
				SwrveComponent.Instance.Config.PushNotificationEnabled = true;

				// additional initialization for Android
				#if UNITY_ANDROID
				SwrveComponent.Instance.Config.GCMSenderId = platformConfig.Sender;
				SwrveComponent.Instance.Config.GCMPushNotificationTitle = platformConfig.Title;
				#endif

				// initializing Swrve
				SwrveManager.instance.InitWithAnalyticsKeySecret(int.Parse(platformConfig.Id), platformConfig.Key);

				// setting log level for Swrve
				SwrveLog.Level = SwrveLog.LogLevel.Disabled;

				// getting Analytics gameobject
				GameObject analytics = new GameObject("Analytics");

				Object.DontDestroyOnLoad(analytics);

				// setting parent of SwrveManager to Analytics gameobject
				go.transform.SetParent(analytics.transform);
			}
		}

		public static void LogIAPAction(IAPAnalytics analytics)
		{
			instance.LogIAPAction(analytics);
		}

		public static void LogPurchaseAction(PurchaseAnalytics analytics)
		{
			instance.LogPurchaseAction(analytics);
		}

		public static void LogCurrencyGivenAction(CurrencyGivenAnalytics analytics)
		{
			instance.LogCurrencyGivenAction(analytics);
		}

		public static void LogAction(ActionAnalytics analytics)
		{
			if (instance == null)
			{				
				return;
			}
			instance.LogAction(analytics);
		}

		public static void LogFunnelAction(FunnelStepsAnalytics analytics)
		{
			instance.LogFunnelAction(analytics);
		}

		public static void LogAdAction(AdActionAnalytics analytics)
		{
			instance.LogAdAction(analytics);
		}

		public static void LogTestImpressionAction(TestImpressionAnalytics analytics)
		{
			instance.LogTestImpressionAction(analytics);
		}

		public static void LogFailedReceiptAction(FailedReceiptAnalytics analytics)
		{
			instance.LogFailedReceiptAction(analytics);
		}

		public static void LogTimingAction(TimingAnalytics analytics)
		{
			instance.LogTimingAction(analytics);
		}

		public static void LogNavigationAction(NavigationActionAnalytics analytics)
		{
			instance.LogNavigationAction(analytics);
		}

		public static void LogErrorAction(ErrorAnalytics analytics)
		{
			instance.LogErrorAction(analytics);
		}

		public static void LogGenericAction(string action)
		{
			instance.LogGenericAction(action);
		}

		public static void LogGenericAction(string action, Dictionary<string, object> messageDetails)
		{
			instance.LogGenericAction(action, messageDetails);
		}

		#endregion

	}
}