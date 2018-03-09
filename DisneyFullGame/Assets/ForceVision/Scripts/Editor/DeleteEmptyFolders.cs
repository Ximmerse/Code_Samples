using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/* editor script to automatically delete empty folders from the project, because git will not automatically delete empty folders,
 * and the project otherwise ends up littered with them when folders in the project are deleted or moved -- rhg
*/

[InitializeOnLoad]
public class DeleteEmptyFolders : Editor
{
	static int numFoldersDeleted = 0;
	static int numFoldersChecked = 0;

	public const string DeleteEmptyFoldersPrefKey = "DeleteEmptyFoldersEnabled";
	public const bool AutoDeleteDefault = true;
    
	// put any directories to exclude from auto-deletion here,
	// e.g. directories that are populated from other repositories -- rhg
	static string[] exclude_directories = {
		"Resources/secrets"
	};

	static bool savedKey
	{
		get
		{
			return EditorPrefs.GetBool(DeleteEmptyFoldersPrefKey, AutoDeleteDefault);
		}
		set
		{
			EditorPrefs.SetBool(DeleteEmptyFoldersPrefKey, value);
		}
	}

	// runs this script automatically on startup
	static DeleteEmptyFolders()
	{
#if false   // JMA: removing automated folder deletion for SWAR project.
        if(savedKey) {
			DeleteFolders();
		}
#endif
	}

	[MenuItem("Tools/DI Custom/Delete Empty Folders Now")]// add item to menu
    static void DeleteFolders()
	{
		//Logger.LogDebug(this, "Running DeleteEmptyFolders editor script...");    
		RemoveFolders("Assets");                    // start recursive call from root of Assets folder
		ShowDeletedFolderCount();                    // display output log of empty folders found
		AssetDatabase.Refresh();                    // refresh project hierarchy window in Unity editor
	}
	#if false // JMA Removing these options for SWAR project.
    [MenuItem("Tools/DI Custom/Empty Folders/", false)]
    static void Breaker () { }

	[MenuItem ("Tools/DI Custom/Empty Folders/Enable Auto Delete")]// add item to menu
    static void EnableAutoDelete() {
		savedKey = true;
        EditorUtility.DisplayDialog("Auto Delete ON", "Empty folders will be deleted automatically deleted.", "OK");
        DeleteFolders();
    }
    
	[MenuItem ("Tools/DI Custom/Empty Folders/Enable Auto Delete", true)]// add item to menu
    static bool ValidateEnableAutoDelete() {
		return savedKey == false;       
    }
    
	[MenuItem ("Tools/DI Custom/Empty Folders/Disable Auto Delete")]// add item to menu
    static void DisableAutoDelete() {
		savedKey = false;
        EditorUtility.DisplayDialog("Auto Delete OFF", "Empty folders will not be deleted.", "OK");
    }
    
	[MenuItem ("Tools/DI Custom/Empty Folders/Disable Auto Delete", true)]// add item to menu
    static bool ValidateDisableAutoDelete() {
		return savedKey == true;       
    }
#endif
	static void RemoveFolders(string path)
	{        // recursive function    
		string[] dirs = Directory.GetDirectories(path);
        
		foreach (string dirPath in dirs)
		{
			numFoldersChecked++;
			RemoveFolders(dirPath);                    // recursive call, performing depth-first search

			bool validForDeletion = false;

			// delete .DS_Store files
			if (Directory.GetDirectories(dirPath).Length == 0 && Directory.GetFiles(dirPath).Length == 1 && Path.GetFileName(Directory.GetFiles(dirPath)[0]) == ".DS_Store")
			{
				File.Delete(Directory.GetFiles(dirPath)[0]);
				validForDeletion = true;
			}
            
			// check if no files or folders inside the current path, to see if it's empty
			if (Directory.GetFiles(dirPath).Length == 0 && Directory.GetDirectories(dirPath).Length == 0)
			{
				validForDeletion = true;
				for (int i = 0; i < exclude_directories.Length; i++)
				{
					if (dirPath.EndsWith(exclude_directories[i]))
					{
						validForDeletion = false;
						Debug.LogWarning("excluding directory from auto deletion: " + dirPath);
					}
				}
			}    

			if (validForDeletion)
			{
				Directory.Delete(dirPath);            // delete empty folder
				File.Delete(dirPath + ".meta");        // delete metafile also, if exists
				numFoldersDeleted++;
				Debug.LogWarning("Deleted empty folder at: " + dirPath);
			}
		}
	}

	static void ShowDeletedFolderCount()
	{
		if (numFoldersDeleted > 0)
		{
			Debug.LogWarning("Empty folders deleted: " + numFoldersDeleted + ".  Folders checked: " + numFoldersChecked + ".");
		}
		else if (!savedKey)
		{
			Debug.LogWarning("No empty folders found to delete.");
		}
		numFoldersDeleted = 0;                        // reset counters for next run
		numFoldersChecked = 0;
	}
}
