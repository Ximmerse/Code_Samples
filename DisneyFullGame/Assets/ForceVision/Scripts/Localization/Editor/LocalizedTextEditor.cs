using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Disney.ForceVision.Internal
{
	[CustomEditor(typeof(LocalizedText), true)]
	public class LocalizedTextEditor : Editor
	{
		/// <summary>
		/// Called each time the inspector's GUI is refreshed.
		/// </summary>
		public override void OnInspectorGUI()
		{
			LocalizedText myTarget = (LocalizedText)target;

			EditorGUIUtility.labelWidth = 60;

			// If the Locked flag is on, just show a selected label that is grayed out.
			if (myTarget.Locked)
			{
				EditorGUILayout.BeginHorizontal();
				GUI.color = Color.grey;
				EditorGUILayout.LabelField("Token: ", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
				EditorGUILayout.SelectableLabel(myTarget.Token, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				myTarget.Token = EditorGUILayout.TextField("Token: ", myTarget.Token);
			}

			// If the token is in devon, set the Text components text to keep it updated.
			if (Localizer.Has(myTarget.Token))
			{
				myTarget.GetComponent<Text>().text = Localizer.Get(myTarget.Token);
			}
			else
			{
				EditorGUILayout.LabelField("Token is not pushed to Devon");

				if (myTarget.Token == "" && !myTarget.Locked)
				{
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Suggested:", GUILayout.Width(EditorGUIUtility.labelWidth + 10));
					EditorGUILayout.SelectableLabel(SceneManager.GetActiveScene().name + "." + myTarget.transform.parent.name + "." + myTarget.name, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
				}
			}

			myTarget.Locked = EditorGUILayout.Toggle("Locked:", myTarget.Locked);

			if (!myTarget.Locked && GUILayout.Button("Generate Token Name"))
			{
				myTarget.Token = (SceneManager.GetActiveScene().name + "." + myTarget.transform.parent.name + "." + myTarget.name).ToLower();
				EditorUtility.SetDirty(target);
			}
		}
	}
}