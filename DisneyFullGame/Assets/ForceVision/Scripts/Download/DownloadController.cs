using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Disney.ForceVision
{
	//lowercase public vars to match json from cpipe
	[Serializable]
	public struct AuthData
	{
		public int expires_in;
		public string token_type;
		public string access_token;
	}

	public struct Progress
	{
		public int StartingFileCount;
		public int FileRemainingCount;
		public int Sections;
		public int CurrentSection;
		public float PercentOfCurrentLoadingFileDownloaded;
	}

	public class DownloadController
	{
		public const string ManifestVersionFile = "manifestVersion.txt";
		public const string EmbeddedCpipeManifestFile = "cpipe_manifest.json";
		public const string appVerisonFile = "appVersion.txt";
		public const string CacheName = "cache";

		public static int InstanceIdCount { get; private set; }

		public int InstanceId { get; private set; }

		private static bool IsLoading = false;

		private MonoBehaviour monoBehaviour = null;
		private Action<bool, int> callback;
		private AuthData authData;
		private string cacheDirectory;
		private CloudStorageAPI cloudStorageAPI;
		private List<string> downloads;
		private JSONObject manifest;
		private PersistentDataStorage persistentStore;
		private Dictionary<string, double> cache = null;
		private int maxAttempts = 3;
		private string language = "";
		private bool isCancel = false;

		private Action<Progress> progressCallback;
		private Progress progress;
		private string overrideManifestVersion = null;

		public DownloadController(MonoBehaviour monoBehaviour, Action<bool, int> callback = null, string language = "en_US", Action<Progress> progressCallback = null, string overrideManifestVersion = null)
		{
			InstanceIdCount++;
			this.InstanceId = InstanceIdCount;
			this.monoBehaviour = monoBehaviour;
			this.callback = callback;
			this.language = language;
			this.progressCallback = progressCallback;
			this.overrideManifestVersion = overrideManifestVersion;
		}

		/// <summary>
		/// Purges the cpipe cache.
		/// Called when app starts and runs if the app version on disk is older then the app verison.
		/// The purge will delete any english or non loc specific files and update the cache with all of the latest version from the embedded manifest.
		/// </summary>
		public static void PurgeCpipeCache()
		{
			bool doPurge = false;
			Dictionary<string, string> cache = new Dictionary<string, string>();
			PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.None);
			StreamingAssetsStorage streamingAssetsStorage = new StreamingAssetsStorage(Game.None, null);

			//if app version has increased, do purge
			AppVersion appVersion = new AppVersion(Application.version);

			if (persistentDataStorage.FileExists(appVerisonFile))
			{
				AppVersion storedVersion = new AppVersion(persistentDataStorage.LoadText(appVerisonFile));
				if (storedVersion.Major > appVersion.Major)
				{
					doPurge = true;
				}
				else if (storedVersion.Major >= appVersion.Major && storedVersion.Minor > appVersion.Minor)
				{
					doPurge = true;
				}
				else if (storedVersion.Major >= appVersion.Major && storedVersion.Minor >= appVersion.Minor && storedVersion.Revision > appVersion.Revision)
				{
					doPurge = true;
				}
			}
			else
			{
				doPurge = true;			
			}
			Log.Debug("dopurge " + doPurge, Log.LogChannel.General);

			if (doPurge == false)
			{
				return;
			}

			string oldCache = "";
			if (persistentDataStorage.FileExists(CacheName))
			{
				oldCache = persistentDataStorage.LoadText(CacheName);
				string[] tempOldCacheLines = oldCache.Split('\n');
				for (int i = 0; i < tempOldCacheLines.Length; i++)
				{
					if (!string.IsNullOrEmpty(tempOldCacheLines[i]))
					{
						string rawLine = tempOldCacheLines[i];
						string[] rawLineComponents = rawLine.Split(':');
						cache[rawLineComponents[0]] = rawLineComponents[1];
					}
				}
			}

			Log.Debug("load manifest from streaming at " + Application.streamingAssetsPath + "/" + Game.ForceVision + "/" + EmbeddedCpipeManifestFile, Log.LogChannel.General);
			//load streaming assets manifest and write cache file of embedded content
			streamingAssetsStorage.LoadStreamingAssetsText(Game.ForceVision + "/" + EmbeddedCpipeManifestFile, (error, text) =>
			{
				if (string.IsNullOrEmpty(error))
				{
					try
					{
						JSONObject manifest = new JSONObject(text);

						WriteManifestVersion(manifest["version"].ToString());

						JSONObject paths = manifest["paths"];
						string newCacheString = "";
						for (int i = 0; i < paths.keys.Count; i++)
						{
							string folder = paths.keys[i];
							string v = paths[i]["v"].ToString();

							//update english only
							bool isEnglishFile = true;
							foreach (string languageFolder in Localizer.LanguageFolders)
							{
								if (folder.Contains("GeneratedSoundBanks") && folder.Contains("/" + languageFolder + "/") && languageFolder != Localizer.LanguageFolders[0])
								{
									isEnglishFile = false;
									Log.Debug("non english file found. path = " + folder, Log.LogChannel.Download);
								}
							}

							if (isEnglishFile)
							{
								if (cache.ContainsKey(folder))
								{
									//if in cache already then update to local version and delete local file
									if (persistentDataStorage.FileExists(folder))
									{
										persistentDataStorage.DeleteFile(folder);
										Log.Debug("deleteing file in " + folder, Log.LogChannel.Download);
									}
								}
								newCacheString += folder + ":" + v + "\n";
							}
							else
							{
								if (cache.ContainsKey(folder))
								{
									newCacheString += folder + ":" + v + "\n";
								}
							}

						}
						persistentDataStorage.SaveText(CacheName, newCacheString, false);

						persistentDataStorage.SaveText(appVerisonFile, Application.version, false);
					}
					catch (System.Exception e)
					{
						Log.Exception(e);
					}
				}
				else
				{
					#if !UNITY_EDITOR
					Log.Error("error loading manifest from streamingassets. " + error);
					#endif
				}
			}, true);

		}

		public void Init()
		{
			if (IsLoading == true || isCancel == true)
			{
				Cancel();
				return;
			}

			IsLoading = true;

			persistentStore = new PersistentDataStorage(Game.None);

			cacheDirectory = persistentStore.GetPersistentDataPath("");

			cache = new Dictionary<string, double>();
			if (persistentStore.FileExists(CacheName))
			{
				string tempOldCache = persistentStore.LoadText(CacheName);
				Log.Debug("old cache text = " + tempOldCache, Log.LogChannel.Download);
				string[] tempOldCacheLines = tempOldCache.Split('\n');
				for (int i = 0; i < tempOldCacheLines.Length; i++)
				{
					if (!string.IsNullOrEmpty(tempOldCacheLines[i]))
					{
						string rawLine = tempOldCacheLines[i];
						string[] rawLineComponents = rawLine.Split(':');
						cache[rawLineComponents[0]] = double.Parse(rawLineComponents[1]);
					}
				}
			}

			downloads = new List<string>();

			cloudStorageAPI = new CloudStorageAPI(monoBehaviour);
			progress = new Progress();
			progress.StartingFileCount = 1;
			progress.FileRemainingCount = 1;
			#if RC_BUILD
			progress.Sections = 2;
			progress.CurrentSection = 1;
			UpdateProgress();
			cloudStorageAPI.GetContentList(OnContentList, "");
			#else
			progress.Sections = 3;
			progress.CurrentSection = 1;
			UpdateProgress();
			cloudStorageAPI.AuthenticateWithGAE(OnAuthResponse, (downloadPercent) =>
			{
				progress.PercentOfCurrentLoadingFileDownloaded = downloadPercent;
				UpdateProgress();
			});
			#endif
		}

		public void Cancel()
		{
			Destroy(false);
		}

		public void Destroy(bool isSuccess = true, bool cancel = true)
		{
			isCancel = cancel;
			IsLoading = false;
			if (manifest != null && manifest["version"] != null)
			{
				if (isSuccess)
				{
					WriteManifestVersion(manifest["version"].ToString());
				}
				else
				{
					AppendManifestVersion(manifest["version"].ToString());
				}
			}
			if (callback != null)
			{
				callback(isSuccess, InstanceId);
			}
			monoBehaviour = null;
			callback = null;
			cloudStorageAPI = null;
			progressCallback = null;
		}

		/// <summary>
		/// Writes the manifest version.
		/// </summary>
		/// <param name="version">Version.</param>
		public static void WriteManifestVersion(string version)
		{
			version = version.Replace("\"", "");
			version = version.TrimStart('0');
			PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.None);
			persistentDataStorage.SaveText(ManifestVersionFile, version, false);
		}

		/// <summary>
		/// Appends the manifest version.
		/// </summary>
		/// <param name="version">Version.</param>
		public static void AppendManifestVersion(string version)
		{
			PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.None);
			if (persistentDataStorage.FileExists(ManifestVersionFile))
			{
				version = version.Replace("\"", "");
				version = version.TrimStart('0');
				string existingVersion = persistentDataStorage.LoadText(ManifestVersionFile);
				if (existingVersion != version)
				{
					string[] parts = existingVersion.Split(',');
					existingVersion = parts[0];
					persistentDataStorage.SaveText(ManifestVersionFile, existingVersion + "," + version, false);
				}
			}
			else
			{
				WriteManifestVersion(version);
			}
		}

		/// <summary>
		/// Gets the manifest verison.
		/// If multiple entries appear in the string[], then each manifest file has been loaded but its contents not completely downlaoded, in the order that they were downloaded.
		/// </summary>
		/// <returns>The manifest verison.</returns>
		public static string[] GetManifestVersion()
		{
			PersistentDataStorage persistentDataStorage = new PersistentDataStorage(Game.None);
			if (persistentDataStorage.FileExists(ManifestVersionFile))
			{
				string existingVersion = persistentDataStorage.LoadText(ManifestVersionFile);
				return existingVersion.Split(',');
			}
			return new string[1]{ "1" };
		}

		/// <summary>
		/// Raises the auth response event.
		/// </summary>
		/// <param name="res">Res.</param>
		private void OnAuthResponse(Response res)
		{
			if (res == null)
			{
				Cancel();
			}

			progress.CurrentSection = 2;
			UpdateProgress();
			if (isCancel == true)
			{
				return;
			}
			Log.Debug("[DEBUG-AUTH] AppController OnAuthResponse HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.General);
			if (res.HttpStatusCode == Response.HttpStatusOK)
			{
				authData = JsonUtility.FromJson<AuthData>(res.Text);
				cloudStorageAPI.GetContentList(OnContentList, authData.access_token, (downloadPercent) =>
				{
					progress.PercentOfCurrentLoadingFileDownloaded = downloadPercent;
					UpdateProgress();
				});
			}
			else
			{
				Log.Debug("[DEBUG-AUTH] AppController OnAuthResponse failled with HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.General);
				if (res.Request.Attempts < maxAttempts)
				{
					res.Request.Retry();
				}
				else
				{
					Cancel();
				}
			}
		}

		/// <summary>
		/// Raises the download event.
		/// </summary>
		/// <param name="res">Res.</param>
		private void OnDownload(Response res)
		{
			if (isCancel == true)
			{
				return;
			}
			Log.Debug("[DEBUG-AUTH] AppController OnDownload " + res.ID + "  HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
			if (res.HttpStatusCode == Response.HttpStatusOK)
			{
				//cache += res.ID + ":" + manifest["paths"][res.ID]["v"] + "\n";
				if (cache.ContainsKey(res.ID))
				{
					cache[res.ID] = double.Parse(manifest["paths"][res.ID]["v"].ToString());
				}
				else
				{
					cache.Add(res.ID, double.Parse(manifest["paths"][res.ID]["v"].ToString()));
				}

				if (res.ID.IndexOf(".json") > -1 || res.ID.IndexOf(".txt") > -1)
				{
					persistentStore.SaveText(res.ID, res.Text, false);
				}
				else
				{
					persistentStore.SaveBytes(res.ID, res.Bytes);
				}

				string downloadedFilePath = "";
				for (int i = 0; i < downloads.Count; i++)
				{
					if (downloads[i] == res.ID)
					{
						downloadedFilePath = downloads[i];
						downloads.Remove(res.ID);
					}
				}

				StringBuilder result = new StringBuilder();
				foreach (KeyValuePair<string, double> entry in cache)
				{
					result.Append(entry.Key);
					result.Append(":");
					result.Append(entry.Value);
					result.Append("\n");
				}
				persistentStore.SaveText(CacheName, result.ToString(), false);
				result = null;

				if (downloads.Count <= 0)
				{
					Log.Debug("[DEBUG-AUTH] AppController OnDownload downloads complete!", Log.LogChannel.Download);
					Destroy();
				}
				else if (downloadedFilePath.Equals("ForceVision/languages.txt"))
				{
					Log.Debug("Downloaded Languages file, update loc and reparse donwloads!", Log.LogChannel.Download);
					Localizer.Load(true);
					ParseDownloads();
					DownloadNextFile();
				}
				else
				{
					DownloadNextFile();
				}
			}
			else
			{
				Log.Debug("[DEBUG-AUTH] AppController OnDownload failed: " + res.ID + " with HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
				if (res.Request.Attempts < maxAttempts)
				{
					res.Request.Retry();
				}
				else
				{
					Cancel();
				}
			}
		}

		/// <summary>
		/// Raises the content list event.
		/// </summary>
		/// <param name="res">Res.</param>
		private void OnContentList(Response res)
		{
			if (isCancel == true)
			{
				return;
			}
			Log.Debug("[DEBUG-AUTH] AppController OnContentList HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
			if (res.HttpStatusCode == Response.HttpStatusOK)
			{
				cloudStorageAPI.GetMappingFile(OnMappingResponse, authData.access_token, (downloadPercent) =>
				{
					progress.PercentOfCurrentLoadingFileDownloaded = downloadPercent;
					UpdateProgress();
				});
			}
			else
			{
				Log.Debug("[DEBUG-AUTH] AppController OnContentList failled with HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
				if (res.Request.Attempts < maxAttempts)
				{
					res.Request.Retry();
				}
				else
				{
					Cancel();
				}
			}
		}

		/// <summary>
		/// Raises the mapping response event.
		/// </summary>
		/// <param name="res">Res.</param>
		private void OnMappingResponse(Response res)
		{
			if (isCancel == true)
			{
				return;
			}
			Log.Debug("[DEBUG-AUTH] AppController OnMappingResponse HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
			if (res.HttpStatusCode == Response.HttpStatusOK)
			{
				cloudStorageAPI.GetManifest(OnManifestResponse, authData.access_token, new JSONObject(res.Text), (downloadPercent) =>
				{
					progress.PercentOfCurrentLoadingFileDownloaded = downloadPercent;
					UpdateProgress();
				}, null, overrideManifestVersion);
			}
			else
			{
				Log.Debug("[DEBUG-AUTH] AppController OnMappingResponse failled with HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
				if (res.Request.Attempts < maxAttempts)
				{
					res.Request.Retry();
				}
				else
				{
					Cancel();
				}
			}
		}

		/// <summary>
		/// Raises the manifest response event.
		/// </summary>
		/// <param name="res">Res.</param>
		private void OnManifestResponse(Response res)
		{
			progress.CurrentSection = progress.Sections;
			if (isCancel == true)
			{
				return;
			}
			Log.Debug("[DEBUG-AUTH] AppController OnManifestResponse HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
			if (res.HttpStatusCode == Response.HttpStatusOK)
			{
				persistentStore.SaveText(res.ID, res.Text, false);
				manifest = new JSONObject(res.Text);

				if (cache.ContainsKey(res.ID))
				{
					cache[res.ID] = double.Parse(manifest["unique"].str);
				}
				else
				{
					cache.Add(res.ID, double.Parse(manifest["unique"].str));
				}

				ParseDownloads();

				progress.FileRemainingCount = downloads.Count;
				progress.StartingFileCount = downloads.Count;

				DownloadNextFile();
			}
			else
			{
				Log.Debug("[DEBUG-AUTH] AppController OnManifestResponse failled with HTTPStatusCode: " + res.HttpStatusCode, Log.LogChannel.Download);
				if (res.Request.Attempts < maxAttempts)
				{
					res.Request.Retry();
				}
				else
				{
					Cancel();
				}
			}
		}

		private void ParseDownloads()
		{
			downloads = new List<string>();
			JSONObject paths = manifest["paths"];

			string languageFolder = Localizer.GetWWiseFolderByLocale(language);
			Log.Debug("languageFolder = " + languageFolder, Log.LogChannel.General);

			for (int i = 0; i < paths.keys.Count; i++)
			{
				bool isSkipFile = false;
				if (paths.keys[i].Contains("GeneratedSoundBanks/"))
				{
					#if UNITY_ANDROID
					if (paths.keys[i].Contains("GeneratedSoundBanks/iOS"))
					#elif UNITY_IOS
					if (paths.keys[i].Contains("GeneratedSoundBanks/Android"))
					#else  
					if (paths.keys[i].Contains("GeneratedSoundBanks/"))
					#endif
					{
						Log.Debug("skipping file. path = " + paths.keys[i], Log.LogChannel.Download);
						isSkipFile = true;
					}
					else
					{
						foreach (string folder in Localizer.LanguageFolders)
						{
							if (folder != languageFolder)
							{
								if (paths.keys[i].Contains("/" + folder + "/"))
								{
									Log.Debug("Skipping file. folder = " + folder + " " + paths.keys[i], Log.LogChannel.Download);
									isSkipFile = true;
									continue;
								}
							}
						}
					}
				}

				if (!isSkipFile)
				{
					Log.Debug("load file from path " + paths.keys[i], Log.LogChannel.Download);
					downloads.Add(paths.keys[i]);
				}
			}

			//sort downloads. sounds banks last, devices file just before soundbanks.
			string devicesString = "ForceVision/devices.json";
			int devicesIndex = downloads.IndexOf(devicesString);
			if (devicesIndex > -1)
			{
				downloads.RemoveAt(devicesIndex);
				downloads.Add(devicesString);
			}
			//push text loc to front
			for (int i = 0; i < downloads.Count; i++)
			{
				string downloadPath = downloads[i];
				if (downloadPath.Contains("Localization/"))
				{
					downloads.RemoveAt(i);
					downloads.Insert(0, downloadPath);
				}
			}
			//send vo loc to back
			for (int i = downloads.Count - 1; i > -1; i--)
			{
				string downloadPath = downloads[i];
				if (downloadPath.Contains("GeneratedSoundBanks"))
				{
					downloads.RemoveAt(i);
					downloads.Add(downloadPath);
				}
			}
		}

		private void UpdateProgress()
		{
			if (progressCallback != null)
			{
				progressCallback(progress);
			}
		}

		private void DownloadNextFile()
		{
			progress.FileRemainingCount = downloads.Count;
			UpdateProgress();
			
			if (isCancel == true)
			{
				return;
			}
			JSONObject paths = manifest["paths"];
			JSONObject fileInfo = paths[downloads[0]];

			if (cache != null && cache.ContainsKey(downloads[0]) && cache[downloads[0]] == fileInfo["v"].n) //&& File.Exists(cacheDirectory + System.IO.Path.DirectorySeparatorChar + downloads[0]))
			{
				downloads.Remove(downloads[0]);
				if (downloads.Count > 0)
				{
					DownloadNextFile();
				}
				else
				{
					string result = cache.Select(entry => entry.Key + ":" + entry.Value).Aggregate((entryA, entryB) => entryA + "\n" + entryB);

					Log.Debug("write cache = " + result, Log.LogChannel.Download);

					persistentStore.SaveText(CacheName, result, false);
					Log.Debug("[DEBUG-AUTH] AppController DownloadNextFile no more files need to be downloaded.", Log.LogChannel.Download);
					Destroy();
				}
			}
			else
			{
				if (cache == null || !cache.ContainsKey(downloads[0]) || cache[downloads[0]] != fileInfo["v"].n) //|| !File.Exists(cacheDirectory + System.IO.Path.DirectorySeparatorChar + downloads[0]))
				{
					Log.Debug("get file", Log.LogChannel.Download);
					cloudStorageAPI.GetFile(OnDownload, manifest["cdnRoot"].str, fileInfo["v"].n, downloads[0], authData.access_token, (downloadPercent) =>
					{
						progress.PercentOfCurrentLoadingFileDownloaded = downloadPercent;
						UpdateProgress();
					});
				}
				else
				{
					DownloadNextFile();
				}
			}
		}
	}
}