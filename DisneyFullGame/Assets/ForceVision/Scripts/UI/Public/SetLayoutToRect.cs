using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	// Require a Layout Element so there's somewhere to set the width and height.
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(LayoutElement))]

	/// <summary>
	/// This class sets the width and/or height of this game object's Layout Element to the width and height of a specified Rect Transform. 
	/// </summary>
	public class SetLayoutToRect : MonoBehaviour
	{
		#region Member Variables

		[Tooltip("The rect transform to use as reference")]
		public RectTransform targetRect;
		[Tooltip("Set dimensions using a layout element")]
		public bool useLayoutElement = true;
		[Tooltip("Use preferred dimensions instead of minimum")]
		public bool usePreferred = false;
		[Tooltip("Use the same width as Target Rect")]
		public bool setWidth = false;
		[Tooltip("Use the same height as Target Rect")]
		public bool setHeight = false;

		#endregion

		#region Member Variables

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start ()
		{
			// Get the Rect Transform and Layout Element attached to this game object.
			RectTransform rectTransform = gameObject.transform.GetComponent<RectTransform>();
			LayoutElement layout = gameObject.transform.GetComponent<LayoutElement>();

			float tempWidth = rectTransform.rect.width;
			float tempHeight = rectTransform.rect.height;

			// If there is a target Rect Transform and an attached Layout Element, proceed to set the width and/or height.
			if (targetRect != null)
			{
				if (useLayoutElement)
				{
					if (setWidth)
					{
						if (usePreferred)
						{
							layout.preferredWidth = targetRect.rect.width;
						}
						else
						{
							layout.minWidth = targetRect.rect.width;
						}
					}
					
					if (setHeight)
					{
						if (usePreferred)
						{
							layout.preferredHeight = targetRect.rect.height;
						}
						else
						{
							layout.minHeight = targetRect.rect.height;
						}
					}
				}
				else
				{
					if (setWidth)
					{
						tempWidth = targetRect.rect.width;
					}
					
					if (setHeight)
					{
						tempHeight = targetRect.rect.height;
					}

					rectTransform.sizeDelta = new Vector2(tempWidth, tempHeight);
				}
			}
		}

		#endregion
	}
}
