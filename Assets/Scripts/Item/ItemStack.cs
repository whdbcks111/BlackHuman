using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemStack
{
    public readonly Dictionary<string, object> Extras = new();
    public ItemType ItemType;
    private int _amount;
    public int Amount { get { return _amount; } set { _amount = Mathf.Clamp(value, 1, ItemType.MaxAmount); } }
    public Data ItemData = new();
    public float Cooldown = 0f, MaxCooldown = 1f;

    public ItemStack(ItemType type, int amount=1)
    {
        ItemType = type;
        Amount = amount;
        ItemData.Durability = type.ItemData.Durability;
    }

    public ItemStack(ItemStack itemStack) 
    {
        ItemType = itemStack.ItemType;
        Amount = itemStack.Amount;

        ItemData = new(itemStack.ItemData);
    }

    public void SetCooldown(float dur)
    {
        if(dur <= 0) dur = 1f;
        MaxCooldown = Cooldown = dur;
    }

    public class Data
    {
        public float Durability = -1;

        public Data() {}

        public Data(Data data)
        {
            Durability = data.Durability;
        }
    }
}