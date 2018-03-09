using System.Collections.Generic;
using System.Collections;
using Disney.ForceVision.Internal;
using UnityEngine;
using System;
using System.Linq;

namespace Disney.ForceVision
{
	// Localizer

	/// <summary>
	/// The Localizer. This class allow access to the localizer for all games
	/// This also allows easy access for the LocalizedText component.
	/// </summary>
	public static class Localizer
	{
		#region Constants

		public const string KO = "mAls";
		public const string LanguagePrefKey = "LanguageForApp";
        public const string LanguageDefinitionsFileName =
#if SKU_CHINA // English and Chinese
           "languagesChina.txt";
#elif PULL_ALL_LANGUAGES
			"languagesALL.txt";
#else // EFIGSJ: English, French, Italian, German, Spanish, and Japanese
            "languages.txt";
#endif

		#endregion

		#region Public Properties

		/// <summary>
		/// List of devon versions to load/merge for this version of the game. Duplicated tokens take the value of the highest version.
		/// This list of versions MUST BE ORDERED lowest precedence to highest precedence, as the latter ones in the list override earlier ones. 
		/// Generally the order will be from lowest to highest version number.
		/// </summary>
#if SKU_CHINA // English and Chinese for People's Republic of China (PRC)
		public static string[] DevonVersions = new string[] { "1.0", "1.2" };	// Must be ordered lowest to highest precedence to resolve duplicate token values to highest precedence.
#else
		// Note: the minor version 1.2.1 only contains the international version of URLs for Chinese (to override the PRC versions in 1.2).
		public static string[] DevonVersions = new string[] { "1.0", "1.2", "1.21" };   // Must be ordered lowest to highest precedence to resolve duplicate token values to highest precedence.
#endif
		/// <summary>
		/// The Devon "Platform" version to use for new tokens pushed to Devon. Should normally be the highest Devon version. It has to match a version in Devon.
		/// It can be modified in the Localization Panel by the user (for the session).
		/// </summary>
		public static string DevonPushVersion = "1.2";

		public delegate void LanguageChangedAction();
		public static event LanguageChangedAction OnLanguageChanged;

#endregion

#region Private Properties

		private static bool loaded = false;
		private static readonly Dictionary<string, string> tokens = new Dictionary<string, string>();
		public static readonly Dictionary<string, string> tokensVersions = new Dictionary<string, string>();

		public static List<string> Locales { get; private set; }

		public static List<string> LanguageNames { get; private set; }

		public static List<string> LanguageFolders { get; private set; }

		public static List<string> LanguageImages { get; private set; }

#endregion

#region Public Methods

		/// <summary>
		/// Initializes the <see cref="Disney.ForceVision.Localizer"/> class.
		/// </summary>
		static Localizer()
		{
			Load();
		}

		/// <summary>
		/// Get the string for the specified token.
		/// </summary>
		/// <param name="token">Token.</param>
		public static string Get(string token)
		{
#if MARK_LOCALIZED_STRINGS // Expose unlocalized strings by marking the localized ones. Any unmarked strings are therefore not localized. --JMA
            if (tokens.ContainsKey(token))
            {
                return "<" + tokens[token] + ">";
            }
            else
            {
                return "[" + token + "]";   // This means the string is localized but does not yet exist in the database. --JMA
            }
#else
            return (tokens.ContainsKey(token)) ? tokens[token] : "[" + token + "]";
#endif
        }

        /// <summary>
        /// Determines whether this instance has the specified token.
        /// </summary>
        /// <returns><c>true</c> if this instance has the token; otherwise, <c>false</c>.</returns>
        /// <param name="token">Token.</param>
        public static bool Has(string token)
		{
#if MARK_LOCALIZED_STRINGS
            return true;    // We don't want higher level code to override with defaults. Instead, in Get(), we will return the token in square brackets. --JMA
#else
            return tokens.ContainsKey(token);
#endif
        }

        /// <summary>
        /// Stores the language.
        /// </summary>
        public static string Locale = "en_US";

		/// <summary>
		/// Gets the W wise folder name by locale.
		/// </summary>
		/// <returns>The W wise folder by locale.</returns>
		public static string GetWWiseFolderByLocale(string locale)
		{
			int index = Locales.IndexOf(locale);
			if (index > -1)
			{
				return LanguageFolders[index];
			}
			return LanguageFolders[0];
		}

