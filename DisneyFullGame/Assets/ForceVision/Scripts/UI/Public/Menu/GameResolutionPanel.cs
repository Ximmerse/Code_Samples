using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	/// <summary>
	/// Game resolution panel behavior. Used for victory, defeat, and reward panels across the app.
	/// </summary>
	public class GameResolutionPanel : MonoBehaviour
	{
		public Text MessageLabel;
		public GameObject DismissPrompt;
		public float DismissTime = 2;

		private float dismissCountup = 0;

		/// <summary>
		/// Whether or not this panel is ready to be dismissed
		/// </summary>
		/// <value><c>true</c> if dismiss ready; otherwise, <c>false</c>.</value>
		public bool DismissReady
		{
			get
			{
				if (!DismissPrompt)
				{
					// panels without dismiss prompt dismiss themselves
					return false;
				}
				return dismissCountup > DismissTime;
			}
		}

		protected virtual void Update()
		{
			dismissCountup += Time.deltaTime;
			if (dismissCountup > DismissTime)
			{
				if (DismissPrompt)
				{
					DismissPrompt.gameObject.SetActive(true);
				}
				else
				{
					Show(false);
				}
			}
		}

		/// <summary>
		/// Show / hide the panel.
		/// </summary>
		/// <param name="b">If set to <c>true</c>, show panel. Otherwise hide panel.</param>
		public virtual void Show(bool b)
		{
			if (gameObject.activeSelf == b)
			{
				return;
			}

			gameObject.SetActive(b);
			dismissCountup = 0;
			if (DismissPrompt)
			{
				DismissPrompt.gameObject.SetActive(false);
			}
		}
	}
}