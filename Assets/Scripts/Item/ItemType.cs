using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemType : Enumeration
{
    public static ItemType Sword = new(0, nameof(Sword), "겁나멋진 검", Resources.Load<Sprite>("Sprites/Item/Sword"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    if(Player.Instance.IsAttackEnded()) 
                    {
                        Player.Instance.Sweep(new Color(1, 1, 1, .7f));
                        Player.Instance.AttackNearby(3.6f);
                    }
                }
                return false;
            },
            ItemStack => {
                var angle = -Time.time * 360 / 7;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Sword"), angle, 45 + angle);
            }
    );

    public string DisplayName { get; private set; }
    public Sprite Sprite { get; private set; }
    public int Durability { get; private set; }
    public int MaxAmount { get; private set; }
    public ItemUseAction OnUse { get; private set; }
    public Action<ItemStack> OnUpdate { get; private set; }
    public Action<ItemStack> OnUpdateInHand { get; private set; }

    private ItemType(int id, string name, string displayName, Sprite sprite,
            int durability = -1, int maxAmount = 10, 
            ItemUseAction onUse = null, 
            Action<ItemStack> onUpdateInHand = null, 
            Action<ItemStack> onUpdate = null) 
        : base(id, name)
    {
        DisplayName = displayName;
        Sprite = sprite;
        Durability = durability;
        MaxAmount = maxAmount;
        OnUse = onUse;
        OnUpdate = onUpdate;
        OnUpdateInHand = onUpdateInHand;
    }

    public delegate bool ItemUseAction(int mouseBtn, ItemStack itemStack);

}