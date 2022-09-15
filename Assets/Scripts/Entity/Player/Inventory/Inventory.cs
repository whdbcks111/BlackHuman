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

    public void DamageItem(int slot, float amount)
    {
        var itemStack = GetItemStack(slot);
        // 원래부터 내구도가 음수 or 최대 개수가 1개 초과 -> 파괴 불가 아이템
        if(itemStack == null || itemStack.ItemData.Durability < 0 || itemStack.ItemType.MaxAmount > 1) return;
        
        if((itemStack.ItemData.Durability -= amount) < 0f)
        {
            SetItemStack(slot, null);
        }
    }

    public void DamageItem(ItemStack item, float amount)
    {
        DamageItem(GetSlotId(item), amount);
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

    public void GiveDefaultItem()
    {
        PointerHoldItem = null;
        for(var i = 0; i < _contents.Length; i++) _contents[i] = null;
        AddItemStack(new(ItemType.Replin), 1);
        AddItemStack(new(ItemType.Sword), 1);
        AddItemStack(new(ItemType.Shield), 1);
        AddItemStack(new(ItemType.HealingPotion), 3);
        AddItemStack(new(ItemType.ManaPotion), 2);
    }

    public int GetSlotId(ItemStack item)
    {
        var slot = -1;
        for(var i = 0; i < _contents.Length; i++) 
        {
            if(_contents[i] == item)
            {
                slot = i;
                break;
            }
        }
        return slot;
    }

    public int AddItemStack(ItemStack itemStack, int amount=1)
    {
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
        if(itemStack.Cooldown > 0f) return;
        var result = itemStack.ItemType.OnUse(mouseBtn, itemStack);
        if(result) SetItemAmount(slot, GetItemAmount(slot) - 1);
    }

    public void Update() {
        foreach(var itemStack in _contents) {
            if(itemStack != null)
            {
                if(itemStack.Cooldown > 0f) itemStack.Cooldown -= Time.deltaTime;
                if(itemStack.ItemType.OnUpdate != null) itemStack.ItemType.OnUpdate(itemStack);
            }
        }
        var itemInHand = GetItemStack(SelectedSlot);
        if(itemInHand != null && itemInHand.ItemType.OnUpdateInHand != null)
            itemInHand.ItemType.OnUpdateInHand(itemInHand);
    }
}