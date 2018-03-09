using System;
using System.IO;
using Disney.ForceVision.Internal;
using UnityEngine;

namespace Disney.ForceVision
{

	public class StreamingAssetsStorage : IStreamingAssetsStorage
	{

		/// <summary>
		/// The monobehaviour is passed to the StreamingAssetsLoader and used for starting coroutines.
		/// </summary>
		private MonoBehaviour monoBehaviour;

		/// <summary>
		/// The streaming assets path.
		/// </summary>
		private string streamingAssetsPath = "";

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.StreamingAssetsStorage"/> class.
		/// </summary>
		/// <param name="streamingAssetsStorageFolder">Streaming assets storage folder.</param>
		/// <param name="monoBehaviour">Mono behaviour.</param>
		public StreamingAssetsStorage(Game game, MonoBehaviour monoBehaviour)
		{
			this.monoBehaviour = monoBehaviour;

			string folderName = "";
			if (game != Game.None)
			{
				folderName = game.ToString("f");
			}

			string streamingAssetsPathPrefix = "file://";
			#if UNITY_ANDROID && !UNITY_EDITOR
				streamingAssetsPathPrefix = "";
			#endif

			streamingAssetsPath = streamingAssetsPathPrefix + Path.Combine(Application.streamingAssetsPath, folderName);
		}

		/// <summary>
		/// Gets the streaming asset path.
		/// All StreamingAssets paths in Storage are relative to StoragePath.
		/// </summary>
		/// <returns>The streaming asset path.</returns>
		/// <param name="path">Path.</param>
		public string GetStreamingAssetPath(string path)
		{
			return Path.Combine(streamingAssetsPath, path);
		}

		//TODO: lisum001 add throttling of streamingassets loading if needed
		/// <summary>
		/// Loads the streaming assets file.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public void LoadStreamingAssetsFile(string path, Action<string, byte[]> callback, bool isSynchronous = false)
		{
			path = GetStreamingAssetPath(path);
			new StreamingAssetsLoader(monoBehaviour, path, callback, isSynchronous);
		}

		/// <summary>
		/// Loads the streaming assets text.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public void LoadStreamingAssetsText(string path, Action<string, string> callback, bool isSynchronous = false)
		{
			path = GetStreamingAssetPath(path);
			new StreamingAssetsLoader(monoBehaviour, path, callback, isSynchronous);
		}

		/// <summary>
		/// Loads the streaming assets texture.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public void LoadStreamingAssetsTexture(string path, Action<string, Texture> callback, bool isSynchronous = false)
		{
			path = GetStreamingAssetPath(path);
			new StreamingAssetsLoader(monoBehaviour, path, callback, true, isSynchronous);
		}

		/// <summary>
		/// Loads the streaming assets non readable texture.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public void LoadStreamingAssetsNonReadableTexture(string path, Action<string, Texture> callback, bool isSynchronous = false)
		{
			path = GetStreamingAssetPath(path);
			new StreamingAssetsLoader(monoBehaviour, path, callback, false, isSynchronous);
		}

		/// <summary>
		/// Loads the streaming assets json.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public void LoadStreamingAssetsObject<T>(string path, Action<string, T> callback, bool isSynchronous = false)
		{
			path = GetStreamingAssetPath(path);
			new StreamingAssetsObjectLoader<T>(monoBehaviour, path, callback, isSynchronous);
		}
		
	}
}