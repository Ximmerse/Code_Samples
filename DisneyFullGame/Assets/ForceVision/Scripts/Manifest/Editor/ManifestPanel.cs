using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Disney.ForceVision.Internal
{
	public class ManifestPanel : EditorWindow
	{
		#region Private Properties

		private GameManifest manifest;
		private GUIStyle green;
		private GUIStyle red;
		private GUIStyle yellow;
		private bool inited = false;
		private Vector2 scrollPosition = Vector2.zero;

		#endregion

		#region Public Methods

		/// <summary>
		/// Shows the Localization panel window.
		/// </summary>
		[MenuItem("Tools/DI Custom/Manifest Panel")]
		public static void ShowWindow()
		{
			ManifestPanel panel = (ManifestPanel)EditorWindow.GetWindow(typeof(ManifestPanel));
			panel.titleContent = new GUIContent("Manifest");
			panel.Init();
		}

		#endregion

		#region Private Method

		private void Init()
		{
			green = new GUIStyle(EditorStyles.label);
			green.normal.textColor = Color.green;

			red = new GUIStyle(EditorStyles.label);
			red.normal.textColor = Color.red;

			yellow = new GUIStyle(EditorStyles.label);
			yellow.normal.textColor = Color.yellow;

			inited = true;
		}

		private void OnGUI()
		{
			if (!inited)
			{
				Init();
			}

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

			EditorGUIUtility.labelWidth = 75;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Manifest Editor", EditorStyles.boldLabel);

			manifest = (GameManifest)EditorGUILayout.ObjectField("Manifest:", manifest, typeof(GameManifest), false);

			DrawLine();

			if (manifest == null)
			{
				EditorGUILayout.EndScrollView();
				return;
			}

			// --------------------------------------------------------
			// Scenes
			// --------------------------------------------------------

			EditorGUILayout.LabelField("List of scenes that are not in the Player Settings that should be based on this Manifest",
			                           EditorStyles.boldLabel);

			foreach (SceneAsset scene in manifest.Scenes)
			{
				string scenePath = AssetDatabase.GetAssetPath(scene);

				EditorBuildSettingsScene found = EditorBuildSettings.scenes.FirstOrDefault(item => item.path == scenePath);

				if (found == null)
				{
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField("Missing:", (scene == null) ? "[Missing]" : scenePath, red);

					if (scene != null && GUILayout.Button("Add", GUILayout.Width(100)))
					{
						List<EditorBuildSettingsScene> editorBuildSettingsScenes = EditorBuildSettings.scenes.ToList<EditorBuildSettingsScene>();
						editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
						EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
					}

					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Found:", scenePath, green);
					if (GUILayout.Button("Select", GUILayout.Width(100)))
					{
						Selection.activeObject = scene;
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			if (manifest.Scenes.Length < 1)
			{
				EditorGUILayout.LabelField("No scene files!");
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("List of scenes that are in the Player Settings in the Game folder but not in the Manifest",
			                           EditorStyles.boldLabel);

			// Scences that are in the Game Root Folder, that are not in the Player Settings.
			bool foundOne = false;

			foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
			{
				if (editorScene.path.StartsWith(AssetDatabase.GetAssetPath(manifest.GameRootFolder)))
				{
					SceneAsset found = manifest.Scenes.FirstOrDefault(item => AssetDatabase.GetAssetPath(item) == editorScene.path);

					if (!found)
					{
						foundOne = true;

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Missing:", editorScene.path, red);
						if (GUILayout.Button("Remove", GUILayout.Width(100)))
						{
							List<EditorBuildSettingsScene> editorBuildSettingsScenes = EditorBuildSettings.scenes.ToList<EditorBuildSettingsScene>();
							EditorBuildSettingsScene remove = editorBuildSettingsScenes.FirstOrDefault(item => item.path == editorScene.path);
							editorBuildSettingsScenes.Remove(remove);
							EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}

			if (!foundOne)
			{
				EditorGUILayout.LabelField("None found!");
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// --------------------------------------------------------
			// Shaders
			// --------------------------------------------------------

			EditorGUILayout.LabelField("List of shaders, there isn't a way to check in code if they are preloaded or not :(",
			                           EditorStyles.boldLabel);

			foreach (Shader shader in manifest.Shaders)
			{
				string shaderPath = AssetDatabase.GetAssetPath(shader);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Shader: ", (shader == null) ? "[Missing]" : shaderPath, yellow);

				if (shader != null && GUILayout.Button("Select", GUILayout.Width(100)))
				{
					Selection.activeObject = shader;
				}

				EditorGUILayout.EndHorizontal();
			}

			if (manifest.Shaders.Length < 1)
			{
				EditorGUILayout.LabelField("No shader files!");
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