using System;
using UnityEngine;

namespace Disney.ForceVision
{
	public interface IStreamingAssetsStorage
	{

		/// <summary>
		/// Gets the streaming asset path.
		/// All StreamingAssets paths in Storage are relative to StoragePath.
		/// </summary>
		/// <returns>The streaming asset path.</returns>
		/// <param name="path">Path.</param>
		string GetStreamingAssetPath(string path);

		/// <summary>
		/// Loads the streaming assets file.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		void LoadStreamingAssetsFile(string path, Action<string, byte[]> callback, bool isSynchronous = false);

		/// <summary>
		/// Loads the streaming assets text.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		void LoadStreamingAssetsText(string path, Action<string, string> callback, bool isSynchronous = false);

		/// <summary>
		/// Loads the streaming assets texture.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		void LoadStreamingAssetsTexture(string path, Action<string, Texture> callback, bool isSynchronous = false);

		/// <summary>
		/// Loads the streaming assets non readable texture.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback: string is the WWW.error value.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		void LoadStreamingAssetsNonReadableTexture(string path, Action<string, Texture> callback, bool isSynchronous = false);

		/// <summary>
		/// Loads the streaming assets object.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		void LoadStreamingAssetsObject<T>(string path, Action<string, T> callback, bool isSynchronous = false);

	}
}
