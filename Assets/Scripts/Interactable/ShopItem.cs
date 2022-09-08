using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopItem : DroppedItem
{
    private static ShopItem s_prefab = null;
    private static Transform s_container = null;

    [SerializeField]
    private TextMeshProUGUI _costText;
    private int _cost = 0;

    protected override void Start()
    {
        base.Start();
        _costText.SetText(_cost.ToString());
    }

    public override void Interact()
    {
        if(Player.Instance.GoldAmount >= _cost) 
        {
            SoundManager.Instance.PlayOneShot("Coin", 1.5f);
            Player.Instance.GoldAmount -= _cost;
            base.Interact();
        }
    }
    public static ShopItem PlaceItem(ItemStack itemStack, int cost, Vector3 pos)
    {
        if(s_prefab == null) s_prefab = Resources.Load<ShopItem>("Interactable/ShopItem");
        if(s_container == null) s_container = Storage.Get("ShopContainer").transform;

        var shopItem = Instantiate(s_prefab, s_container);
        shopItem.transform.position = pos;
        shopItem.droppedItemStack = new(itemStack);
        shopItem._cost = cost;
        return shopItem;
    }
}
