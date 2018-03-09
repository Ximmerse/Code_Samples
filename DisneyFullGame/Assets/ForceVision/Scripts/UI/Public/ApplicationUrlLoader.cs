using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum ApplicationUrlType
	{
		PrivacyPolicy,
		TermsOfUse,
		EquipmentIssue,
		GameIssue,
		OtherTerms,
		SupportedPhones,
		Shop,
		GvrUnitySDKLicense,
		GvrAudioWwiseSDKLicense,
		GvrAudioWwiseSDKNotice
	}

	public class ApplicationUrlLoader : MonoBehaviour
	{
		#region Constants

		private const string ApplicationUrlsFile = "application-urls.json";

		#endregion

		#region Properties

		[Header("Type used to retrieve URL in application-urls.json")]
		public ApplicationUrlType Type;

		protected ApplicationUrlData ApplicationUrls
		{
			get
			{
				if (applicationUrls == null)
				{
					// checking persistent storage first
					PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.ForceVision);
					if (persistentDataStorage.FileExists(ApplicationUrlsFile) == true)
					{
						applicationUrls = JsonUtility.FromJson<ApplicationUrlData>(persistentDataStorage.LoadText(ApplicationUrlsFile));
					}
					// retrieving urls from streaming assets
					else
					{
						StreamingAssetsStorage storage = new StreamingAssetsStorage(Game.ForceVision, null);
						storage.LoadStreamingAssetsText(ApplicationUrlsFile, (error, json) =>
						{
							if (error != null)
							{
								Log.Error("Error! Unable to load " + ApplicationUrlsFile + ".");
							}
							else
							{
								applicationUrls = JsonUtility.FromJson<ApplicationUrlData>(json);
							}
						}, true);
					}
				}

				return applicationUrls;
			}
		}

		private ApplicationUrlData applicationUrls;

		#endregion

		#region Event Handlers

		public void OnButtonSelected()
		{
			AudioEvent.Play(AudioEventName.Combat.Encounter1, gameObject);

			if (Type >= ApplicationUrlType.PrivacyPolicy && Type <= ApplicationUrlType.GvrAudioWwiseSDKNotice)
			{
				string url = "";
				switch (Type)
				{
					case ApplicationUrlType.PrivacyPolicy:
						url = Localizer.Get("App.URL.PrivacyPolicy");
						break;
					case ApplicationUrlType.TermsOfUse:
						url = Localizer.Get("App.URL.TermsOfUse");
						break;
					case ApplicationUrlType.EquipmentIssue:
						url = Localizer.Get("App.URL.EquipmentIssue");
						break;
					case ApplicationUrlType.GameIssue:
						url = Localizer.Get("App.URL.GameIssue");

					#if SKU_CHINA
					url = Localizer.Get("App.URL.EquipmentIssue");
					#endif

						break;
					case ApplicationUrlType.OtherTerms:
						url = Localizer.Get("App.URL.OtherTerms");
						break;
					case ApplicationUrlType.SupportedPhones:
						url = Localizer.Get("App.URL.SupportedPhones");
						break;
					case ApplicationUrlType.Shop:
						url = Localizer.Get("App.URL.Shop");
						break;
					case ApplicationUrlType.GvrUnitySDKLicense:
						url = Localizer.Get("App.URL.GvrUnitySDKLicense");
						break;
					case ApplicationUrlType.GvrAudioWwiseSDKLicense:
						url = Localizer.Get("App.URL.GvrAudioWwiseSDKLicense");
						break;
					case ApplicationUrlType.GvrAudioWwiseSDKNotice:
						url = Localizer.Get("App.URL.GvrAudioWwiseSDKNotice");
						break;
				}

				if (!string.IsNullOrEmpty(url))
				{
					url = url.Replace(" ", "%20");
					Application.OpenURL(url);
				}
				else
				{
					Log.Warning("Warning! There is no url provided for the given key (" + Type + ").");
				}
			}
		}

		#endregion
	}
}