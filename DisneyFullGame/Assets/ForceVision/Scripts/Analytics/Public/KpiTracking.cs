using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Disney.ForceVision.Internal;
using DCPI.Platforms.SwrveManager.Analytics;

namespace Disney.ForceVision
{

	public class KpiTracking : MonoBehaviour
	{
		#region Public Static Fields

		/// <summary>
		/// The kpi tracking static reference.
		/// </summary>
		public static KpiTracking kpiTracking;

		public static bool ShowMagReading = false;
		public static bool ShowFps = false;

		/// <summary>
		/// The name of the kpi group.
		/// </summary>
		public const string KpiGroupName = "perf";

		#endregion

		#region Public Fields

		[Header("Interval in seconds to record data.")]
		public int RecordInterval = 60;

		/// <summary>
		/// The fps.
		/// </summary>
		public FpsTracking Fps;

		/// <summary>
		/// The scene tracking.
		/// </summary>
		public SceneLoadTimeTracking sceneTracking;

		#endregion

		#region Private Fields

		private static bool isInitialized = false;

		private float time = 0;

		private NativeSettings nativeSettings = null;

		private int kpiCallCount = 0;

		private int loadTimeCount = 0;

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Tracks the scene load time.
		/// </summary>
		public static void TrackSceneLoadTime()
		{
			if (kpiTracking != null && kpiTracking.sceneTracking != null)
			{
				kpiTracking.sceneTracking.TrackSceneLoadTime();
			}
		}

		public static void InitKpiTracking()
		{
			Log.Debug("init kpi tracking");
			if (isInitialized == false)
			{
				isInitialized = true;
				Log.Debug("Adding KpiTracking prefab to scene.");
				GameObject instance = Instantiate(Resources.Load("KpiTracking", typeof(GameObject))) as GameObject;
				instance.name = "KpiTracking";
			}
		}

		#endregion

		#region Private Methods

		private void Start()
		{
			DontDestroyOnLoad(this);
			StartCoroutine(SetStaticReference());
			time = Time.realtimeSinceStartup;
			nativeSettings = new NativeSettings();

			#if RC_BUILD
			RecordInterval = 60;
			#endif
		}

		private void Update()
		{
			if (Time.realtimeSinceStartup - time >= RecordInterval)
			{
				time = Time.realtimeSinceStartup;
				RecordKpis();
			}
		}

		private void OnGUI()
		{
			if (ShowMagReading == true)
			{
				GUI.skin.label.fontSize = 42;
				float magHeading = Input.compass.magneticHeading;
				GUI.Label(new Rect(10, 150, Screen.width, Screen.height), "Magnent reading: " + magHeading);
			}

			if (ShowFps == true)
			{
				GUI.skin.label.fontSize = 42;
				GUI.Label(new Rect(10, 200, Screen.width, Screen.height), "   FPS : " + (int)Fps.GetFps());
			}
		}

		IEnumerator SetStaticReference()
		{
			yield return new WaitForEndOfFrame();
			KpiTracking.kpiTracking = this;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Records the kpis.
		/// </summary>
		public void RecordKpis()
		{
			kpiCallCount++;

			string sceneName = SceneManager.GetActiveScene().name;

			float fps = Fps.GetFps();

			float playSession = Time.realtimeSinceStartup;

			long availableMemory = nativeSettings.GetAvailableMemory() / 1024 / 1024;

			float battery = nativeSettings.GetBatteryRemaining();

			Log.Debug("fps = " + fps + " sessionLength = " + playSession + " availableMemory = " + availableMemory + " battery = " + battery);

			Dictionary<string, string> payload = new Dictionary<string, string>();
			payload.Add("count", kpiCallCount.ToString());
			payload.Add("fps", fps.ToString());
			payload.Add("playTime", playSession.ToString());
			payload.Add("scene", sceneName);
			SwrveComponent.Instance.SDK.NamedEvent(KpiGroupName + ".fps", payload);

			payload.Clear();
			payload.Add("count", kpiCallCount.ToString());
			payload.Add("availableMemory", availableMemory.ToString());
			payload.Add("playTime", playSession.ToString());
			payload.Add("scene", sceneName);
			SwrveComponent.Instance.SDK.NamedEvent(KpiGroupName + ".memory", payload);

			payload.Clear();
			payload.Add("count", kpiCallCount.ToString());
			payload.Add("battery", battery.ToString());
			payload.Add("playTime", playSession.ToString());
			payload.Add("scene", sceneName);
			SwrveComponent.Instance.SDK.NamedEvent(KpiGroupName + ".battery", payload);
		}

		/// <summary>
		/// Records the scene load time.
		/// </summary>
		/// <param name="fromScene">From scene.</param>
		/// <param name="toScene">To scene.</param>
		/// <param name="loadTime">Load time.</param>
		public void RecordSceneLoadTime(string fromScene, string toScene, float loadTime)
		{
			Log.Debug(fromScene + " to " + toScene + " took " + loadTime + " seconds");

			loadTimeCount++;

			Dictionary<string, string> payload = new Dictionary<string, string>();
			payload.Add("count", loadTimeCount.ToString());
			payload.Add("fromScene", fromScene);
			payload.Add("toScene", toScene);
			payload.Add("loadTime", loadTime.ToString());
			payload.Add("playTime", Time.realtimeSinceStartup.ToString());
			SwrveComponent.Instance.SDK.NamedEvent(KpiGroupName + ".loadtime", payload);
		}

		#endregion
	}

}

