using UnityEngine;
using UnityEditor;

namespace Disney.ForceVision.Internal
{
	public static class CreateGameManifest
	{
		[MenuItem("Assets/Create/Force Vision/Game Manifest")]
		public static void CreateMyAsset()
		{
			GameManifest asset = ScriptableObject.CreateInstance<GameManifest>();

			AssetDatabase.CreateAsset(asset, "Assets/GameManifest.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
		}
	}
} 