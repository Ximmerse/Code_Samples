using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision.Internal
{
	public class DownloadPopoutPanel : MonoBehaviour
	{
		public Text Progress;
		public GameObject OkButton;

		private void Start()
		{
			Progress.text = "Checking For Updates.";
		}

		public void DownloadComplete(bool downloadSuccess)
		{
			if (downloadSuccess == true)
			{
				Progress.text = "Download finished successfully";
			}
			else
			{
				Progress.text = "There was an error downloading files. Make sure your device is connected to the internet.";
			}
		}

		public void OnOkButtonPressed()
		{
			Destroy(gameObject);
		}

		public void UpdateProgress(Progress progress)
		{
			if (progress.CurrentSection >= progress.Sections)
			{
				if (progress.FileRemainingCount > 0)
				{
					Progress.text = "Downloading file " + (progress.StartingFileCount - progress.FileRemainingCount + 1) + " of " + progress.StartingFileCount;
				}
				else
				{
					Progress.text = "You are up to date.";
				}
			}
		}
	}
}