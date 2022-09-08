using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    private static DroppedItem s_prefab = null;
    private static Transform s_container = null;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    protected ItemStack droppedItemStack;

    protected virtual void Start() 
    {
        _spriteRenderer.sprite = droppedItemStack.ItemType.Sprite;
    }

    public virtual void Interact()
    {
        Player.Instance.Inventory.AddItemStack(droppedItemStack);
        Destroy(gameObject);
    }

    public static DroppedItem DropItem(ItemStack itemStack, Vector3 pos)
    {
        if(s_prefab == null) s_prefab = Resources.Load<DroppedItem>("Interactable/DroppedItem");
        if(s_container == null) s_container = Storage.Get("ItemContainer").transform;

        var dropped = Instantiate(s_prefab, s_container);
        dropped.transform.position = pos;
        dropped.droppedItemStack = new(itemStack);
        return dropped;
    }
}
