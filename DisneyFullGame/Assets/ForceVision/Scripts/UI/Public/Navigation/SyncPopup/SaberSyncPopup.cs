using Disney.Vision;
using System;

namespace Disney.ForceVision
{
	/// <summary>
	/// Saber sync popup.
	/// </summary>
	public class SaberSyncPopup : AbstractSyncPopup
	{
		/// <summary>
		/// The type of the peripheral associated with this popup
		/// </summary>
		/// <returns>The type.</returns>
		protected override Type PeripheralType()
		{
			return typeof(ControllerPeripheral);
		}

		/// <summary>
		/// Sets the state.
		/// </summary>
		/// <param name="newState">New state.</param>
		protected override void SetState(StateEnum newState)
		{
			base.SetState(newState);

			if (newState == StateEnum.Hidden)
			{
				if (Controller.OnSaberSyncHidden != null)
				{
					Controller.OnSaberSyncHidden.Invoke(this, new EventArgs());
				}
			}
			else if (newState == StateEnum.Unsynched)
			{
				if (Controller.OnSaberSyncShown != null)
				{
					Controller.OnSaberSyncShown.Invoke(this, new EventArgs());
				}
			}
		}
	}
}