using UnityEngine;
using UnityEngine.SceneManagement;

namespace Disney.ForceVision
{
	public class ProxyReload : MonoBehaviour
	{
		public static string SceneToLoad = "Main";

		private void Start()
		{
			SceneManager.LoadScene(SceneToLoad);
		}
	}
}