using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	/// <summary>
	/// Class for all trailer-playing functionality
	/// </summary>
	public class TrailerPlayer : MonoBehaviour
	{
		private const string IntroPlayedCountKey = "IntroPlayedCount";
		private const string StarWarsIntroVideo = "swjc-intro-video.mp4";

		public Button TrailerPlayButton;

		private PlayerPrefsStorage stereoStorage;

		protected PlayerPrefsStorage StereoStorage
		{
			get
			{
				if (stereoStorage == null)
				{
					// creating storage
					stereoStorage = new PlayerPrefsStorage(Game.ForceVision);
				}

				return stereoStorage;
			}
		}

		/// <summary>
		/// Number of times the trailer has been played
		/// </summary>
		/// <value>The play count.</value>
		public int PlayCount
		{
			get
			{
				return StereoStorage.GetPrefInt(IntroPlayedCountKey);
			}
		}

		/// <summary>
		/// Start playing the trailer with video controls showing
		/// </summary>
		public void OnIntroVideoPlaybackSelected()
		{
			AudioEvent.Play(AudioEventName.Combat.Encounter1, gameObject);

			StartCoroutine(PlayTrailer(true));
		}

		/// <summary>
		/// Plays the trailer
		/// </summary>
		/// <param name="showVideoControls">If set to <c>true</c> show video controls.</param>
		public IEnumerator PlayTrailer(bool showVideoControls = false)
		{
			StreamingAssetsStorage storage = new StreamingAssetsStorage(Game.ForceVision, null);
			string introVideoPath = "";

			//copying to persistent data may work, you can also try prefixing the path with "jar://"
			#if UNITY_ANDROID
			PersistentDataStorage persistentStorage = new PersistentDataStorage(Game.ForceVision);

			if (persistentStorage.FileExists(StarWarsIntroVideo) == false)
			{
				storage.LoadStreamingAssetsFile(StarWarsIntroVideo, (string error, byte[] bytes) =>
				{
					if (string.IsNullOrEmpty(error))
					{
						//save file in persistentdata
						bool writeSuccess = persistentStorage.SaveBytes(StarWarsIntroVideo, bytes);
						if (writeSuccess)
						{
							introVideoPath = persistentStorage.GetPersistentDataPath(StarWarsIntroVideo);
						}
						else
						{
							//handle error
							Log.Error("Error! Unable to save the video file to persistent data.");
						}
					}
					else
					{
						//handle error
						Log.Error("Error! Unable to load from streaming assets.");
					}
				}, true);
			}
			else
			{
				introVideoPath = persistentStorage.GetPersistentDataPath(StarWarsIntroVideo);
			}
			#else

			introVideoPath = storage.GetStreamingAssetPath(StarWarsIntroVideo);

			#endif

			// checking that path is valid string
			if (!string.IsNullOrEmpty(introVideoPath))
			{
				if (PlayCount > 0)
				{
					// setting button state
					if (TrailerPlayButton)
					{
						TrailerPlayButton.interactable = false;
					}

					// stopping background music
					AudioEvent.Play(AudioEventName.Ftue.Stereo.BackgroundMusicPause, gameObject);
				}

				if (Application.isEditor)
				{
					Debug.Log("Playing Trailer (Trailer cannot play in Editor)");
				}
				else
				{
					// playing intro video
					Handheld.PlayFullScreenMovie(introVideoPath,
					                             Color.black,
					                             showVideoControls ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden);
				}

				// adding pause
				yield return new WaitForSeconds(1.5f);

				if (PlayCount > 0)
				{
					// setting button state
					if (TrailerPlayButton)
					{
						TrailerPlayButton.interactable = true;
					}

					// starting background music
					AudioEvent.Play(AudioEventName.Ftue.Stereo.BackgroundMusicResume, gameObject);
				}

				// setting new play count
				StereoStorage.SetPrefInt(IntroPlayedCountKey, PlayCount + 1);
			}
		}
	}
}