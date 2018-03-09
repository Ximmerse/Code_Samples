using UnityEditor;
using UnityEngine;

public static class OpenPersistentData
{
	/// <summary>
	/// Open the Persistent Data folder in Finder / Explorer.
	/// Should work on both Mac and Windows.
	/// </summary>
	[MenuItem("Tools/DI Custom/Open Persistent Data Folder")]
	private static void OpenFolder()
	{
		EditorUtility.RevealInFinder(Application.persistentDataPath);
	}
}
