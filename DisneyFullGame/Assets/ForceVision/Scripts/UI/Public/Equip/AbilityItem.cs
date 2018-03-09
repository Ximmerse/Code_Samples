using UnityEngine;
using UnityEngine.UI;
using SG.Lonestar.Inventory;
using System;

namespace Disney.ForceVision
{
	public class AbilityItem : BaseEquipItem
	{
		public Text Title;
		public Text Description;
		public Image Icon;
		public GameObject Tooltip;
		public Text TooltipTitle;
		public GameObject OnGameObject;
		public GameObject OffGameObject;
		public GameObject LockedGameObject;

		private InventoryItem item;
		private Action<InventoryItem> callback;
		private bool locked = true;
		private bool greyed = false;

		public void Setup(string title, string description, Action<InventoryItem> callback)
		{
			this.callback = callback;
			locked = false;

			Description.text = description;
			Title.text = TooltipTitle.text = title;
			OffGameObject.SetActive(true);
		}

		public void Setup(InventoryItem item, Action<InventoryItem> callback, bool locked, bool greyed = false)
		{
			this.item = item;
			this.callback = callback;
			this.locked = locked;
			this.greyed = greyed;

			Title.text = TooltipTitle.text = (locked) ? "" : item.TitleText.GetValue();
			LockedGameObject.SetActive(locked);
			Description.text = item.DescriptionText.GetValue();

			if (!locked)
			{
				Icon.sprite = item.Icon;
				Icon.gameObject.SetActive(true);
			}
		}

		public override void GazedAt()
		{
			if (locked || greyed)
			{
				return;
			}

			Tooltip.SetActive(true);
			OnGameObject.SetActive(true);
			Title.gameObject.SetActive(false);
		}

		public override void GazedOff()
		{
			if (locked || greyed)
			{
				return;
			}

			Tooltip.SetActive(false);
			OnGameObject.SetActive(false);
			Title.gameObject.SetActive(true);
		}

		public override void Clicked()
		{
			if (locked || greyed || !OnGameObject.activeSelf)
			{
				return;
			}

			callback(item);
		}
	}
}