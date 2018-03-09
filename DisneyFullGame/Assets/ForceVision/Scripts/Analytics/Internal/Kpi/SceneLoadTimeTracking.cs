using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Disney.ForceVision.Internal
{

	public class SceneLoadTimeTracking : MonoBehaviour
	{
		private float startLoadTime = 0;
		private string fromScene = "";
		private bool isTracking = false;

		private Dictionary<string, float> timeInEachScene;

		private void Start()
		{
			SceneManager.sceneLoaded += SceneLoaded;
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= SceneLoaded;
		}

		public void TrackSceneLoadTime()
		{
			fromScene = SceneManager.GetActiveScene().name;
			startLoadTime = Time.realtimeSinceStartup;
			isTracking = true;
		}

		private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (isTracking == true)
			{
				isTracking = false;
				KpiTracking.kpiTracking.RecordSceneLoadTime(fromScene, scene.name, Time.realtimeSinceStartup - startLoadTime);
			}
		}
			
	}
}