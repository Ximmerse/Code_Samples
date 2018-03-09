using System;
using System.Collections;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	/// <summary>
	/// Streaming assets loader.
	/// </summary>
	public class StreamingAssetsLoader : StreamingAssetsLoaderBase
	{
		#region Constants

		public const string CD = "aT/u";

		#endregion

		#region Private Properties

		/// <summary>
		/// The byte callback.
		/// </summary>
		private Action<string, byte[]> byteCallback = null;

		/// <summary>
		/// The text callback.
		/// </summary>
		private Action<string, string> textCallback = null;

		/// <summary>
		/// The texture callback.
		/// </summary>
		private Action<string, Texture> textureCallback = null;

		/// <summary>
		/// flag to determine which texture to use for the texture callback(readable or nonreadable).
		/// </summary>
		private bool isTextureReadable;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.StreamingAssetsLoader"/> class.
		/// </summary>
		/// <param name="monoBehaviour">Mono behaviour.</param>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public StreamingAssetsLoader(MonoBehaviour monoBehaviour, string path, Action<string, byte[]> callback, bool isSynchronous = false)
		{
			byteCallback = callback;
			Load(path, monoBehaviour, isSynchronous);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.StreamingAssetsLoader"/> class.
		/// </summary>
		/// <param name="monoBehaviour">Mono behaviour.</param>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public StreamingAssetsLoader(MonoBehaviour monoBehaviour, string path, Action<string, string> callback, bool isSynchronous = false)
		{
			textCallback = callback;
			Load(path, monoBehaviour, isSynchronous);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.StreamingAssetsLoader"/> class.
		/// </summary>
		/// <param name="monoBehaviour">Mono behaviour.</param>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="isTextureReadable">If set to <c>true</c> is texture readable.</param>
		/// <param name="isSynchronous">If set to <c>true</c> is synchronous.</param>
		public StreamingAssetsLoader(MonoBehaviour monoBehaviour, string path, Action<string, Texture> callback, bool isTextureReadable, bool isSynchronous = false)
		{
			textureCallback = callback;
			this.isTextureReadable = isTextureReadable;
			Load(path, monoBehaviour, isSynchronous);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Calls the callback.
		/// </summary>
		protected override void LoadComplete()
		{
			if (byteCallback != null)
			{
				byteCallback(error, bytes);
			}
			else if (textCallback != null)
			{
				textCallback(error, text);
			}
			else if (textureCallback != null)
			{
				if (isTextureReadable)
				{
					textureCallback(error, texture);
				}
				else
				{
					textureCallback(error, nonReadableTexture);
				}
			}
		}

		#endregion

	}
}