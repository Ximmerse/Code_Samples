using UnityEditor;
using UnityEngine;

namespace Disney.ForceVision
{
	[CustomEditor(typeof(MeshRenderer))]
	public class MeshSortingOrder : Editor 
	{

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			MeshRenderer renderer = target as MeshRenderer;

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			int order = EditorGUILayout.IntField("Sorting Order", renderer.sortingOrder);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(renderer);
				renderer.sortingOrder = order;
			}
			EditorGUILayout.EndHorizontal();
		}

	}
}