using System;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class DownloadControllerFactory
	{

		#region Private Static Fields

		private static List<DownloadController> downloadControllers = new List<DownloadController>();

		#endregion

		#region Public static Methods

		/// <summary>
		/// Cancel the existing downloads.
		/// </summary>
		/// <returns><c>true</c> if cancel ; otherwise, <c>false</c>.</returns>
		public static void CancelAll()
		{
			for (int i = 0; i < downloadControllers.Count; i++)
			{
				if (downloadControllers[i] != null)
				{
					downloadControllers[i].Cancel();
					downloadControllers[i] = null;
				}
			}
			downloadControllers.Clear();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates the download controller.
		/// </summary>
		/// <returns>The download controller.</returns>
		/// <param name="monobehaviour">Monobehaviour.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="language">Language.</param>
		/// <param name="progressCallback">Progress callback.</param>
		/// <param name="overrideManifestVersion">Override manifest version.</param>
		public DownloadController CreateDownloadController(MonoBehaviour monobehaviour, Action<bool, int> callback = null, string language = "en_US", Action<Progress> progressCallback = null, string overrideManifestVersion = null)
		{
			DownloadController downloadController = new DownloadController(monobehaviour, callback, language, progressCallback, overrideManifestVersion);
			DownloadControllerFactory.downloadControllers.Add(downloadController);
			return downloadController;
		}

		#endregion
	}
}