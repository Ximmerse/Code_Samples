using System;
using System.Collections;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	/// <summary>
	/// Streaming assets loader base.
	/// </summary>
	public abstract class StreamingAssetsLoaderBase
	{
		
		#region Public Fields

		public const string InvalidArguments = "StreamingAssetsStorage Can't load asynchrounously from StreamingAssets without passing a monoBehaviour.";

		#endregion

		#region Private Fields

		protected string error = null;
		protected byte[] bytes = null;
		protected string text = null;
		protected Texture texture = null;
		protected Texture nonReadableTexture = null;

		#endregion

		#region Protected Methods

		protected void Load(string path, MonoBehaviour monoBehaviour, bool isSynchronous = false)
		{
			if (isSynchronous == false && monoBehaviour == null)
			{
				error = InvalidArguments;
				LoadComplete();
				return;
			}

			if (isSynchronous == true)
			{
				LoadSynchronousFileFromStreamingAssets(path);
			}
			else
			{
				monoBehaviour.StartCoroutine(LoadFileFromStreamingAssets(path));
			}
		}

		protected abstract void LoadComplete();

		#endregion

		#region Private Methods

		/// <summary>
		/// Loads the synchronous file from streaming assets.
		/// </summary>
		/// <param name="path">Path.</param>
		public void LoadSynchronousFileFromStreamingAssets(string path)
		{
			WWW www = new WWW(path);

			while (www.isDone == false)
			{
				//nothing
			}

			FinishLoad(www);
		}

		/// <summary>
		/// Loads the file from streaming assets.
		/// </summary>
		/// <returns>The file from streaming assets.</returns>
		/// <param name="path">Path.</param>
		IEnumerator LoadFileFromStreamingAssets(string path)
		{
			WWW www = new WWW(path);

			yield return www;

			FinishLoad(www);
		}

		/// <summary>
		/// Finishs the load.
		/// </summary>
		/// <param name="www">Www.</param>
		private void FinishLoad(WWW www)
		{
			error = www.error;
			if (string.IsNullOrEmpty(www.error) == true)
			{
				bytes = www.bytes;
				text = www.text;
				texture = www.texture;
				nonReadableTexture = www.textureNonReadable;
			}
			www.Dispose();
			LoadComplete();	
		}

		#endregion

	}
}