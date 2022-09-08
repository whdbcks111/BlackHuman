using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStagePortal : MonoBehaviour, IInteractable
{
    private static NextStagePortal s_prefab = null;
    private static Transform s_container = null;

    public void Interact()
    {
        GameManager.Instance.NextStage();
    }

    public static NextStagePortal PlacePortal(ItemStack itemStack, Vector3 pos)
    {
        if(s_prefab == null) s_prefab = Resources.Load<NextStagePortal>("Interactable/NextStagePortal");
        if(s_container == null) s_container = Storage.Get("InteractableContainer").transform;

        var portal = Instantiate(s_prefab, s_container);
        portal.transform.position = pos;
        portal.droppedItemStack = new(itemStack);
        return portal;
    }
}
