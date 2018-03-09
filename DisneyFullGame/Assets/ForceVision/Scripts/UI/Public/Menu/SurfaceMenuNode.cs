using UnityEngine;
using System;

namespace Disney.ForceVision
{
	public class SurfaceMenuNode : MenuNode
	{
		#region Constants

		public const string TP = "Q=";

		#endregion

		#region Public Properties 

		/// <summary>
		/// The game track to launch.
		/// </summary>
		public Game LaunchGame;

		#endregion

		#region Protected Methods

		protected override void OnFocusUpdated(bool state)
		{
			base.OnFocusUpdated(state);

			if (state && OnNodeFocused != null)
			{
				OnNodeFocused(this, new MenuNodeEventArgs(NodeType, LaunchGame));
			}
		}

		protected override void OnIdleTimeReached ()
		{
			if (OnNodeIdle != null)
			{
				OnNodeIdle(this, new MenuNodeEventArgs(NodeType, LaunchGame));
			}
		}

		protected override void OnSelectUpdated(bool state)
		{
			if (state && OnNodeSelected != null)
			{
				OnNodeSelected(this, new MenuNodeEventArgs(NodeType, LaunchGame));
			}
		}

		#endregion
	}
}