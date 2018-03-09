using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class ObbPermissionsController : MonoBehaviour
	{
		private string expansionFilePath;
		private bool isDownloadingObb;

		public static void OnObbComplete(bool success)
		{
			if (success == true)
			{
				#if UNITY_ANDROID && !UNITY_EDITOR
				AppInit.InitApp();
				#endif
				SceneManager.LoadScene("PreLaunch");
			}
		}

		public void Init()
		{
			#if !RC_BUILD || SKU_CHINA
			OnObbComplete(true);
			return;
			#else

			// see if obb is already downloaded
			expansionFilePath = GooglePlayDownloader.GetExpansionFilePath();
			if (GooglePlayDownloader.GetMainOBBPath(expansionFilePath) != null)
			{
				OnObbComplete(true);
			}
			else
			{
				Log.Debug("expansionFilePath = " + expansionFilePath);
				isDownloadingObb = true;
				GooglePlayDownloader.FetchOBB();
				StartCoroutine(WaitForObb());
			}
			#endif
		}
			
		private IEnumerator WaitForObb()
		{
			// native UI will show progress bar
			while (isDownloadingObb && GooglePlayDownloader.GetMainOBBPath(expansionFilePath) == null)
			{
				yield return new WaitForSeconds(0.5f);
			}

			isDownloadingObb = false;
			OnObbComplete(true);
		}
	}
}