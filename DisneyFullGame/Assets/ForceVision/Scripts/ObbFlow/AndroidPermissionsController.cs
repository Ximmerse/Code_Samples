using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public class AndroidPermissionsController : MonoBehaviour
	{
		public GameObject PromptUI;
		public GameObject PromptPermissionsBtn;
		public GameObject GoToSettingsBtn;

		#if UNITY_ANDROID && !UNITY_EDITOR
		private ContainerAPI container;
		private string readPermission = "android.permission.READ_EXTERNAL_STORAGE";
		private string writePermission = "android.permission.WRITE_EXTERNAL_STORAGE";
		private bool checkForPermissionsRequestable = true;
		private float time = 0;
		#endif

		void Start()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			time = 0;
			PromptUI.SetActive(true);
			if (CheckPermissions() == true)
			{
				StartObbCheck();
			}
			else
			{
				container = new ContainerAPI(Game.ForceVision);
				PromptPermissionsBtn.SetActive(true);
				GoToSettingsBtn.SetActive(false);
			}
			#else
			SceneManager.LoadScene("PreLaunch");
			#endif
		}
		
		#if UNITY_ANDROID && !UNITY_EDITOR
		void Update()
		{
			time += Time.unscaledDeltaTime;
			if (time > 1.5f)
			{
				time = 0;
				if (checkForPermissionsRequestable == true)
				{
					bool isWriteExternalStorageRequestable = container.NativeSettings.IsPermissonRequestable(writePermission);
					bool isReadExternalStorageRequestable = container.NativeSettings.IsPermissonRequestable(readPermission);

					if (isWriteExternalStorageRequestable == true && isReadExternalStorageRequestable == true)
					{
						Log.Debug("both permissions still requestable");
						if (CheckPermissions() == true)
						{
							StartObbCheck();
						}
					}
					else
					{
						if (CheckPermissions() == true)
						{
							StartObbCheck();
						}
						Log.Debug("permission not requestable");
						PromptPermissionsBtn.SetActive(false);
						GoToSettingsBtn.SetActive(true);
					}
				}
			}
		}
		private void StartObbCheck()
		{
			Log.Debug("StartObbCheck");
			checkForPermissionsRequestable = false;

			if (container != null)
			{
				container.Dispose();
			}

			PromptUI.SetActive(false);
			this.gameObject.GetComponent<ObbPermissionsController>().Init();
		}
		#endif
		public bool CheckPermissions()
		{
			Log.Debug("Check Permissions");
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			object[] args = new object[1];
			args[0] = new string[] { readPermission };
			bool readPermissionAccepted = permissions.CallStatic<bool>("CheckPermission", args);

			args = new object[1];
			args[0] = new string[] { writePermission };
			bool writePermissionAccepted = permissions.CallStatic<bool>("CheckPermission", args);
			if (readPermissionAccepted == true && writePermissionAccepted == true)
			{
				return true;
			}
			#endif
			return false;
		}

		public void PromptWritePermission()
		{
			Log.Debug("Show Permissions Prompt");
			#if UNITY_ANDROID && !UNITY_EDITOR
			List<string> permissionsList = new List<string>();
			permissionsList.Add(readPermission);
			permissionsList.Add(writePermission);

			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			object[] args = new object[1];
			args[0] = permissionsList.ToArray();
			permissions.CallStatic("RequestPermission", args);
			#endif
		}

		public void GoToSettings()
		{
			Log.Debug("GoToSettings");
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass permissions = new AndroidJavaClass("com.disney.forcevision.permissions.Permissions");
			permissions.CallStatic("GotoApplicationSettings");
			#endif
		}
	}
}