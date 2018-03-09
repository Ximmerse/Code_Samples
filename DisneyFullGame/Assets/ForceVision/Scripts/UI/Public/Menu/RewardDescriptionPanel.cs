using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	/// <summary>
	/// Reward description panel. Contains specific functionality for unlock / reward panels.
	/// </summary>
	public class RewardDescriptionPanel : GameResolutionPanel
	{
		public Image Icon;
		public Text RewardNameLabel;

		/// <summary>
		/// Sets the reward info.
		/// </summary>
		/// <param name="rewardName">Reward name.</param>
		/// <param name="description">Description.</param>
		/// <param name="icon">Icon.</param>
		public void SetReward(string rewardName, string description, Sprite icon)
		{
			if (MessageLabel)
			{
				MessageLabel.text = description;
			}

			if (RewardNameLabel)
			{
				RewardNameLabel.text = rewardName;
			}

			Icon.sprite = icon;
		}
	}
}