		/// <summary>
		/// Gets the language from locale.
		/// </summary>
		/// <returns>The language from locale.</returns>
		/// <param name="locale">Locale.</param>
		public static String GetLanguageFromLocale(string locale)
		{
			int index = Locales.IndexOf(locale);
			if (index > -1)
			{
				return LanguageNames[index];
			}
			return LanguageNames[0];
		}


		/// <summary>
		/// Gets the locale from language.
		/// </summary>
		/// <returns>The locale from language.</returns>
		/// <param name="language">Language.</param>
		public static String GetLocaleFromLanguage(string language)
		{
			int index = LanguageNames.IndexOf(language);
			if (index > -1)
			{
				return Locales[index];
			}
			return Locales[0];
		}

		/// <summary>
		/// Gets the locale from player prefs.
		/// </summary>
		/// <returns>The locale from preference.</returns>
		public static String GetLocaleFromPref()
		{
			if (Locales == null)
			{
				LoadLanguageDefinitions();
			}

			PlayerPrefsStorage prefsStorage = new PlayerPrefsStorage(Game.ForceVision);
			if (prefsStorage.PrefKeyExists(Localizer.LanguagePrefKey) == true)
			{
				Locale = prefsStorage.GetPrefString(Localizer.LanguagePrefKey);
			}

			//verrify pref is valid
			if (Localizer.Locales.IndexOf(Locale) < 0)
			{
				Locale = Localizer.Locales[0];
			}

			return Locale;
		}

		public static String GetLanguageImageFromLocale(string locale)
		{
			int index = Locales.IndexOf(locale);
			if (index > -1)
			{
				return LanguageImages[index];
			}
			return LanguageImages[0];
		}

		/// <summary>
		/// Load the json data, force is reload is true.
		/// </summary>
		/// <param name="reload">If set to <c>true</c> reload.</param>
		public static void Load(bool reload = false)
		{
			if (loaded && !reload)
			{
				return;
			}

			if (reload)
			{
				tokens.Clear();
			}

			LoadLanguageDefinitions();

			GetLocaleFromPref();

			LoadLocalizationVersionsMerged(Locale, tokens, true);

			loaded = true;
		}

		/// <summary>
		/// Load the json data for all versions (from DevonVersions list) of the given local into a dictionary. Higher versions of same token override lower versions.
		/// </summary>
		/// <param name="locale">Name of locale for which to load strings (defaults to English "en_US").</param>
		/// <param name="dictTokens">Reference to preexisting dictionary to fill out. Null OK, in which case will allocate.</param>
		/// <returns>Reference to dictionary that was filled out.</returns>
		public static Dictionary<string, string> LoadLocalizationVersionsMerged(string locale, Dictionary<string, string> dictTokens = null, bool tryFromPersistentStorage = false)
		{
			if (dictTokens == null)
				dictTokens = new Dictionary<string, string>();

			CdnAssetLoaderApi cdnAssetLoaderAPI = new CdnAssetLoaderApi(null, Game.None, tryFromPersistentStorage);

			foreach (string devonVersion in DevonVersions)
			{
				string tokensFile = "Localization/" + Locale + "." + devonVersion.Replace(".", "_") + ".json";
				LocalizationTokens localizationTokens =  cdnAssetLoaderAPI.LoadJson<LocalizationTokens>(tokensFile);
				if (localizationTokens != null && localizationTokens.Tokens != null)
				{
					foreach (LocalizationToken token in localizationTokens.Tokens)
					{
#if UNITY_EDITOR
						if (dictTokens.ContainsKey(token.Token)) 
						{
							if (tokensVersions[token.Token] == devonVersion)
								Debug.LogError("Duplicate " + devonVersion + " Localization Token: '" + token.Token + "'");
						}
						tokensVersions[token.Token] = devonVersion;
#endif
						dictTokens[token.Token] = token.Value;  // Doing it this way instead of tokens.Add() avoids exception for duplicate token keys (which should not, but can and has happened).
					}
				}
			}
			return dictTokens;
		}

#if UNITY_EDITOR
		static public LocalizationTokens LoadLocalizationTokens(string locale = "en_US", Dictionary<string, string> dictTokens = null)
		{
			dictTokens = LoadLocalizationVersionsMerged(locale, dictTokens);
			LocalizationTokens localizationTokens = new LocalizationTokens();
			localizationTokens.Tokens = new LocalizationToken[dictTokens.Count];
			int iToken = 0;
			foreach (KeyValuePair<string, string> keyval in dictTokens)
			{
                localizationTokens.Tokens[iToken] = new LocalizationToken();

                localizationTokens.Tokens[iToken].Token = keyval.Key;
				localizationTokens.Tokens[iToken++].Value = keyval.Value;
			}
			return localizationTokens;
		}
#endif
		/// <summary>
		/// Loads the language definitions.
		/// </summary>
		public static void LoadLanguageDefinitions()
		{
			CdnAssetLoaderApi cdnAssetLoaderAPI = new CdnAssetLoaderApi(null, Game.ForceVision);
			LanguageDefinitions languageDefinitions = cdnAssetLoaderAPI.LoadJson<LanguageDefinitions>(LanguageDefinitionsFileName);

			Locales = languageDefinitions.languages.Select(language => language.code).ToList();
			LanguageNames = languageDefinitions.languages.Select(language => language.name).ToList();
			LanguageFolders = languageDefinitions.languages.Select(language => language.folder).ToList();
			LanguageImages = languageDefinitions.languages.Select(language => language.image).ToList();
		}

