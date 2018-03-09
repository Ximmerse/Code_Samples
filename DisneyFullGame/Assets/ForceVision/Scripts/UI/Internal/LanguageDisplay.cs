using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Disney.ForceVision;
using UnityEngine.EventSystems;
using System;

namespace Disney.ForceVision.Internal
{
	public class LanguageDisplay : MonoBehaviour, IPointerClickHandler
	{
		public static EventHandler<ToggleEventArgs> OnToggleClick { get; set; }

		public int LanguageId { get; private set; }
		public Image Flag;
		public Text Language;
		public GameObject SelectedIcon;

		public void Init(int languageId, bool isOn = false)
		{
			loadFlagImage("CountryFlags/" + Localizer.LanguageImages[languageId]);

			//TODO: lisum001 get tokenized language
			Language.text = Localizer.LanguageNames[languageId];

			LanguageId = languageId;

			GetComponent<Toggle>().isOn = isOn;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if(OnToggleClick != null)
			{
				OnToggleClick(this, new ToggleEventArgs(GetComponent<Toggle>()));
				AudioEvent.Play(GetComponent<Toggle>().isOn ? AudioEventName.Ftue.Stereo.PlusButton : AudioEventName.Ftue.Stereo.MinusButton, gameObject);
			}
		}

		public int GetLanguageId()
		{
			return LanguageId;
		}

		private void loadFlagImage(string path)
		{
			byte[] bytes = null;

			PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.ForceVision);
			if (persistentDataStorage.FileExists(path))
			{
				bytes = persistentDataStorage.LoadBytes(path);
			}
			else
			{
				StreamingAssetsStorage streamingAssetsStorage = new StreamingAssetsStorage(Game.ForceVision, this);
				streamingAssetsStorage.LoadStreamingAssetsFile(path, (success, loadedBytes) =>
				{
					if (string.IsNullOrEmpty(success))
					{
						bytes = loadedBytes;
					}
					else
					{
						Log.Error("No image flag found for " + path);
					}
				}, true);
			}

			if(bytes != null && bytes.Length > 0)
			{
				Texture2D flagTexture = new Texture2D(4, 4);
				bool isLoaded = flagTexture.LoadImage(bytes);
				if (isLoaded)
				{
					Flag.sprite = Sprite.Create(flagTexture, new Rect(0, 0, flagTexture.width, flagTexture.height), new Vector2(0, 0));
				}
			}

		}
	}
}