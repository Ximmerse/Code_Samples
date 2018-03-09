using System.Collections.Generic;
using UnityEngine;
using SG.Lonestar.Inventory;
using System;

namespace Disney.ForceVision
{
	public class PowerSelectController : MonoBehaviour
	{
		public GameObject ItemPrefab;

		public Transform ItemHolder;

		private Action<InventoryItem> callback;

		public void OnItemSelected(InventoryItem item)
		{
			if (callback != null)
			{
				callback(item);
				callback = null;
			}
		}

		public void DisplayForcePowerItems(List<ForcePowerItem> allItems, List<ForcePowerItem> ownedItems, Action<InventoryItem> callback)
		{
			this.callback = callback;

			foreach (Transform child in ItemHolder) 
			{
				UnityEngine.Object.Destroy(child.gameObject);
 			}

			GameObject noneItem = Instantiate(ItemPrefab);
			noneItem.transform.SetParent(ItemHolder, false);
			noneItem.GetComponent<AbilityItem>().Setup(Localizer.Get("LightsaberDuel.Label.None"), Localizer.Get("LightsaberDuel.Description.None"), callback);

			foreach (ForcePowerItem item in allItems)
			{
				GameObject holder = Instantiate(ItemPrefab);
				holder.transform.SetParent(ItemHolder, false);

				holder.GetComponent<AbilityItem>().Setup(item, callback, !ownedItems.Contains(item));
			}
		}

		public void DisplayPassivePowerItems(List<PassiveAbilityItem> allItems, List<PassiveAbilityItem> ownedItems, PassiveAbilityItem otherSlotItem, Action<InventoryItem> callback)
		{
			this.callback = callback;

			bool assaultMode = false;
			if (MenuController.ConfigToLoad != null && MenuController.ConfigToLoad.Game == Game.Assault)
			{
				assaultMode = true;
			}

			HashSet<string> assaultPassiveAbilities = new HashSet<string> 
				{
					"Mental Discipline",
					"Riposte",
					"Force Attunement",
					"Painful Focus",
					"Second Wind",
					"Preemptive Strike",
					"Form II Expert",
					"Superb Deflection",
					"Determination",
					"Dueling Master"
				};

			foreach (Transform child in ItemHolder) 
			{
				UnityEngine.Object.Destroy(child.gameObject);
 			}

			GameObject noneItem = Instantiate(ItemPrefab);
			noneItem.transform.SetParent(ItemHolder, false);
			noneItem.GetComponent<AbilityItem>().Setup(Localizer.Get("LightsaberDuel.Label.None"), Localizer.Get("LightsaberDuel.Description.None"), callback);

			foreach (PassiveAbilityItem item in allItems)
			{
				GameObject holder = Instantiate(ItemPrefab);
				holder.transform.SetParent(ItemHolder, false);

				if (item == otherSlotItem || (assaultMode &&!assaultPassiveAbilities.Contains(item.TitleText.ToString())))
				{
					holder.transform.Find("Content/Unavailable").gameObject.SetActive(true);
					holder.GetComponent<AbilityItem>().Setup(item, callback, !ownedItems.Contains(item), true);
				}
				else
				{
					holder.GetComponent<AbilityItem>().Setup(item, callback, !ownedItems.Contains(item), false);
				}
			}
		}
	}
}