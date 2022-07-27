using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory
{

    public const byte LineCount = 10;

    public InventoryHolder Owner { get; private set; }
    private byte _selectedSlot;
    public byte SelectedSlot { get { return _selectedSlot; } set { _selectedSlot = (byte)Mathf.Clamp(value, 0, Player.HotbarSize - 1); } }
    private readonly ItemStack[] _contents = new ItemStack[Player.HotbarSize * LineCount];


    public Inventory(InventoryHolder owner)
    {
        Owner = owner;
    }


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

    public int GetItemAmount(int slot) {
        if(_contents[slot] == null) return 0;
        else return _contents[slot].Amount;
    }

    public int AddItemStack(ItemStack itemStack, int amount = 1)
    {
        var maxAmount = itemStack.ItemType.MaxAmount;
        for (var i = 0; i < _contents.Length; i++)
        {
            if (_contents[i] == null)
            {
                if (amount > maxAmount)
                {
                    amount -= maxAmount;
                    itemStack.Amount = maxAmount;
                    SetItemStack(i, new ItemStack(itemStack));
                }
                else {
                    itemStack.Amount = amount;
                    amount = 0;
                    SetItemStack(i, new ItemStack(itemStack));
                }
            }
            else {
                var remainAmount = maxAmount - GetItemAmount(i);
                if(remainAmount > amount) {
                    SetItemAmount(i, GetItemAmount(i) + amount);
                    amount = 0;
                }
                else {
                    SetItemAmount(i, maxAmount);
                    amount -= remainAmount;
                }
            }
            if(amount == 0) break;
        }
        return amount;
    }
}