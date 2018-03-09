using UnityEngine;
using UnityEditor;

namespace Disney.ForceVision
{
	public class BuildAssetFolderStructure : MonoBehaviour
	{
		[MenuItem("Assets/Create Asset Folders")]
		static void CreateFolder()
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			AssetDatabase.CreateFolder(path, "Textures");
			AssetDatabase.CreateFolder(path, "Materials");
			AssetDatabase.CreateFolder(path, "Meshes");
		}
	}
}