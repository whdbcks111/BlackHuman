using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemType : Enumeration
{
    public static ItemType Sword = new(0, nameof(Sword), "겁나멋진 검", Resources.Load<Sprite>("Sprites/Item/Sword"), 100, 1);

    public string DisplayName { get; private set; }
    public Sprite Sprite { get; private set; }
    public int Durability { get; private set; }
    public int MaxAmount { get; private set; }

    private ItemType(int id, string name, string displayName, Sprite sprite, int durability = -1, int maxAmount = 10) : base(id, name)
    {
        DisplayName = displayName;
        Sprite = sprite;
        Durability = durability;
        MaxAmount = maxAmount;
    }
}