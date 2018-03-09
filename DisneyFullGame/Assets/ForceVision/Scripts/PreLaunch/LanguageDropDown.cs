using System.Collections;
using System.Collections.Generic;
using Disney.ForceVision.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public class LanguageDropDown : MonoBehaviour
	{
		#region Public Fields

		/// <summary>
		/// The drop down.
		/// </summary>
		public Dropdown LanguageDropDownUI;

		/// <summary>
		/// The loading window prefab.
		/// </summary>
		public GameObject LoadingWindowPrefab;

		#endregion

		#region Private Fields

		private GameObject loadingWindow;

		private string locale;

		private int activeSelection = 0;

		#endregion

		private void Start()
		{
			Localizer.LoadLanguageDefinitions();

			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (prefsStorage.PrefKeyExists(Localizer.LanguagePrefKey) == true)
			{
				locale = prefsStorage.GetPrefString(Localizer.LanguagePrefKey);
			}

			//verrify pref is valid
			if (Localizer.Locales.IndexOf(locale) < 0)
			{
				locale = Localizer.Locales[0];
			}

			Localizer.Locale = locale;

			int count = 0;
			List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
			foreach (string languageName in Localizer.LanguageNames)
			{
				if (locale.Equals(Localizer.GetLocaleFromLanguage(languageName)))
				{
					activeSelection = count;
				}
				count++;
				options.Add(new Dropdown.OptionData() { text = languageName }); 
			}
			LanguageDropDownUI.options = options;
			LanguageDropDownUI.value = activeSelection;
		}

		#region Public Methods

		/// <summary>
		/// Raises the dropdown changed event.
		/// </summary>
		public void OnDropdownChanged()
		{
			string dropDownValue = LanguageDropDownUI.options[LanguageDropDownUI.value].text;
			locale = Localizer.GetLocaleFromLanguage(dropDownValue);
			StartCoroutine(ChangeLanguage());
		}

		#endregion

		IEnumerator ChangeLanguage()
		{
			yield return new WaitForEndOfFrame();

			if (locale != Localizer.Locale)
			{
				loadingWindow = Instantiate(LoadingWindowPrefab);
				loadingWindow.transform.SetParent(transform.parent.transform, false);
				DownloadControllerFactory.CancelAll();
				DownloadControllerFactory factory = new DownloadControllerFactory();
				DownloadController downloadController = factory.CreateDownloadController(this, (success, id) =>
				{
					loadingWindow.GetComponent<DownloadPanel>().DownloadComplete(success, id);
					if (success == true)
					{
						activeSelection = LanguageDropDownUI.value;
						Localizer.Load(true);

						//set player pref and locale
						PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
						prefsStorage.SetPrefString(Localizer.LanguagePrefKey, locale);

						//set locale in Localizer
						Localizer.Locale = locale;

						string[] bankNames = Localizer.GetSoundBankNames();
						StartCoroutine(Localizer.ChangeWWiseLanguage(bankNames, bankNames));
					}
					else
					{
						LanguageDropDownUI.value = activeSelection;
					}
				}, locale, (prog) =>
				{
					if(loadingWindow != null)
					{
						loadingWindow.GetComponent<DownloadPanel>().UpdateProgress(prog);
					}
				});
				yield return new WaitForEndOfFrame();
				downloadController.Init();
			}
		}

	}
}