using UnityEngine;
using UnityEditor;

namespace Disney.ForceVision
{
	public class SphericalPlacer : EditorWindow
	{
		// Collider to move selected objects onto
		private Collider planetCollider;

		// Distance from planet to place the selected object
		private float offset;

		[MenuItem("Tools/DI Custom/Spherical Placer")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			SphericalPlacer window = (SphericalPlacer)EditorWindow.GetWindow(typeof(SphericalPlacer));
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			planetCollider = EditorGUILayout.ObjectField("Target", planetCollider, typeof(Collider), true) as Collider;
			offset = EditorGUILayout.FloatField("Offset", offset);

			EditorGUILayout.Space();

			if (GUILayout.Button("Place"))
			{
				foreach (GameObject obj in Selection.gameObjects)
				{
					if (obj != planetCollider.gameObject)
					{
						RaycastHit hit;
						if (Physics.Raycast(obj.transform.position, planetCollider.transform.position - obj.transform.position, out hit))
						{
							if (hit.collider == planetCollider)
							{
								// Rotate object to match planet surface
								Vector3 targetNormal = hit.normal;
								obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, targetNormal);

								// Position object on planet
								obj.transform.position = hit.point + targetNormal * offset;
							}
						}
					}
				}
			}
		}
	}
}