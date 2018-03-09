using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	[RequireComponent(typeof(Text))]
	public class LocalizedText : MonoBehaviour
	{
		#region Public Properties

		/// <summary>
		/// The token to use for this Text component.
		/// </summary>
		public string Token = "";

		/// <summary>
		/// If this Text is "Locked", which means you can't edit the token.
		/// </summary>
		public bool Locked = false;

		#endregion

		/// <summary>
		/// Copy of the original font before it is mapped to a different font for localization.
		/// Required in case user switches languages more than once, so that the font for each language is mapped from the original developer-selected font (not a mapped-to font).
		/// </summary>
		private Font _originalFont = null;

		#region Protected Methods

		protected virtual void Awake()
		{
			// Subscribe to LanguageChanged event to check for localized font switching.
			Localizer.OnLanguageChanged += LanguageChangedHandler;
			LanguageChangedHandler(); // Do initial font mapping for current language setting.
        }
		void OnDestroy()
		{
			Localizer.OnLanguageChanged -= LanguageChangedHandler;
		}
		void LanguageChangedHandler()
		{
			Text text = GetComponent<Text>();
			if (text)
			{
				text.text = Localizer.Get(Token);
				if (_originalFont == null)
					_originalFont = text.font;
				text.font = BSG.Util.LocalizedFontDatabase.GetInstance().GetMappedFont(_originalFont, gameObject);
			}
		}

		#endregion
	}
}