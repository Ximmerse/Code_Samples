using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Disney.ForceVision
{

	public class PersistentDataStorage : IPersistentDataStorage
	{

		#region Private Fields

		/// <summary>
		/// The persistent data path relative to Game.
		/// </summary>
		private string persistentDataPath;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.PersistentDataStorage"/> class.
		/// </summary>
		/// <param name="game">Game.</param>
		public PersistentDataStorage(Game game)
		{
			string folderName = "";
			if (game != Game.None)
			{
				folderName = game.ToString("f");
			}
			persistentDataPath = Path.Combine(Application.persistentDataPath, folderName);
			if (game != Game.None)
			{
				CreateFolder("");
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the persistent data path relative to Game.
		/// </summary>
		/// <returns>The persistent data path.</returns>
		/// <param name="path">Path.</param>
		public string GetPersistentDataPath(string path)
		{
			return Path.Combine(persistentDataPath, path);
		}

		/// <summary>
		/// tries to open and return a FileStream at the path relative to Game.
		/// </summary>
		/// <returns>The file stream.</returns>
		/// <param name="path">Path.</param>
		public FileStream GetFileStream(string path)
		{
			path = GetPersistentDataPath(path);
			FileStream file = null;
			if (File.Exists(path) == true)
			{
				try
				{
					file = File.Open(path, FileMode.Open);
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
			}
			else
			{
				Log.Error("Path does not exist, returning null FileStream. path =  " + path);
			}
			return file;
		}

		/// <summary>
		/// Save or append text at the path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		/// <param name="text">Text.</param>
		/// <param name="isAppend">If set to <c>true</c> is append.</param>
		public bool SaveText(string path, string text, bool isAppend)
		{
			CreateFolder(path, true);
			path = GetPersistentDataPath(path);
			bool fileSaved = true;
			FileStream file = null;

			try
			{
				if (File.Exists(path) == false)
				{
					file = File.Create(path);
					file.Close();
				}

				if (isAppend == true)
				{
					File.AppendAllText(path, text);
				}
				else
				{
					File.WriteAllText(path, text);
				}
			}
			catch (Exception e)
			{
				fileSaved = false;
				Log.Error(e.Message);
			}

			return fileSaved;
		}

		/// <summary>
		/// Loads the text at Path relative to Game.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="path">Path.</param>
		public string LoadText(string path)
		{
			path = GetPersistentDataPath(path);
			string text = null;

			if (File.Exists(path) == true)
			{
				try
				{
					text = File.ReadAllText(path);
				}
				catch (Exception e)
				{
					Log.Error(e.Message);
				}
			}
			else
			{
				Log.Error("Path does not exist, returning null string. path =  " + path);
			}
			return text;
		}

		/// <summary>
		/// Save the bytes at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		/// <param name="bytes">Bytes.</param>
		public bool SaveBytes(string path, byte[] bytes)
		{
			CreateFolder(path, true);
			path = GetPersistentDataPath(path);
			bool fileSaved = true;
			try
			{
				File.WriteAllBytes(path, bytes);
			}
			catch (Exception e)
			{
				fileSaved = false;
				Log.Error(e.Message);
			}
			return fileSaved;
		}

		/// <summary>
		/// Loads the bytes at path relative to Game.
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="path">Path.</param>
		public byte[] LoadBytes(string path)
		{
			path = GetPersistentDataPath(path);
			byte[] bytes = null;
			try
			{
				bytes = File.ReadAllBytes(path);
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
			}
			return bytes;
		}

		/// <summary>
		/// Loads the partial bytes at path relative to Game.
		/// </summary>
		/// <returns>The partial bytes.</returns>
		/// <param name="path">Path.</param>
		/// <param name="byteSize">Byte size.</param>
		/// <param name="startPosition">Start position of bytes into the data to start loading from.</param>
		public byte[] LoadPartialBytes(string path, int byteSize, int startPosition)
		{
			path = GetPersistentDataPath(path);
			byte[] data = new byte[byteSize];
			if (File.Exists(path) == true)
			{
				int read;
				try
				{
					using (FileStream file = new FileStream(path, FileMode.Open))
					{
						file.Position = startPosition;
						read = 0;
						while (read != byteSize && file.Position < file.Length)
						{
							read += file.Read(data, read, byteSize - read);
						}
					}
				}
				catch (Exception e)
				{
					Log.Error(e.Message);
				}
			}
			return data;
		}

		/// <summary>
		/// Saves the partial bytes at path relative to Game. This will overwrite bytes, not insert them.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		/// <param name="data">Data.</param>
		/// <param name="startPosition">Number of bytes into the data to start saving.</param>
		public bool SavePartialBytes(string path, byte[] data, long startPosition)
		{
			CreateFolder(path, true);
			path = GetPersistentDataPath(path);
			bool fileSaved = true;
			if (File.Exists(path) == true)
			{
				FileStream stream = null;
				try
				{
					stream = File.Open(path, FileMode.Open);
					stream.Position = startPosition;
					stream.Write(data, 0, data.Length);
				}
				catch (Exception e)
				{
					fileSaved = false;
					Log.Error(e.Message);
				}
				finally
				{
					if (stream != null)
					{
						stream.Close();
					}
				}
			}
			else
			{
				Log.Error("Path does not exist. path =  " + path);
				fileSaved = false;
			}
			return fileSaved;
		}

		/// <summary>
		/// Saves the serializable class at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		/// <param name="data">Data.</param>
		public bool SaveSerialized(string path, object data)
		{
			CreateFolder(path, true);
			path = GetPersistentDataPath(path);
			bool fileSaved = true;
			FileStream file = null;

			try
			{
				if (File.Exists(path) == false)
				{
					file = File.Create(path);
				}
				else
				{
					file = new FileStream(path, FileMode.Open);
				}
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(file, data);
			}
			catch (Exception e)
			{
				fileSaved = false;
				Log.Error(e.Message);
			}
			finally
			{
				file.Close();
			}

			return fileSaved;
		}

		/// <summary>
		/// Loads the serialized class at path relative to Game.
		/// </summary>
		/// <returns>The serialized.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The class used for serialization</typeparam>
		public T LoadSerialized<T>(string path) where T : new()
		{
			path = GetPersistentDataPath(path);
			object data = null;

			if (File.Exists(path) == true)
			{
				FileStream file = null;
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					file = File.Open(path, FileMode.Open);
					data = binaryFormatter.Deserialize(file);
				}
				catch (Exception e)
				{
					Log.Error(e.Message);
				}
				finally
				{
					file.Close();
				}
			}
			else
			{
				Log.Error("Path does not exist, returning empty serialized class. path =  " + path);
			}

			if (data == null)
			{
				return new T();
			}
			else
			{
				return (T)data;
			}
		}

		/// <summary>
		/// Loads the json object at path relative to Game.
		/// </summary>
		/// <returns>The json object.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T LoadJsonObject<T>(string path) where T : new()
		{
			try
			{
				string json = LoadText(path);
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return new T();
			}
		}

		/// <summary>
		/// Deletes the file at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		public bool DeleteFile(string path)
		{
			bool fileDeleted = true;
			path = GetPersistentDataPath(path);

			if (File.Exists(path) == true)
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception e)
				{
					fileDeleted = false;
					Log.Exception(e);
				}	
			}
			else
			{
				fileDeleted = false;
				Log.Error("Path does not exist. path =  " + path);
			}
			return fileDeleted;
		}

		/// <summary>
		/// Creates the folder at path relative to StorageFolder.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		/// <param name="isStripLast">If set to <c>true</c> is strip last.</param>
		public bool CreateFolder(string path, bool isStripLast = false)
		{
			path = GetPersistentDataPath(path);
			if (isStripLast == true)
			{
				int index = path.LastIndexOf("/");
				path = path.Substring(0, index);
			}

			bool folderCreated = true;
			if (Directory.Exists(path) == true)
			{
				folderCreated = false;
			}
			else
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch (Exception e)
				{
					folderCreated = false;
					Log.Exception(e);
				}
			}
			return folderCreated;
		}

		/// <summary>
		/// Deletes the folder at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		public bool DeleteFolder(string path)
		{
			bool folderDeleted = true;
			path = GetPersistentDataPath(path);

			if (Directory.Exists(path))
			{
				try
				{
					Directory.Delete(path, true);
				}
				catch (Exception e)
				{
					folderDeleted = false;
					Log.Exception(e);
				}
			}
			else
			{
				folderDeleted = false;
				Log.Error("Path does not exist. path =  " + path);
			}
			return folderDeleted;
		}

		/// <summary>
		/// Check if a file exists at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		public bool FileExists(string path)
		{
			path = GetPersistentDataPath(path);
			return File.Exists(path);
		}

		/// <summary>
		/// Check if a folder exists at path relative to Game.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="path">Path.</param>
		public bool FolderExists(string path)
		{
			path = GetPersistentDataPath(path);
			return Directory.Exists(path);
		}

		#endregion
	}
}