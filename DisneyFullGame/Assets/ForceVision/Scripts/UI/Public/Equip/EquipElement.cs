using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Disney.AssaultMode;
using SG.Lonestar;
using SG.Lonestar.Inventory;

namespace Disney.ForceVision
{
	public enum EquipItemType
	{
		ForcePower,
		PassivePower1,
		PassivePower2,
		DifficultyItem
	}

	public class EquipElement : BaseEquipItem
	{
		public EquipItemType PanelType;

		public GameObject OnGameObject;

		public GameObject OffGameObject;

		public GameObject LockedGameObject;

		public Text Title;

		public Image Icon;

		public GameObject StartPanel;

		public PowerSelectController PowerSelectPanel;

		public EquipController EquipController;

		private DuelAPI duelApi;
		private AssaultAPI assaultApi;
		private List<PassiveAbilityItem> allPassiveItems;
		private List<ForcePowerItem> allForceItems;
		private List<PassiveAbilityItem> ownedPassiveItems;
		private List<ForcePowerItem> ownedForceItems;
		private bool locked = false;
		private string defaultLabel;

		private void Start()
		{
			duelApi = ContainerAPI.GetDuelApi();
			assaultApi = new AssaultAPI();
			allPassiveItems = duelApi.Inventory.PassiveAbilities.GetAllItems();
			ownedPassiveItems = duelApi.Inventory.PassiveAbilities.GetOwnedItems();
			allForceItems = duelApi.Inventory.ForcePowers.GetAllItems();
			ownedForceItems = duelApi.Inventory.ForcePowers.GetOwnedItems();

			if (PanelType == EquipItemType.ForcePower)
			{
				defaultLabel = Title.text;

				List<ForcePowerItem> equippedItems = duelApi.Inventory.ForcePowers.GetEquippedItems();

				foreach (ForcePowerItem item in equippedItems)
				{
					OnItemSelected(item);
					break;
				}
			}
			else if (PanelType == EquipItemType.PassivePower1 || PanelType == EquipItemType.PassivePower2)
			{
				defaultLabel = Title.text;

				List<PassiveAbilityItem> equippedItems = duelApi.Inventory.PassiveAbilities.GetEquippedItems();

				if (equippedItems.Count >= 1 && PanelType == EquipItemType.PassivePower1)
				{
					if (MenuController.ConfigToLoad.Game == Game.Assault)
					{
						if (IsAbilityUsableInAssault(equippedItems[0]))
						{
							OnItemSelected(equippedItems[0]);
						}
					}
					else
					{
						OnItemSelected(equippedItems[0]);
					}
				}

				if (equippedItems.Count >= 2 && PanelType == EquipItemType.PassivePower2)
				{
					if (MenuController.ConfigToLoad.Game == Game.Assault)
					{
						if (IsAbilityUsableInAssault(equippedItems[1]))
						{
							OnItemSelected(equippedItems[1]);
						}
					}
					else
					{
						OnItemSelected(equippedItems[1]);
					}
				}
			}
		}

		public void Lock()
		{
			if (LockedGameObject != null)
			{
				locked = true;
				OnGameObject.SetActive(false);
				GetComponent<CanvasGroup>().alpha = 0.25f;
				LockedGameObject.SetActive(true);
				Title.text = Localizer.Get("LightsaberDuel.Prompt.Unavailable");
			}
		}

		public override void GazedAt()
		{
			if (!locked)
			{
				OnGameObject.SetActive(true);
				AudioEvent.Play("MAP_UI_Combat_FirstEncounter_Select", gameObject);
			}
		}

		public override void GazedOff()
		{
			if (!locked)
			{
				OnGameObject.SetActive(false);
			}
		}

		public override void Clicked()
		{
			if (locked || !OnGameObject.activeSelf)
			{
				return;
			}

			switch (PanelType)
			{
				case EquipItemType.ForcePower:
					StartPanel.SetActive(false);
					PowerSelectPanel.DisplayForcePowerItems(allForceItems, ownedForceItems, OnItemSelected);
					PowerSelectPanel.gameObject.SetActive(true);
					AudioEvent.Play("MAP_UI_GalaxyMap_BeginActivity", gameObject);
				break;

				case EquipItemType.PassivePower1:
				case EquipItemType.PassivePower2:
					StartPanel.SetActive(false);
					PowerSelectPanel.DisplayPassivePowerItems(allPassiveItems, ownedPassiveItems, (PassiveAbilityItem)(PanelType == EquipItemType.PassivePower1 ? EquipController.PassivePower2 : EquipController.PassivePower1), OnItemSelected);
					PowerSelectPanel.gameObject.SetActive(true);
					AudioEvent.Play("MAP_UI_GalaxyMap_BeginActivity", gameObject);
				break;

				case EquipItemType.DifficultyItem:
					duelApi.Inventory.ForcePowers.UnequipAllItems();
					duelApi.Inventory.PassiveAbilities.UnequipAllItems();

					if (EquipController.ForcePower != null)
					{
						duelApi.Inventory.ForcePowers.EquipItem(EquipController.ForcePower.ID);
					}

					if (EquipController.PassivePower1 != null)
					{
						duelApi.Inventory.PassiveAbilities.EquipItem(EquipController.PassivePower1.ID);
					}

					if (EquipController.PassivePower2 != null)
					{
						duelApi.Inventory.PassiveAbilities.EquipItem(EquipController.PassivePower2.ID);
					}
					
					AudioEvent.Play("MAP_UI_GalaxyMap_LaunchActivity", gameObject);
					duelApi.Inventory.ForcePowers.SaveToDisk();
					duelApi.Inventory.PassiveAbilities.SaveToDisk();

					EquipController.LaunchGame(assaultApi);
				break;
			}
		}

		private void OnItemSelected(InventoryItem item)
		{
			if (item != null)
			{
				Title.text = item.TitleText.GetValue();
				Icon.sprite = item.Icon;
				Icon.gameObject.SetActive(true);
			}
			else
			{
				Title.text = defaultLabel;
				Icon.gameObject.SetActive(false);
			}

			StartPanel.SetActive(true);
			PowerSelectPanel.gameObject.SetActive(false);

			switch (PanelType)
			{
				case EquipItemType.ForcePower:
					EquipController.ForcePower = item;
				break;

				case EquipItemType.PassivePower1:
					EquipController.PassivePower1 = item;
				break;

				case EquipItemType.PassivePower2:
					EquipController.PassivePower2 = item;
				break;
			}
		}

		private bool IsAbilityUsableInAssault(PassiveAbilityItem item)
		{
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

			return assaultPassiveAbilities.Contains(item.name);
		}
	}
}