		/// <summary>
		/// Changes the W wise language.
		/// This method stops all sounds while changing languages. 
		/// The language is determined by Localizer.Locale value.
		/// Usage: StartCoroutine(Localizer.ChangeWWiseLanguage());.
		/// </summary>
		/// <param name="soundBanksToUnload">Sound banks to unload.</param>
		/// <param name="soundBanksToLoad">Sound banks to load.</param>
		public static IEnumerator ChangeWWiseLanguage(string[] soundBanksToUnload = null, string[] soundBanksToLoad = null)
		{
			AkSoundEngine.StopAll();

			if (soundBanksToUnload != null)
			{
				foreach (string soundBank in soundBanksToUnload)
				{
					AkBankManager.UnloadBank(soundBank);
				}
			}

			string device = "iOS";
#if UNITY_ANDROID
			device = "Android";
#endif
#if SKU_CHINA
			string path = Application.streamingAssetsPath+ "/Audio/GeneratedSoundBanks/" + device;
#else
			string path = Application.persistentDataPath + "/Audio/GeneratedSoundBanks/" + device;
#endif
			Log.Debug("path for persistentDataPath soundbanks = " + path);
			AKRESULT result = AkSoundEngine.AddBasePath(path);
			Log.Debug("AddBasePath Result = " + result);


			Log.Debug("Setting wwise language to " + GetWWiseFolderByLocale(Locale));
			AKRESULT setLanguageResult = AkSoundEngine.SetCurrentLanguage(GetWWiseFolderByLocale(Locale));
			if (setLanguageResult != AKRESULT.AK_Success)
			{
				Log.Error("Unable to set language in wwise. result = " + setLanguageResult);
			}

			yield return new WaitForEndOfFrame();

			if (soundBanksToLoad != null)
			{
				foreach (string soundBank in soundBanksToLoad)
				{
					AkBankManager.LoadBank(soundBank, false, false);
				}
			}

			BSG.Util.LocalizedFontDatabase.GetInstance().SetLanguage(); // Must call LocalizedFontDatabase.GetInstance().SetLanguage() before OnLanguageChange() so the Font DB is setup for calls from Font-updaters subscribed to OnLanguageChaned event.
			if (OnLanguageChanged != null)
				OnLanguageChanged();
		}

		/// <summary>
		/// Get the sound bank names for the Container app.
		/// </summary>
		/// <returns>Array of strings of Container app sound banks</returns>
		public static string[] GetSoundBankNames()
		{
			List<string> result = new List<string>();

			// Grab all containers that can potentially house sound banks.
			GameObject[] containerSoundbanks = new GameObject[]
			{
				GameObject.Find("Wwise"),
				GameObject.FindGameObjectWithTag("Container_Soundbanks")
			};

			// Find all the sound bank names in the sound bank containers to reload
			if (containerSoundbanks != null)
			{
				foreach (GameObject containerSoundbank in containerSoundbanks)
				{
					if (containerSoundbank != null)
					{
						AkBank[] banks = containerSoundbank.GetComponentsInChildren<AkBank>();

						if (banks != null)
						{
							for (int i = 0; i < banks.Length; i++)
							{
								result.Add(banks[i].bankName);
							}
						}
					}
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}