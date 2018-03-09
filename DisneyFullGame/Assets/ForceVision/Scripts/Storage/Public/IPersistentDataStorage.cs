using System.IO;

namespace Disney.ForceVision
{
	public interface IPersistentDataStorage
	{

		/// <summary>
		/// Each game has a designated folder that lives under the Application.persistentData path
		/// </summary>
		string GetPersistentDataPath(string path);

		/// <summary>
		/// tries to open and return a FileStream at the path relative to Game.
		/// </summary>
		/// <returns>The file stream.</returns>
		/// <param name="path">Path.</param>
		FileStream GetFileStream(string path);

		/// <summary>
		/// Save or append text at the path relative to Game.
		/// </summary>
		/// <returns><c>true</c>, if text was saved, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="text">Text.</param>
		/// <param name="isAppend">If set to <c>true</c> text is appendend to end of file.</param>
		bool SaveText(string path, string text, bool isAppend);

		/// <summary>
		/// Loads the text at Path relative to Game.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="path">Path.</param>
		string LoadText(string path);

		/// <summary>
		/// Save the bytes at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if bytes was saved, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="bytes">Bytes.</param>
		bool SaveBytes(string path, byte[] bytes);

		/// <summary>
		/// Loads the bytes at path relative to Game.
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="path">Path.</param>
		byte[] LoadBytes(string path);

		/// <summary>
		/// Saves the partial bytes at path relative to Game. This will overwrite bytes, not insert them.
		/// </summary>
		/// <returns><c>true</c>, if partial bytes was saved, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="data">Data.</param>
		/// <param name="startPosition">Number of bytes into the data to start saving.</param>
		bool SavePartialBytes(string path, byte[] data, long startPosition);

		/// <summary>
		/// Loads the partial bytes at path relative to Game.
		/// </summary>
		/// <returns>The partial bytes.</returns>
		/// <param name="path">Path.</param>
		/// <param name="byteSize">Byte size.</param>
		/// <param name="startPosition">Start position of bytes into the data to start loading from.</param>
		byte[] LoadPartialBytes(string path, int byteSize, int startPosition);

		/// <summary>
		/// Saves the serializable class at path relative to Game.
		/// </summary>
		/// <returns><c>true</c>, if serialized was saved, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="data">Data.</param>
		bool SaveSerialized(string path, object data);

		/// <summary>
		/// Loads the serialized class at path relative to Game.
		/// </summary>
		/// <returns>The serialized.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The class used for serialization</typeparam>
		T LoadSerialized<T>(string path) where T : new();

		/// <summary>
		/// Loads the json object at path relative to Game.
		/// </summary>
		/// <returns>The json object.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T LoadJsonObject<T>(string path) where T : new();

		/// <summary>
		/// Deletes the file  at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if file was deleted, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		bool DeleteFile(string path);

		/// <summary>
		/// Creates the folder at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if folder was created, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="isSplitLast">If set to <c>true</c> is split last.</param>
		bool CreateFolder(string path, bool isSplitLast = false);

		/// <summary>
		/// Deletes the folder at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if folder was deleted, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		bool DeleteFolder(string path);

		/// <summary>
		/// Check if a file exists at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if file exists, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		bool FileExists(string path);

		/// <summary>
		/// Check if a folder exists at path relative to StorageFolder.
		/// </summary>
		/// <returns><c>true</c>, if folder exists, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		bool FolderExists(string path);

	}
}
