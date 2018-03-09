using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Disney.Vision;

namespace Disney.ForceVision
{
	// Require a Rect Transform to set the dimensions.
	[RequireComponent(typeof(RectTransform))]

	/// <summary>
	/// This class adjusts the layout of this game object's rect transform to adhere to a specific safe area.
	/// </summary>
	public class SafeAreaLayout : MonoBehaviour
	{
		#region Variables

		[Tooltip("Use this to determine the safe area dimensions")]
		public RectTransform ReferenceRect;

		[Space(10)]

		[Tooltip("Do not set the Top padding")]
		public bool IgnoreTop = false;
		[Tooltip("Do not set the Bottom padding")]
		public bool IgnoreBottom = false;
		[Tooltip("Do not set the Left padding")]
		public bool IgnoreLeft = false;
		[Tooltip("Do not set the Right padding")]
		public bool IgnoreRight = false;

		[Space(10)]

		[Tooltip("Instead of pushing in the Top, extend it out")]
		public bool ReverseTop = false;
		[Tooltip("Instead of pushing in the Bottom, extend it out")]
		public bool ReverseBottom = false;
		[Tooltip("Instead of pushing in the Left, extend it out")]
		public bool ReverseLeft = false;
		[Tooltip("Instead of pushing in the Right, extend it out")]
		public bool ReverseRight = false;

		// Theses values represent how far from the screen's edge the rect transform should be as a percentage
		private const float iPhoneXPercentTop = 0f;
		private const float iPhoneXPercentBottom = 0.056f;
		private const float iPhoneXPercentLeft = 0.05418719f;
		private const float iPhoneXPercentRight = 0.05418719f;

		private RectTransform rectTransform;
		internal VisionSDK Sdk;

		#endregion

		#region MonoBehaviour

		void Start()
		{
			rectTransform = gameObject.transform.GetComponent<RectTransform>();

			Sdk = GameObject.FindObjectOfType<VisionSDK>();

			CreateSafeArea();
		}
			
		#endregion

		#region Class Methods

		/// <summary>
		/// Creates the safe area based on the device the app is running on
		/// </summary>
		private void CreateSafeArea()
		{
			if (Sdk == null || Sdk.Settings == null)
			{
				Log.Debug("VisionSdk not set up yet");
				return;
			}

			Device device = Sdk.Settings.CurrentDevice;

			if (device != null)
			{
				switch (device.Name)
				{
					case "iPhone X":
						SetDimensions(iPhoneXPercentTop, iPhoneXPercentBottom, iPhoneXPercentLeft, iPhoneXPercentRight);
						break;
				}
			}
		}
			
		/// <summary>
		/// Sets the appropriate dimensions for the safe area
		/// </summary>
		/// <param name="topPerc">Percentage from the Top edge of the screen.</param>
		/// <param name="bottomPerc">Percentage from the Bottom edge of the screen.</param>
		/// <param name="leftPerc">Percentage from the Left edge of the screen.</param>
		/// <param name="rightPerc">Percentage from the Right edge of the screen.</param>
		private void SetDimensions(float topPerc, float bottomPerc, float leftPerc, float rightPerc)
		{
			if (ReferenceRect == null)
			{
				Debug.LogError("Reference rect transform not assigned");
				return;
			}

			float top = topPerc * ReferenceRect.rect.height;
			float bottom = bottomPerc * ReferenceRect.rect.height;
			float left = leftPerc * ReferenceRect.rect.width;
			float right = rightPerc * ReferenceRect.rect.width;

			top = ReverseTop ? -top : top;
			bottom = ReverseBottom ? -bottom : bottom;
			left = ReverseLeft ? -left : left;
			right = ReverseRight ? -right : right;

			top = IgnoreTop ? rectTransform.offsetMax.y : -top;
			bottom = IgnoreBottom ? rectTransform.offsetMin.y : bottom;
			left = IgnoreLeft ? rectTransform.offsetMin.x : left;
			right = IgnoreRight ? rectTransform.offsetMax.x : -right;

			rectTransform.offsetMin = new Vector2(left, bottom);
			rectTransform.offsetMax = new Vector2(right, top);
		}

		#endregion
	}
}
