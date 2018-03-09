using UnityEngine;

namespace Disney.ForceVision
{
	public class RtueLightsaberDetail : MonoBehaviour
	{
		public GameObject GoToSettings;
		public StereoSetupController Controller;

		private void Start()
		{
			CheckPermissions();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (!pauseStatus)
			{
				CheckPermissions();
			}
		}

		public void OnGoToSettings()
		{
			if (Application.isEditor)
			{
				return;
			}

			#if UNITY_ANDROID
			Log.Debug("GoToSettings");
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			permissions.CallStatic("GotoApplicationSettings");
			#endif
		}

		private void CheckPermissions()
		{
			if (Application.isEditor)
			{
				return;
			}

			//check if permission then show settings prompt if not
			#if UNITY_ANDROID
			//check for permissons, pop screen if needed
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			object[] args = new object[1];
			args[0] = new string[] { "android.permission.ACCESS_COARSE_LOCATION" };
			bool isPermitted = permissions.CallStatic<bool>("CheckPermission", args);
			if (isPermitted == false)
			{
				GoToSettings.SetActive(true);
			}
			else
			{
				GoToSettings.SetActive(false);
			}
			#endif
		}

	}
}