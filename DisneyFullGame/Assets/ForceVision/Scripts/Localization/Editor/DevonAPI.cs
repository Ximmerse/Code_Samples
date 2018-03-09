using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Disney.ForceVision.Internal;
using UnityEngine;

namespace Disney.ForceVision
{
	public static class DevonAPI
	{
		#region Private Properties

		private const string DevonBuild = "https://rest-dot-di-devon-us-prodtool-1.appspot.com/default/v2/build";
		private const string DevonPush = "https://rest-dot-di-devon-us-prodtool-1.appspot.com/default/v2/input";
		private const string DevonPull = "https://rest-dot-di-devon-us-prodtool-1.appspot.com/default/v2/output";

		#endregion

		#region Public Methods

		/// <summary>
		/// Pulls localization files from devon.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="authHeader">Auth header.</param>
		/// <param name="callback">Callback.</param>
		public static void PullFromDevon(string project, string authHeader, Action<bool> callback)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			foreach (string lang in Localizer.Locales) { stringBuilder.Append(lang); stringBuilder.Append(", "); }
			Debug.Log("Pulling localization text from Devon: " + stringBuilder.ToString());
			Debug.Log("Please wait...");

			Dictionary<string, string> buildHeaders = new Dictionary<string, string>();

			buildHeaders.Add("Authorization", authHeader);
			buildHeaders.Add("Accept", "application/json");

			WWW buildCall = new WWW(DevonBuild + "/" + project, null, buildHeaders);

			QueueManager.Add(() => buildCall.isDone, () =>
			{
				if (buildCall.error != null && !buildCall.error.Equals(""))
				{
					Debug.LogError("== Build Failed: " + buildCall.error);
					callback(false);
					return;
				}

				int remaining = Localizer.Locales.Count;
				bool result = true;
                foreach(string lang in Localizer.Locales)
				{
					Dictionary<string, string> headers = new Dictionary<string, string>();

					headers.Add("Authorization", authHeader);
					headers.Add("Accept", "application/json");

					foreach (string devonVersion in Localizer.DevonVersions)
					{
						Debug.Log(lang + " " + devonVersion + " downloading...");
						WWW call = new WWW(DevonPull + "/" + project + "/" + devonVersion + "%2F" + lang + ".json", null, headers);

						QueueManager.Add(() => call.isDone, () =>
						{
							if (call.error == null || call.error.Equals(""))
							{
								Debug.Log(lang + " " + devonVersion + " done.");
								string bundlePath = Application.streamingAssetsPath + "/Localization/" + lang + "." + devonVersion.Replace(".", "_") + ".json";

								if (File.Exists(bundlePath))
								{
									File.SetAttributes(bundlePath, FileAttributes.Normal);
								}
								else
								{
									File.Create(bundlePath).Dispose();
									File.SetAttributes(bundlePath, FileAttributes.Normal);
								}

								File.WriteAllText(bundlePath, call.text);
							}
							else
							{
								Debug.LogError("== Pull Failed: " + call.error);
								result = false;
							}

							if (--remaining == 0)
							{
								callback(result);
							}
						});
					}
				}
			});
		}

		/// <summary>
		/// Pushs to devon.
		/// </summary>
		/// <param name = "tokensToAdd"></param>
		/// <param name = "tokensToDelete"></param>
		/// <param name="project">Project.</param>
		/// <param name="authHeader">Auth header.</param>
		/// <param name="callback">Callback.</param>
		public static void PushToDevon(Dictionary<string, string> tokensToAdd,
		                               List<string> tokensToDelete,
		                               string project,
		                               string authHeader,
		                               Action<bool> callback)
		{
			// Data to send
			DevonPayload data = new DevonPayload();
			data.Version = Localizer.DevonPushVersion;

			if (tokensToAdd == null)
			{
				tokensToAdd = new Dictionary<string, string>();
			}

			if (tokensToDelete == null)
			{
				tokensToDelete = new List<string>();
			}

			// Adding
			List<string> keys = new List<string>(tokensToAdd.Keys);
			foreach (string key in keys)
			{
				DevonToken token = new DevonToken();
				token.Token = key;
				token.Value = tokensToAdd[key];
				data.Tokens.Add(token);
			}

			// Deleted
			data.Deleted = tokensToDelete;

			if (data.Tokens.Count < 1 && data.Deleted.Count < 1)
			{
				Debug.LogWarning("Devon Warning: No tokens to push");
				callback(false);
				return;
			}

			HttpWebRequest putRequest = (HttpWebRequest)WebRequest.Create(DevonPush + "/" + project + "/" + "unity.push." + DateTime.Now.ToString("MM_dd_yyyy") + ".json");
			putRequest.ContentType = "text/json";
			putRequest.Method = "PUT";
			putRequest.Accept = "application/json";
			putRequest.Headers.Add("Authorization", authHeader);

			using (var streamWriter = new StreamWriter(putRequest.GetRequestStream()))
			{
				try
				{
					streamWriter.Write(JsonUtility.ToJson(data));
				}
				catch (Exception error)
				{
					Debug.LogError("Error writting to stream writer: " + error.Message);
					Debug.LogError("Stack: " + error.StackTrace);
				}

				try
				{
					streamWriter.Close();
				}
				catch (Exception error)
				{
					Debug.LogError("Error closing stream writer: " + error.Message);
					Debug.LogError("Stack: " + error.StackTrace);
				}
			}

			HttpWebResponse httpResponse = (HttpWebResponse)putRequest.GetResponse();

			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				if (!httpResponse.StatusCode.Equals(HttpStatusCode.Created))
				{
					Debug.LogError("== Push Failed: " + httpResponse.StatusDescription);
					callback(false);
					return;
				}

				Debug.Log("Devon Success: Added " + data.Tokens.Count + " token(s) and Deleted " + data.Deleted.Count + " token(s).");
				PullFromDevon(project, authHeader, callback);
			}
		}

		#endregion
	}
}