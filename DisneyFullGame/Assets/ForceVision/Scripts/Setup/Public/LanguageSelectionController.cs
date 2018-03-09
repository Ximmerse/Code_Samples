using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Disney.ForceVision.Internal;

namespace Disney.ForceVision
{

	public class LanguageSelectionEvents
	{
		public static EventHandler OnLanguageSelected;
		public static EventHandler OnLanguageSelectionClosed;
	}

	public class LanguageSelectionController : MonoBehaviour
	{
		#region Properties

		public GameObject CloseButton;
		public GameObject TargetContent;
		public Button ConfirmButton;
		public GameObject LanguagePrefab;
		public GameObject LoadingWindow;
		public GameObject TitleScreen;
		public GameObject ContainerSoundBanks;

		private Toggle selectedToggle;
		private Toggle[] toggles;

		#endregion

		#region MonoBehaviour

		protected void Awake()
		{
			// displaying close button only if the setup FTUE has been completed
			CloseButton.SetActive(FtueDataController.IsFtueComplete(FtueType.Setup));
		}

		protected void Start()
		{
			string locale = null;
			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (prefsStorage.PrefKeyExists(Localizer.LanguagePrefKey) == true)
			{
				locale = prefsStorage.GetPrefString(Localizer.LanguagePrefKey);
			}
				
			toggles = new Toggle[Localizer.Locales.Count];
			//load language prefabs
			bool foundMatch = false;
			for (int i = 0; i < Localizer.Locales.Count; i++)
			{
				GameObject languageGameObject = GameObject.Instantiate(LanguagePrefab);
				toggles[i] = languageGameObject.GetComponent<Toggle>();

				if (languageGameObject != null)
				{
					if (languageGameObject.GetComponent<LanguageDisplay>() != null)
					{
						languageGameObject.transform.SetParent(TargetContent.transform, false);

						bool isOn = false;
						if (Localizer.Locales[i].Equals(locale))
						{
							isOn = true;
							foundMatch = true;
						}

						languageGameObject.GetComponent<LanguageDisplay>().Init(i, isOn);
					}
				}
			}

			LanguageDisplay.OnToggleClick += OnToggleClick;

			ConfirmButton.interactable = foundMatch;
		}

		protected void OnDestroy()
		{
			LanguageDisplay.OnToggleClick -= OnToggleClick;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handler for the event when the confirm button is selected
		/// </summary>
		public void OnConfirmButtonSelected()
		{
			int selectedLanguageId = 0;
			if (selectedToggle != null && selectedToggle.GetComponent<LanguageDisplay>() != null)
			{
				selectedLanguageId = selectedToggle.GetComponent<LanguageDisplay>().GetLanguageId();
			}
			StartCoroutine(ChangeLanguage(Localizer.Locales[selectedLanguageId]));

			ResetUI();

			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.CheckLaunch, gameObject);
		}

		/// <summary>
		/// Handler for the event when the close button on the language selection panel is selected
		/// </summary>
		public void OnCloseButtonSelected()
		{
			// playing exit animation
			GetComponent<Animator>().Play("Popup_Outro");

			// dispatching close event
			if (LanguageSelectionEvents.OnLanguageSelectionClosed != null)
			{
				LanguageSelectionEvents.OnLanguageSelectionClosed(this, new EventArgs());
			}
				
			ResetUI();

			// playing sound
			AudioEvent.Play(AudioEventName.Ftue.Stereo.ExitButton, gameObject);
		}

		/// <summary>
		/// Called when a toggle is clicked.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="eventArgs">Event arguments.</param>
		protected void OnToggleClick(object sender, ToggleEventArgs eventArgs)
		{
			selectedToggle = eventArgs.SelectedToggle;
			ConfirmButton.interactable = selectedToggle.isOn;
			for (int i = 0; i < toggles.Length; i++)
			{
				if (toggles[i] != selectedToggle)
				{
					toggles[i].isOn = false;
				}
			}
		}

		/// <summary>
		/// Close the language selection window and animate out.
		/// </summary>
		protected void LanguageSelectionComplete()
		{
			// dispatching event
			if (LanguageSelectionEvents.OnLanguageSelected != null)
			{
				LanguageSelectionEvents.OnLanguageSelected(this, new EventArgs());
			}

			// playing exit animation
			GetComponent<Animator>().Play("Popup_Outro");
		}

		#endregion

		IEnumerator ChangeLanguage(string locale)
		{
			yield return new WaitForEndOfFrame();

			if (locale != Localizer.Locale)
			{
#if SKU_CHINA
				OnAssetsReadyUseLanguage(locale);
#else
				LoadingWindow.GetComponent<DownloadPanel>().StartWindow(DownloadController.InstanceIdCount + 1);
				DownloadControllerFactory.CancelAll();
				DownloadControllerFactory factory = new DownloadControllerFactory();
				DownloadController downloadController = factory.CreateDownloadController(this, (success, id) =>
				{
					if(LoadingWindow != null && LoadingWindow.GetComponent<DownloadPanel>() != null)
					{
						LoadingWindow.GetComponent<DownloadPanel>().DownloadComplete(success, id);
						if (success == true)
						{
							Localizer.Load(true);
							OnAssetsReadyUseLanguage(locale);
						}
					}
				}, locale, (prog) =>
				{
					if(LoadingWindow != null && LoadingWindow.GetComponent<DownloadPanel>() != null)
					{
						LoadingWindow.GetComponent<DownloadPanel>().UpdateProgress(prog);
					}
				});
				yield return new WaitForEndOfFrame();
				downloadController.Init();
#endif
			}
			else
			{
				LanguageSelectionComplete();

				//load prelaunch if first time selecting language
				PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
				if (prefsStorage.PrefKeyExists(Localizer.LanguagePrefKey) == false)
				{
					prefsStorage.SetPrefString(Localizer.LanguagePrefKey, locale);

					//only reload prelaunch if first language selected isnt first language in list.
					//First language means they selected the default language for the app so no reload is necessary.

					if (locale.Equals(Localizer.Locales[0]))
					{
						OnCloseButtonSelected();
						if (TitleScreen != null)
						{
							TitleScreen.SetActive(true);
						}
					}
					else
					{
						UnityEngine.SceneManagement.SceneManager.LoadScene("PreLaunch");
					}
				}
			}

			ForceVisionAnalytics.LogLanguageSelect(locale);
		}

		private void OnAssetsReadyUseLanguage(string locale)
		{
			//set player pref and locale
			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			prefsStorage.SetPrefString(Localizer.LanguagePrefKey, locale);

			//set locale in Localizer
			Localizer.Locale = locale;

			string[] bankNames = Localizer.GetSoundBankNames();
			StartCoroutine(Localizer.ChangeWWiseLanguage(bankNames, bankNames));

			Localizer.Load(true);
			UnityEngine.SceneManagement.SceneManager.LoadScene("PreLaunch");
		}

		private void ResetUI()
		{
			if (selectedToggle != null)
			{
				selectedToggle.isOn = false;
			}

			ConfirmButton.interactable = false;
		}
	}
}