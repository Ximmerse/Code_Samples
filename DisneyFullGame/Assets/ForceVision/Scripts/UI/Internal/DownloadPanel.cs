using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision.Internal
{
	public class DownloadPanel : MonoBehaviour
	{
		public Image Progress;
		public GameObject DownloadFinished;
		public GameObject ErrorScreen;
		public GameObject DownloadScreen;

		private int downloadControllerInstanceId;

		public void StartWindow(int id, bool isWaitForFoundItems = true)
		{
			downloadControllerInstanceId = id;
			Progress.fillAmount = 0;
			DownloadFinished.SetActive(false);
			DownloadScreen.SetActive(true);
			ErrorScreen.SetActive(false);
			gameObject.SetActive(isWaitForFoundItems);
		}

		public void DownloadComplete(bool downloadSuccess, int id)
		{
			if (downloadControllerInstanceId == DownloadController.InstanceIdCount)
			{
				if (downloadSuccess == true)
				{
					Progress.fillAmount = 1.0f;
					ErrorScreen.SetActive(false);
					DownloadScreen.SetActive(false);
				}
				else
				{ 
					ErrorScreen.SetActive(true);
					DownloadScreen.SetActive(false);
					return;
				}
				DownloadFinished.SetActive(true);
			}
		}

		public void OnCloseCancelCompleteButtonPressed()
		{
			DownloadControllerFactory.CancelAll();
			gameObject.SetActive(false);
		}

		public void UpdateProgress(Progress progress)
		{
			if (progress.CurrentSection == progress.Sections)
			{
				if (progress.FileRemainingCount > 0)
				{
					float slice = 1.0f / (float)progress.StartingFileCount;
					float remaining = (float)progress.FileRemainingCount / (float)progress.StartingFileCount;
					float percent = 1.0f - remaining - slice;
					if (percent < 0)
					{
						percent = 0;
					}
					Progress.fillAmount = percent + (slice * progress.PercentOfCurrentLoadingFileDownloaded);
				}
				else
				{
					Progress.fillAmount = 1.0f;
				}
			}
		}
	}
}