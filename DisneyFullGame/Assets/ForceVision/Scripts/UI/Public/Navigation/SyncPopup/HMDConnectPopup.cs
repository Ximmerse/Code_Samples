using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	public class HMDConnectPopup : AbstractSyncPopup
	{
		/// <summary>
		/// The type of the peripheral associated with this popup
		/// </summary>
		/// <returns>The type.</returns>
		protected override Type PeripheralType()
		{
			return typeof(HmdPeripheral);
		}

		/// <summary>
		/// Sets the state.
		/// </summary>
		/// <param name="newState">New state.</param>
		protected override void SetState(StateEnum newState)
		{
			base.SetState(newState);

			if (!Controller)
			{
				return;
			}

			if (newState == StateEnum.Hidden)
			{
				if (Controller.OnHMDConnectHidden != null)
				{
					Controller.OnHMDConnectHidden.Invoke(this, new EventArgs());
				}
			}
			else if (newState == StateEnum.Unsynched)
			{
				if (Controller.OnHMDConnectShown != null)
				{
					Controller.OnHMDConnectShown.Invoke(this, new EventArgs());
				}
			}
		}

		/// <summary>
		/// Forces showing / hiding the popup.
		/// Used for testing in Editor.
		/// </summary>
		/// <param name="on">If set to <c>true</c> on.</param>
		public void ForceShow(bool on)
		{
			SetState(on ? StateEnum.Unsynched : StateEnum.Hidden);
		}

		/// <summary>
		/// Checks the connection, and opens the popup if the hmd is disconnected.
		/// Specifically used in the flow of the prepare headset scene.
		/// </summary>
		public void CheckConnection()
		{
			if (sdk == null || sdk.Tracking.Hmd == null)
			{
				return;
			}

			if (!sdk.Tracking.Hmd.Connected)
			{
				SetState(StateEnum.Unsynched);
			}
		}
	}
}