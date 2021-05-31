﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ConsumeItem : MonoBehaviour, IPointerDownHandler
{
    public Item item;
    private static Tooltip tooltip;
    public ItemType[] itemTypeOfSlot;
    public static EquipmentSystem eS;
    public GameObject duplication;
    public static GameObject mainInventory;

    float timeToCraftFinalItem;

    float startTimer;
    float endTimer;
    bool showTimer=false;

    static Image timerImage;
    public GameObject timer;

    bool makeFinalItem;
    void Start()
    {
        timeToCraftFinalItem = 0;
        item = GetComponent<ItemOnObject>().item;
        if (GameObject.FindGameObjectWithTag("Tooltip") != null)
            tooltip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
        if (GameObject.FindGameObjectWithTag("EquipmentSystem") != null)
            eS = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().characterSystem.GetComponent<EquipmentSystem>();

        if (GameObject.FindGameObjectWithTag("MainInventory") != null)
            mainInventory = GameObject.FindGameObjectWithTag("MainInventory");
        if (GameObject.FindGameObjectWithTag("Timer") != null)
        {
            timerImage = GameObject.FindGameObjectWithTag("Timer").GetComponent<Image>();
            timer = GameObject.FindGameObjectWithTag("Timer");
            timer.SetActive(false);
        }
    }
    void Update()
    {
        CraftSystem cS = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().craftSystem.GetComponent<CraftSystem>();
        if (showTimer)
        {
            if (timerImage != null)
            {
                timer.SetActive(true);
                float fillAmount = (Time.time - startTimer) / timeToCraftFinalItem;
                timerImage.fillAmount = fillAmount;
            }
        }
        else
        {
            if (timerImage != null)
            {
                timer.SetActive(false);
            }
        }
        if (makeFinalItem)
        {
            cS.getText().text = "남은 제작시간: " + (int)(timeToCraftFinalItem - (Time.time - startTimer) + 1) + "초";
            if (timerImage != null)
            {
                timer.SetActive(true);
                float fillAmount = (Time.time - startTimer) / timeToCraftFinalItem;
                timerImage.fillAmount = fillAmount;
            }
        }
        else
        {
            if (timerImage != null)
            {
                timer.SetActive(false);
            }
        }
    }
    IEnumerator CraftFinalItemWithTimer(int n)
    {
        if (makeFinalItem)
        {
            startTimer = Time.time;
            showTimer = true;
            yield return new WaitForSeconds(timeToCraftFinalItem);
            if (makeFinalItem)
            {
                Inventory inventory = transform.parent.parent.GetComponent<Inventory>();
                CraftSystem cS = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().craftSystem.GetComponent<CraftSystem>();
                if (n == cS.getN())
                {
                    GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().addItemToInventory(item.itemID, item.itemValue);
                    GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().stackableSettings();
                    if (item.itemID == 30)
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().addItemToInventory(item.itemID + 1, item.itemValue);
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().stackableSettings();
                    }
                    else if (item.itemID == 32)
                    {
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().addItemToInventory(item.itemID + 2, item.itemValue);
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().stackableSettings();
                    }
                    if (tooltip != null)
                        tooltip.deactivateTooltip();
                    cS.destroyItems();
                    cS.nonDestroy();
                    inventory.craftcloseInventory();
                    if(GameObject.FindGameObjectWithTag("MainInventory")!=null)
                        GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().updateItemList();
                    showTimer = false;
                    makeFinalItem = false;
                }
            }
        }
        else
        {
            yield return null;
            if (tooltip != null)
                tooltip.deactivateTooltip();
        }


    }
    public void makeFinalItemfalse()
    {
        makeFinalItem = false;
    }
    public void OnPointerDown(PointerEventData data)
    {
        
            if (this.gameObject.transform.parent.parent.parent.GetComponent<EquipmentSystem>() == null)
            {
                bool gearable = false;
                Inventory inventory = transform.parent.parent.parent.GetComponent<Inventory>();

                if (eS != null)
                    itemTypeOfSlot = eS.itemTypeOfSlots;

                if (data.button == PointerEventData.InputButton.Right)
                {
                    //item from craft system to inventory
                    if (transform.parent.GetComponent<CraftResultSlot>() != null)
                    {
                        CraftSystem cS = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().craftSystem.GetComponent<CraftSystem>();
                        
                        if (cS.checkfinalitem())
                        {
                            int now = cS.getN();
                            timeToCraftFinalItem = cS.gettimeToCraft();
                            makeFinalItem = true;
                            showTimer = true;
                            StartCoroutine(CraftFinalItemWithTimer(now));
                            gearable = true;
                        }
                        return;
                    }
                    else
                    {
                        bool stop = false;
                        if (eS != null)
                        {
                            for (int i = 0; i < eS.slotsInTotal; i++)
                            {
                                if (itemTypeOfSlot[i].Equals(item.itemType))
                                {
                                    if (eS.transform.GetChild(1).GetChild(i).childCount == 0)
                                    {
                                        stop = true;
                                        if (eS.transform.GetChild(1).GetChild(i).parent.parent.GetComponent<EquipmentSystem>() != null && this.gameObject.transform.parent.parent.parent.GetComponent<EquipmentSystem>() != null) { }
                                        else
                                            inventory.EquiptItem(item);

                                        this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                        this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                        eS.gameObject.GetComponent<Inventory>().updateItemList();
                                        inventory.updateItemList();
                                        gearable = true;
                                        if (duplication != null)
                                            Destroy(duplication.gameObject);
                                        break;
                                    }
                                }
                            }


                            if (!stop)
                            {
                                for (int i = 0; i < eS.slotsInTotal; i++)
                                {
                                    if (itemTypeOfSlot[i].Equals(item.itemType))
                                    {
                                        if (eS.transform.GetChild(1).GetChild(i).childCount != 0)
                                        {
                                            GameObject otherItemFromCharacterSystem = eS.transform.GetChild(1).GetChild(i).GetChild(0).gameObject;
                                            Item otherSlotItem = otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item;
                                            if (item.itemType == ItemType.UFPS_Weapon)
                                            {
                                                inventory.UnEquipItem1(otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item);
                                                inventory.EquiptItem(item);
                                            }
                                            else
                                            {
                                                inventory.EquiptItem(item);
                                                if (item.itemType != ItemType.Backpack)
                                                    inventory.UnEquipItem1(otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item);
                                            }
                                            if (this == null)
                                            {
                                                GameObject dropItem = (GameObject)Instantiate(otherSlotItem.itemModel);
                                                dropItem.AddComponent<PickUpItem>();
                                                dropItem.GetComponent<PickUpItem>().item = otherSlotItem;
                                                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
                                                inventory.OnUpdateItemList();
                                            }
                                            else
                                            {
                                                otherItemFromCharacterSystem.transform.SetParent(this.transform.parent);
                                                otherItemFromCharacterSystem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                                if (this.gameObject.transform.parent.parent.parent.GetComponent<Hotbar>() != null)
                                                    createDuplication(otherItemFromCharacterSystem);

                                                this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                                this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                            }

                                            gearable = true;
                                            if (duplication != null)
                                                Destroy(duplication.gameObject);
                                            eS.gameObject.GetComponent<Inventory>().updateItemList();
                                            inventory.OnUpdateItemList();
                                            break;
                                        }
                                    }
                                }
                            }

                        }

                    }
                    if (!gearable && item.itemType != ItemType.UFPS_Ammo && item.itemType != ItemType.UFPS_Grenade&& (int)item.itemType != 3)
                    {
                        Item itemFromDup = null;
                        if (duplication != null)
                            itemFromDup = duplication.GetComponent<ItemOnObject>().item;
                        inventory = transform.parent.parent.GetComponent<Inventory>();
                        inventory.ConsumeItem(item);

                        item.itemValue--;
                        if (itemFromDup != null)
                        {
                            duplication.GetComponent<ItemOnObject>().item.itemValue--;
                            if (itemFromDup.itemValue <= 0)
                            {
                                if (tooltip != null)
                                    tooltip.deactivateTooltip();
                                inventory.deleteItemFromInventory(item);
                                inventory.closeInventory();
                            }
                        }
                        if (item.itemValue <= 0)
                        {
                            if (tooltip != null)
                                tooltip.deactivateTooltip();
                            inventory.deleteItemFromInventory(item);
                            inventory.closeInventory();
                        }

                    }

                }


            
        }
        
    }    

    public void consumeIt()
    {
        Inventory inventory = transform.parent.parent.parent.GetComponent<Inventory>();

        bool gearable = false;

        if (GameObject.FindGameObjectWithTag("EquipmentSystem") != null)
            eS = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().characterSystem.GetComponent<EquipmentSystem>();

        if (eS != null)
            itemTypeOfSlot = eS.itemTypeOfSlots;

        Item itemFromDup = null;
        if (duplication != null)
            itemFromDup = duplication.GetComponent<ItemOnObject>().item;       

        bool stop = false;
        if (eS != null)
        {
            
            for (int i = 0; i < eS.slotsInTotal; i++)
            {
                if (itemTypeOfSlot[i].Equals(item.itemType))
                {
                    if (eS.transform.GetChild(1).GetChild(i).childCount == 0)
                    {
                        stop = true;
                        this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                        this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                        eS.gameObject.GetComponent<Inventory>().updateItemList();
                        inventory.updateItemList();
                        inventory.EquiptItem(item);
                        gearable = true;
                        if (duplication != null)
                            Destroy(duplication.gameObject);
                        break;
                    }
                }
            }

            if (!stop)
            {
                for (int i = 0; i < eS.slotsInTotal; i++)
                {
                    if (itemTypeOfSlot[i].Equals(item.itemType))
                    {
                        if (eS.transform.GetChild(1).GetChild(i).childCount != 0)
                        {
                            GameObject otherItemFromCharacterSystem = eS.transform.GetChild(1).GetChild(i).GetChild(0).gameObject;
                            Item otherSlotItem = otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item;
                            if (item.itemType == ItemType.UFPS_Weapon)
                            {
                                inventory.UnEquipItem1(otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item);
                                inventory.EquiptItem(item);
                            }
                            else
                            {
                                inventory.EquiptItem(item);
                                if (item.itemType != ItemType.Backpack)
                                    inventory.UnEquipItem1(otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item);
                            }
                            if (this == null)
                            {
                                GameObject dropItem = (GameObject)Instantiate(otherSlotItem.itemModel);
                                dropItem.AddComponent<PickUpItem>();
                                dropItem.GetComponent<PickUpItem>().item = otherSlotItem;
                                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
                                inventory.OnUpdateItemList();
                            }
                            else
                            {
                                otherItemFromCharacterSystem.transform.SetParent(this.transform.parent);
                                otherItemFromCharacterSystem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                if (this.gameObject.transform.parent.parent.parent.GetComponent<Hotbar>() != null)
                                    createDuplication(otherItemFromCharacterSystem);

                                this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            }

                            gearable = true;
                            if (duplication != null)
                                Destroy(duplication.gameObject);
                            eS.gameObject.GetComponent<Inventory>().updateItemList();
                            inventory.OnUpdateItemList();
                            break;                           
                        }
                    }
                }
            }


        }
        if (!gearable && item.itemType != ItemType.UFPS_Ammo && item.itemType != ItemType.UFPS_Grenade)
        {

            if (duplication != null)
                itemFromDup = duplication.GetComponent<ItemOnObject>().item;

            inventory.ConsumeItem(item);


            item.itemValue--;
            if (itemFromDup != null)
            {
                duplication.GetComponent<ItemOnObject>().item.itemValue--;
                if (itemFromDup.itemValue <= 0)
                {
                    if (tooltip != null)
                        tooltip.deactivateTooltip();
                    inventory.deleteItemFromInventory(item);
                    Destroy(duplication.gameObject);

                }
            }
            if (item.itemValue <= 0)
            {
                if (tooltip != null)
                    tooltip.deactivateTooltip();
                inventory.deleteItemFromInventory(item);
                Destroy(this.gameObject); 
            }

        }        
    }

    public void createDuplication(GameObject Item)
    {
        Item item = Item.GetComponent<ItemOnObject>().item;
        GameObject dup = mainInventory.GetComponent<Inventory>().addItemToInventory(item.itemID, item.itemValue);
        Item.GetComponent<ConsumeItem>().duplication = dup;
        dup.GetComponent<ConsumeItem>().duplication = Item;
    }
}
