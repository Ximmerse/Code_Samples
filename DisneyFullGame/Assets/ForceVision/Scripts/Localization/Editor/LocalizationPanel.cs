using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class LocalizationPanel : EditorWindow
	{
		#region Private Properties

		private DevonLogin login;
		private bool showTokens = false;
		private string tokenToCreate = "";
		private string tokenValueToCreate = "";

		private Dictionary<string, bool> selectedTokens = new Dictionary<string, bool>();
		private Dictionary<string, string> tokens = new Dictionary<string, string>();
		private List<string> tokenKeysSorted;

		private Vector2 scrollPosition;
		private Vector2 scrollPositionTokens;

		#endregion

		#region Public Methods

		/// <summary>
		/// Shows the Localization panel window.
		/// </summary>
		[MenuItem("Tools/DI Custom/Localization Panel")]
		public static void ShowWindow()
		{
			LocalizationPanel panel = (LocalizationPanel)EditorWindow.GetWindow(typeof(LocalizationPanel));
			panel.titleContent = new GUIContent("Localization");
			panel.Init();
		}

		#endregion

		#region Private Methods

		private void Init()
		{
			tokens.Clear();
			selectedTokens.Clear();
			if (tokenKeysSorted != null)
				tokenKeysSorted.Clear();

			if (!login && File.Exists(Application.dataPath + "/ForceVision/Logins/NewLogin.asset"))
			{
				login = AssetDatabase.LoadAssetAtPath<DevonLogin>("Assets/ForceVision/Logins/NewLogin.asset");
			}

			Localizer.LoadLocalizationVersionsMerged("en_US", tokens);
			foreach(string key in tokens.Keys)
			{
				selectedTokens.Add(key, false);
			}
			tokenKeysSorted = new List<string>(tokens.Keys);
			tokenKeysSorted.Sort();
		}

		private void OnGUI()
		{
			if (tokens.Count <= 0)
			{
				Init();
			}

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

			EditorGUILayout.Space();

			// Title
			EditorGUILayout.LabelField("Devon Settings", EditorStyles.boldLabel);

			// Login Info
			login = (DevonLogin)EditorGUILayout.ObjectField("Login Information:", login, typeof(DevonLogin), false);

			DrawLine();

			// Pull from devon
			EditorGUILayout.LabelField("Pull All Tokens", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Localization files will be saved in: /Assets/StreamingAssets/Localization/");

			string versionsList = "";
			{
				System.Text.StringBuilder vlist = new System.Text.StringBuilder();
				foreach (string version in Localizer.DevonVersions)
				{
					if (vlist.Length > 0)
						vlist.Append(", ");
					vlist.Append(version);
				}
				versionsList = vlist.ToString();
			}
			EditorGUILayout.LabelField("Versions (defined in Localizer.DevonVersions): " + versionsList);

			string localeList = "";
			{
				System.Text.StringBuilder loclist = new System.Text.StringBuilder();
				foreach (string version in Localizer.Locales)
				{
					if (loclist.Length > 0)
						loclist.Append(", ");
					loclist.Append(version);
				}
				localeList = loclist.ToString();
			}
			EditorGUILayout.LabelField("Locales (defined in '/Assets/StreamingAssets/ForceVision/" + Localizer.LanguageDefinitionsFileName + "' (specified in Localizer.LanguageDefinitionsFileName)):");
			EditorGUILayout.LabelField("    " + localeList);

			if (!Directory.Exists("Assets/StreamingAssets/Localization"))
			{
				EditorGUILayout.LabelField("Please create the folder \"Localization\" in /Assets/StreamingAssets/.");
			}
			else
			{
				// Pull Tokens
				EditorGUILayout.BeginHorizontal();
				GUI.enabled = (login != null);
				if (GUILayout.Button("Pull", GUILayout.Width(100)))
				{
					if (EditorUtility.DisplayDialog("Pull from Devon",
					                                "Are you sure you want to pull ALL tokens from Devon",
					                                "Pull",
					                                "Cancel"))
					{
						DevonAPI.PullFromDevon(Game.ForceVision.ToString(), login.OAuthHeader, (result) =>
						{
							Debug.Log("Pull Result: " + result);
							Init();
						});
					}
				}
				GUI.enabled = true;
				if (login == null)
				{
					EditorGUILayout.LabelField("Login Information required.");
				}
				EditorGUILayout.EndHorizontal();
			}

			DrawLine();

			// Add a Token
			EditorGUILayout.LabelField("Create a new token", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Version (initialized from Localizer.DevonPushVersion):");
			Localizer.DevonPushVersion = EditorGUILayout.TextField("", Localizer.DevonPushVersion);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();

			EditorGUIUtility.labelWidth = 40;
			tokenToCreate = EditorGUILayout.TextField("Token:", tokenToCreate);
			tokenValueToCreate = EditorGUILayout.TextField("Value:", tokenValueToCreate);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();

			//if (tokenToCreate != "" && !tokens.ContainsKey(tokenToCreate))
			{
				GUI.enabled = (login != null && tokenToCreate != "" && !tokens.ContainsKey(tokenToCreate) && tokenValueToCreate != "");
				if (GUILayout.Button("Push", GUILayout.Width(100)))
				{
					if (EditorUtility.DisplayDialog("Push token to Devon",
					                                "Are you sure you want to push \"" + tokenToCreate + "\" to Devon?",
					                                "Push",
					                                "Cancel"))
					{
						DevonAPI.PushToDevon(new Dictionary<string, string> { { tokenToCreate, tokenValueToCreate } }, null, Game.ForceVision.ToString(), login.OAuthHeader, (result) =>
						{
							Debug.Log("Push Result: " + result);

							if (result)
							{
								tokenToCreate = "";
								tokenValueToCreate = "";
							}
							Init();
						});
					}
				}
				GUI.enabled = true;
				if (login == null)
					EditorGUILayout.LabelField("Login Information required.");
				else if (tokenToCreate == "")
					EditorGUILayout.LabelField("Token name required.");
				else if (tokens.ContainsKey(tokenToCreate))
					EditorGUILayout.LabelField("Token already exists! Value = " + tokens[tokenToCreate]);
				else if (tokenValueToCreate == "")
					EditorGUILayout.LabelField("Value required.");
			}

			EditorGUILayout.EndHorizontal();

			DrawLine();


			// Delete Button
			EditorGUILayout.BeginHorizontal();

			bool doDelete = false;
			{
				bool hasSelected = selectedTokens.Values.Contains(true);
				GUI.enabled = (login != null && hasSelected);
				if (GUILayout.Button("Delete Selected Tokens", GUILayout.Width(150)))
				{
					doDelete = true;
				}
				GUI.enabled = true;
				if (login == null)
				{
					EditorGUILayout.LabelField("Login Information required.");
				}
				else if (!hasSelected)
					EditorGUILayout.LabelField("Select Tokens below.");
			}

			EditorGUILayout.EndHorizontal();

			DrawLine();

			// Tokens
			scrollPositionTokens = EditorGUILayout.BeginScrollView(scrollPositionTokens, false, false);
			showTokens = EditorGUILayout.Foldout(showTokens, "Tokens");
			if (showTokens)
			{
				foreach (string devonVersion in Localizer.DevonVersions)
				{
					foreach(string key in tokenKeysSorted)
					{
						if (Localizer.tokensVersions[key] == devonVersion)
						{
							selectedTokens[key] = EditorGUILayout.BeginToggleGroup(" " + Localizer.tokensVersions[key] + " " + key + " = " + tokens[key],
																						selectedTokens[key]);
							EditorGUILayout.EndToggleGroup();
						}
					}
				}
			}
			EditorGUILayout.EndScrollView();

			// Actual Delete operation
			if (doDelete && selectedTokens.Values.Contains(true))
			{
				List<string> results = new List<string>();
				string selectedTokensString = "";

				foreach (KeyValuePair<string, bool> item in selectedTokens)
				{
					if (item.Value)
					{
						results.Add(item.Key);
						selectedTokensString += item.Key + "\n";
					}
				}

				if (EditorUtility.DisplayDialog("Delete from Devon",
												"Are you sure you want to delete the following:\n" + selectedTokensString,
												"Delete",
												"Cancel"))
				{
					DevonAPI.PushToDevon(null, results, Game.ForceVision.ToString(), login.OAuthHeader, (result) =>
					{
						Debug.Log("Push Result: " + result);
						Init();
					});
				}
			}

			EditorGUILayout.EndScrollView();
		}

		private static void DrawLine()
		{
			EditorGUILayout.Space();
			GUILayout.Box("", new [] {
				GUILayout.ExpandWidth(true),
				GUILayout.Height(1)
			});
			EditorGUILayout.Space();
		}

#endregion
	}
}