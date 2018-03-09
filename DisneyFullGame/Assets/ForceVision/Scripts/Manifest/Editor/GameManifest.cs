using UnityEngine;
using UnityEditor;

namespace Disney.ForceVision.Internal
{
	public class GameManifest : ScriptableObject
	{
		/// <summary>
		/// The game root folder.
		/// </summary>
		public DefaultAsset GameRootFolder;

		/// <summary>
		/// The scenes the game needs in the player settings.
		/// </summary>
		public SceneAsset[] Scenes;

		/// <summary>
		/// The shaders the game needs to have flagged as preloaded.
		/// </summary>
		public Shader[] Shaders;
	}
}