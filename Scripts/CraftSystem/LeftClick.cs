using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LeftClick : MonoBehaviour, IPointerDownHandler
{

    CraftResultSlot resultScript;
    CraftSystem craftSystem;
    ConsumeItem consumeItem;

    public void OnPointerDown(PointerEventData data)
    {
        if (craftSystem == null)
            craftSystem = transform.parent.GetComponent<CraftSystem>();
        if (consumeItem == null)
            consumeItem = transform.parent.GetChild(3).GetChild(0).GetComponent<ConsumeItem>();
        consumeItem.makeFinalItemfalse();
        craftSystem.backToInventory();
        craftSystem.Nplus();
        craftSystem.ListWithoutItem();


    }
}