using UnityEngine;
using UnityEditor;

namespace Disney.ForceVision.Internal
{
	public static class CreateDevonLogin
	{
		[MenuItem("Assets/Create/Force Vision/Devon Login")]
		public static void CreateMyAsset()
		{
			DevonLogin asset = ScriptableObject.CreateInstance<DevonLogin>();

			if (!System.IO.Directory.Exists(Application.dataPath + "/ForceVision/Logins"))
			{
				System.IO.Directory.CreateDirectory(Application.dataPath + "/ForceVision/Logins");
			}

			AssetDatabase.CreateAsset(asset, "Assets/ForceVision/Logins/NewLogin.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
		}
	}
}