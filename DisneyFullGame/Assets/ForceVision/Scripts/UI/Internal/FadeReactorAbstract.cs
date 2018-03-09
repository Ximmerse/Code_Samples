using UnityEngine;

namespace Disney.ForceVision
{
	/// <summary>
	/// Abstract class to be extended by things that handle fading when a pause or sync menu appears.
	/// </summary>
	public abstract class FadeReactorAbstract : MonoBehaviour
	{
		public bool MenuOpen { get; private set; }

		private bool saberSynchOpen = false;
		private bool hmdConnectOpen = false;

		private MainNavigationController navController;

		/// <summary>
		/// Setup the fader's events.
		/// </summary>
		/// <param name="controller">Controller.</param>
		public virtual void Setup(MainNavigationController controller)
		{
			navController = controller;

			controller.OnMenuShown += OnMenuShown;
			controller.OnMenuHidden += OnMenuHidden;

			controller.OnSaberSyncShown += OnSaberSyncShown;
			controller.OnSaberSyncHidden += OnSaberSyncHidden;

			controller.OnHMDConnectShown += OnHMDConnectShown;
			controller.OnHMDConnectHidden += OnHMDConnectHidden;
		}

		/// <summary>
		/// Unregister delegates on destroy.
		/// </summary>
		private void OnDestroy()
		{
			if (!navController)
			{
				return;
			}

			navController.OnMenuShown -= OnMenuShown;
			navController.OnMenuHidden -= OnMenuHidden;

			navController.OnSaberSyncShown -= OnSaberSyncShown;
			navController.OnSaberSyncHidden -= OnSaberSyncHidden;

			navController.OnHMDConnectShown -= OnHMDConnectShown;
			navController.OnHMDConnectHidden -= OnHMDConnectHidden;
		}

		/// <summary>
		/// Raises the menu shown event.
		/// </summary>
		private void OnMenuShown(object obj, System.EventArgs args)
		{
			MenuOpen = true;
			StartFading(true);
		}

		/// <summary>
		/// Raises the menu hidden event.
		/// </summary>
		private void OnMenuHidden(object obj, System.EventArgs args)
		{
			MenuOpen = false;
			FadeOutCheck();
		}

		/// <summary>
		/// Raises the saber sync shown event.
		/// </summary>
		private void OnSaberSyncShown(object obj, System.EventArgs args)
		{
			saberSynchOpen = true;
			StartFading(true);
		}

		/// <summary>
		/// Raises the saber sync hidden event.
		/// </summary>
		private void OnSaberSyncHidden(object obj, System.EventArgs args)
		{
			saberSynchOpen = false;
			FadeOutCheck();
		}

		/// <summary>
		/// Raises the HMD connect shown event.
		/// </summary>
		private void OnHMDConnectShown(object obj, System.EventArgs args)
		{
			hmdConnectOpen = true;
			StartFading(true);
		}

		/// <summary>
		/// Raises the HMD connect hidden event.
		/// </summary>
		private void OnHMDConnectHidden(object obj, System.EventArgs args)
		{
			hmdConnectOpen = false;
			FadeOutCheck();
		}

		/// <summary>
		/// Check to see if all panels are closed, and start fading out if we should fade.
		/// </summary>
		private void FadeOutCheck()
		{
			if (MenuOpen || saberSynchOpen || hmdConnectOpen)
			{
				return;
			}

			StartFading(false);
		}

		/// <summary>
		/// Start fading in / out.
		/// </summary>
		/// <param name="toBlack">If set to <c>true</c> fade to black. Otherwise fade from black.</param>
		protected abstract void StartFading(bool toBlack);
	}
}