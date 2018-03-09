using UnityEngine;
using Disney.Vision;

namespace Disney.ForceVision
{
	public class BaseController : MonoBehaviour
	{
		protected Game Game;
		protected GazeListener GazeListener;
		protected GazeWatcher GazeWatcher;
		protected NavigationElement CurrentElement;
		protected NavigationElement LastElement;
		protected VisionSDK Sdk;
		protected MainNavigationController Controller;

		public virtual void Setup(VisionSDK sdk, GazeWatcher gazeWatcher, Game game, MainNavigationController controller)
		{
			Sdk = sdk;
			GazeWatcher = gazeWatcher;
			Game = game;
			Controller = controller;

			GazeListener = new GazeListener(new [] { typeof(NavigationElement) }, OnGazedAt, OnGazedOff);
			GazeWatcher.AddListener(GazeListener);
		}

		public virtual void Init()
		{
			gameObject.SetActive(false);
		}

		protected virtual void OnEnable()
		{
			CurrentElement = LastElement = null;

			if (Sdk != null && Sdk.Input != null)
			{
				Sdk.Input.OnButtonUp += OnButtonUp;
			}
		}

		protected virtual void OnDisable()
		{
			if (Sdk != null && Sdk.Input != null)
			{
				Sdk.Input.OnButtonUp -= OnButtonUp;
			}
		}

		protected virtual void OnDestroy()
		{
			if (GazeWatcher != null)
			{
				GazeWatcher.RemoveListener(GazeListener);
			}
		}

		protected virtual void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
			
		}

		protected virtual void OnGazedAt(object sender, GazeEventArgs eventArguments)
		{
			if (eventArguments.Hit.IsChildOf(transform))
			{
				CurrentElement = eventArguments.Hit.GetComponent<NavigationElement>();
			}
		}

		protected virtual void OnGazedOff(object sender, GazeEventArgs eventArguments)
		{
			if (eventArguments.Hit.IsChildOf(transform))
			{
				CurrentElement = null;
			}
		}

		/// <summary>
		/// Returns whether or not the button is any of saber activate, saber control, or HMD select
		/// </summary>
		protected bool ButtonIsActivateOrControl(int button) {
			return button == ButtonType.SaberActivate || button == ButtonType.SaberControl || button == ButtonType.HmdSelect;
		}
	}
}