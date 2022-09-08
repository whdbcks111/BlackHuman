using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory
{

    public const byte LineCount = 6;

    private byte _selectedSlot;
    public byte SelectedSlot { get { return _selectedSlot; } set { _selectedSlot = (byte)Mathf.Clamp(value, 0, Player.HotbarSize - 1); } }
    private readonly ItemStack[] _contents = new ItemStack[Player.HotbarSize * LineCount];
    public ItemStack PointerHoldItem = null;

    public ItemStack GetItemStack(int slot)
    {
        return _contents[slot];
    }

    public void SetItemStack(int slot, ItemStack itemStack)
    {
        _contents[slot] = itemStack;
    }

    public void SetItemAmount(int slot, int amount)
    {
        var itemStack = GetItemStack(slot);
        if (itemStack == null) return;
        if (amount <= 0)
        {
            SetItemStack(slot, null);
            return;
        }
        itemStack.Amount = amount;
    }

    public int GetItemAmount(int slot)
    {
        if (_contents[slot] == null) return 0;
        else return _contents[slot].Amount;
    }

    public bool HasItem(ItemType type, int amount = 1)
    {
        if(type == null) return false;
        foreach(var item in _contents)
        {
            if(item != null && item.ItemType == type) 
            {
                amount -= item.Amount;
            }
            if(amount <= 0) return true;
        }
        return false;
    }

    public int AddItemStack(ItemStack itemStack)
    {
        int amount = itemStack.Amount;
        var maxAmount = itemStack.ItemType.MaxAmount;
        for (var i = 0; i < _contents.Length; i++)
        {
            if (_contents[i] == null)
            {
                if (amount > maxAmount)
                {
                    itemStack.Amount = maxAmount;
                    amount -= maxAmount;
                    SetItemStack(i, new ItemStack(itemStack));
                }
                else
                {
                    itemStack.Amount = amount;
                    amount = 0;
                    SetItemStack(i, new ItemStack(itemStack));
                }
            }
            else if(_contents[i].ItemType == itemStack.ItemType)
            {
                var remainAmount = maxAmount - GetItemAmount(i);
                if (remainAmount > amount)
                {
                    SetItemAmount(i, GetItemAmount(i) + amount);
                    amount = 0;
                }
                else
                {
                    SetItemAmount(i, maxAmount);
                    amount -= remainAmount;
                }
            }
            if (amount == 0) break;
        }
        return amount;
    }

    public void UseItem(int slot, int mouseBtn)
    {
        var itemStack = GetItemStack(slot);
        if (itemStack == null || itemStack.ItemType.OnUse == null) 
        {
            if(mouseBtn == 0) Player.Instance.AttackNearby(1.5f, 1);
            return;
        }
        var result = itemStack.ItemType.OnUse(mouseBtn, itemStack);
        if(result) SetItemAmount(slot, GetItemAmount(slot) - 1);
    }

    public void Update() {
        foreach(var itemStack in _contents) {
            if(itemStack == null || itemStack.ItemType.OnUpdate == null) continue;
            itemStack.ItemType.OnUpdate(itemStack);
        }
        var itemInHand = GetItemStack(SelectedSlot);
        if(itemInHand != null && itemInHand.ItemType.OnUpdateInHand != null)
            itemInHand.ItemType.OnUpdateInHand(itemInHand);
    }
}