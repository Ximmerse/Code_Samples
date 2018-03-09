using UnityEngine;
using System;

namespace Disney.ForceVision.Internal
{
	public class StreamingAssetsObjectLoader<T> : StreamingAssetsLoaderBase
	{
		#region Private Fields

		/// <summary>
		/// The callback.
		/// </summary>
		private Action<string, T> callback;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.Internal.StreamingAssetsObjectLoader`1"/> class.
		/// </summary>
		/// <param name="monoBehaviour">Mono behaviour.</param>
		/// <param name="path">Path.</param>
		/// <param name="callback">Callback.</param>
		public StreamingAssetsObjectLoader(MonoBehaviour monoBehaviour, string path, Action<string, T> callback, bool isSynchronous = false)
		{
			this.callback = callback;
			Load(path, monoBehaviour, isSynchronous);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Loads the complete.
		/// </summary>
		protected override void LoadComplete()
		{
			object obj = null;
			try
			{
				obj = JsonUtility.FromJson<T>(text);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			callback(error, (T)obj);
		}

		#endregion
	}
}