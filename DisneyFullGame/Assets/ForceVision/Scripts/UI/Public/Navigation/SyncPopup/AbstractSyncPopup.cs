using UnityEngine;
using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	/// <summary>
	/// Abstract sync popup class, to be inherited by saber sync and headset connection popup classes.
	/// </summary>
	public abstract class AbstractSyncPopup : MonoBehaviour
	{
		protected enum StateEnum
		{
			Hidden,
			Unsynched,
			Synched
		}

		public GameObject SynchedPanel;
		public GameObject UnsynchedPanel;

		protected VisionSDK sdk;
		protected MainNavigationController Controller;

		private StateEnum state = StateEnum.Hidden;

		/// <summary>
		/// The type of the peripheral associated with this popup
		/// </summary>
		/// <returns>The type.</returns>
		protected abstract Type PeripheralType();

		/// <summary>
		/// Setup the popup with the specified sdk and navigation controller.
		/// </summary>
		/// <param name="sdk">Sdk.</param>
		/// <param name="controller">Controller.</param>
		public void Setup(VisionSDK sdk, MainNavigationController controller)
		{
			this.sdk = sdk;

			sdk.Connections.OnPeripheralStateChange += OnConnectionChanged;
			sdk.Input.OnButtonDown += OnButtonDown;

			this.Controller = controller;

			SetState(StateEnum.Hidden);
		}

		private void OnDestroy()
		{
			if (sdk == null)
			{
				return;
			}

			sdk.Connections.OnPeripheralStateChange -= OnConnectionChanged;
			sdk.Input.OnButtonDown -= OnButtonDown;
		}

		/// <summary>
		/// Whether or not this popup is showing.
		/// </summary>
		public bool IsShowing()
		{
			return state != StateEnum.Hidden;
		}

		/// <summary>
		/// Sets the state.
		/// </summary>
		/// <param name="newState">New state.</param>
		protected virtual void SetState(StateEnum newState)
		{
			this.state = newState;

			gameObject.SetActive(newState != StateEnum.Hidden);
			SynchedPanel.SetActive(newState == StateEnum.Synched);
			UnsynchedPanel.SetActive(newState == StateEnum.Unsynched);
		}

		/// <summary>
		/// Raises the button down event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="eventArguments">Event arguments.</param>
		private void OnButtonDown(object sender, ButtonEventArgs eventArguments)
		{
			// if synch is done and any button is pressed, hide the popup
			if (state == StateEnum.Synched)
			{
				SetState(StateEnum.Hidden);
			}
		}

		/// <summary>
		/// Raises the connection changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="eventArguments">Event arguments.</param>
		private void OnConnectionChanged(object sender, PeripheralStateChangeEventArgs eventArguments)
		{
			if (eventArguments.Peripheral.GetType() != PeripheralType())
			{
				// peripheral is not the right type
				return;
			}

			if (!eventArguments.Connected)
			{
				SetState(StateEnum.Unsynched);
			}
			else if (state == StateEnum.Unsynched)
			{
				SetState(StateEnum.Synched);
			}
		}
	}
}