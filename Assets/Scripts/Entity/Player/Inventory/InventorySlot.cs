using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector]
    public int SlotId;

    public void OnPointerEnter(PointerEventData data)
    {
        Player.Instance.OnSlotHoverEnter(SlotId);
    }

    public void OnPointerExit(PointerEventData data)
    {
        Player.Instance.OnSlotHoverExit(SlotId);
    }

    public void OnPointerClick(PointerEventData data)
    {
        Player.Instance.OnSlotClick(SlotId);
    }
}
