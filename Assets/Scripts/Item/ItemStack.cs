using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public readonly Dictionary<string, object> Extras = new();
    public ItemType ItemType;
    private int _amount, _durability;
    public int Amount { get { return _amount; } set { _amount = Mathf.Clamp(value, 1, ItemType.MaxAmount); } }
    public int Durability { get { return _durability; } set { _durability = Mathf.Min(value, ItemType.Durability); } }

    public ItemStack(ItemType type, int amount=1)
    {
        ItemType = type;
        Amount = amount;
        Durability = type.Durability;
    }

    public ItemStack(ItemStack itemStack) 
    {
        ItemType = itemStack.ItemType;
        Amount = itemStack.Amount;
        Durability = itemStack.Durability;
    }
}