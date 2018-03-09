using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class CdnAssetLoaderApi
	{
		#region Public Fields

		/// <summary>
		/// Loads files from StreamingAssets.
		/// All file paths are relative to the StreamingAssets folder.
		/// </summary>
		/// <value>The streaming asset storage.</value>
		public StreamingAssetsStorage streamingAssetStorage { get; private set; }

		/// <summary>
		/// Loads files from PersistentData.
		/// /// All file paths are relative to the PersistentData folder.
		/// </summary>
		/// <value>The persistent data storage.</value>
		public PersistentDataStorage persistentDataStorage { get; private set; }

		#endregion

		public CdnAssetLoaderApi(MonoBehaviour monoBehaviour, Game game = Game.None, bool tryPersistentData = true)
		{
			streamingAssetStorage = new StreamingAssetsStorage(game, monoBehaviour);
#if !SKU_CHINA && (!UNITY_EDITOR || CDN_ENABLE_IN_UNITY_EDITOR)
			if (tryPersistentData)
				persistentDataStorage = new PersistentDataStorage(game);
#endif
		}

		/// <summary>
		/// Creates the persistent data storage for testing.
		/// </summary>
		/// <param name="game">Game.</param>
		public void CreatePersistentDataStorageForTesting(Game game = Game.None)
		{
			if (!Application.isEditor || Application.isPlaying)
			{
				Debug.LogError("CreatePersistentDataStorageForTesting() should only be run by the Unity Test Runner");
				return;
			}

			persistentDataStorage = new PersistentDataStorage(game);
		}

		#region Public Methods

		/// <summary>
		/// Loads the text.
		/// Checks if file at path exists in PersistentData and loads there, otherwise loads from StreamingAsset folder.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="path">Path.</param>
		public string LoadText(string path)
		{
			string loadedText = null;
			if (persistentDataStorage != null && persistentDataStorage.FileExists(path))
			{
				loadedText = persistentDataStorage.LoadText(path);
			}
			else
			{
				streamingAssetStorage.LoadStreamingAssetsText(path, (error, text) =>
				{
					if (string.IsNullOrEmpty(error))
					{
						loadedText = text;
					}
					else
					{
						Log.Error(error);
					}
				}, true);
			}
			return loadedText;
		}

		/// <summary>
		/// Loads the json.
		/// Checks if file at path exists in PersistentData and loads there, otherwise loads from StreamingAsset folder.
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T LoadJson<T>(string path) where T : new()
		{
			T loadedContent = new T();
			if (persistentDataStorage != null && persistentDataStorage.FileExists(path))
			{
				loadedContent = persistentDataStorage.LoadJsonObject<T>(path);
			}
			else
			{
				streamingAssetStorage.LoadStreamingAssetsObject<T>(path, (error, content) =>
				{
					if (string.IsNullOrEmpty(error))
					{
						loadedContent = content;
					}
					else
					{
						Log.Error(error);
					}
				}, true);
			}
			return loadedContent;
		}

		/// <summary>
		/// Loads the bytes.
		/// Checks if file at path exists in PersistentData and loads there, otherwise loads from StreamingAsset folder.
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="path">Path.</param>
		public byte[] LoadBytes(string path)
		{
			byte[] loadedContent = null;
			if (persistentDataStorage != null && persistentDataStorage.FileExists(path))
			{
				loadedContent = persistentDataStorage.LoadBytes(path);
			}
			else
			{
				streamingAssetStorage.LoadStreamingAssetsFile(path, (error, bytes) =>
				{
					if (string.IsNullOrEmpty(error))
					{
						loadedContent = bytes;
					}
					else
					{
						Log.Error(error);
					}
				}, true);
			}
			return loadedContent;
		}

		#endregion
	}
}