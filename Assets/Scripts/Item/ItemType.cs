using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemType : Enumeration
{
    public static readonly ItemType Sword = new(nameof(Sword), "겁나멋진 검", Resources.Load<Sprite>("Sprites/Item/Sword"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.IsAttackEnded())
                    {
                        SoundManager.Instance.PlayOneShot("Sweep", 6.0f);
                        Player.Instance.Sweep(new Color(1, 1, .8f, .7f), 2.6f);
                        Player.Instance.SweepAttackNearby(2.6f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var angle = -Time.time * 360 / 7;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Sword"), angle, 45 + angle, 2.7f);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, .8f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackSpeed, AttributeModifier.Type.Multiply, 1.2f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, .8f));
            }
    );
    public static readonly ItemType SungchansMagicBong = new(nameof(SungchansMagicBong), "성찬쓰 매직 봉", Resources.Load<Sprite>("Sprites/Item/SungchansMagicBong"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("latestMagicBong", -1f);
                    var latObj = Player.Instance.Extras["latestMagicBong"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > .5f) && Player.Instance.Mana >= 10)
                    {
                        Player.Instance.Mana -= 10;
                        Player.Instance.Extras["latestMagicBong"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 3f,
                                "BongBongBeam", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
                        p.MoveAxis = axis.normalized;

                        SoundManager.Instance.PlayOneShot("Beam", 1.5f);
                        Camera.main.gameObject.GetComponent<CameraMove>().Shake(.1f, .1f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/SungchansMagicBong"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType FireBottle = new(nameof(FireBottle), "화염병", Resources.Load<Sprite>("Sprites/Item/FireBottle"), 100, 5,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0 && Input.GetMouseButtonDown(mouseBtn))
                {
                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.3f,
                                "FireBottle", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 20);
                        p.MoveAxis = axis.normalized;

                        return true;
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/FireBottle"), angle, 45 + angle, 2.7f);
            }
    );
    public static readonly ItemType TimeBomb = new(nameof(TimeBomb), "시한폭탄", Resources.Load<Sprite>("Sprites/Item/TimeBomb"), 100, 5,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0 && Input.GetMouseButtonDown(mouseBtn))
                {
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position,
                                "TimeBomb", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 0);
                        p.Attribute.SetDefaultValue(AttributeType.MagicDamage, 73);

                        return true;
                }
                return false;
            }
    );
    public static readonly ItemType MagicWand = new(nameof(MagicWand), "매직 완드", Resources.Load<Sprite>("Sprites/Item/MagicWand"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("latestMagicWand", -1f);
                    var latObj = Player.Instance.Extras["latestMagicWand"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > 1f) && Player.Instance.Mana >= 60)
                    {
                        Player.Instance.Mana -= 60;
                        Player.Instance.Extras["latestMagicWand"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.GetFloatingItem().transform.position,
                                "Rainbow", Player.Instance);
                        p.transform.SetParent(Player.Instance.GetFloatingItem().transform);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 70);
                        p.MoveAxis = Vector2.one;

                        SoundManager.Instance.PlayOneShot("MagicWand", 3.5f);
                        Camera.main.gameObject.GetComponent<CameraMove>().Shake(.3f, .7f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/MagicWand"), angle, 45 + angle, 2.5f);
                Player.Instance.Attribute.AddModifier(new(AttributeType.CriticalChance, AttributeModifier.Type.Add, 15.4f));
            }
    );
    public static readonly ItemType Boomerang = new(nameof(Boomerang), "부메랑", Resources.Load<Sprite>("Sprites/Item/Boomerang"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("latestBMR", -1f);
                    var latObj = Player.Instance.Extras["latestBMR"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > 2f))
                    {
                        Player.Instance.Extras["latestBMR"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2f,
                                "Boomerang", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 35);
                        p.MoveAxis = axis.normalized;
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Boomerang"), angle, 45 + angle, 2);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType HealingPotion = new(nameof(HealingPotion), "회복 포션", 
            Resources.Load<Sprite>("Sprites/Item/HealingPotion"), 100, 5,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 1 && Input.GetMouseButtonDown(mouseBtn))
                {
                    Player.Instance.AddEffect(EffectType.Regeneration, 2, 3, Player.Instance);
                    SoundManager.Instance.PlayOneShot("Drink", 2.0f);
                    return true;
                }
                return false;
            }
    );
    public static readonly ItemType ManaPotion = new(nameof(ManaPotion), "마나 포션", 
            Resources.Load<Sprite>("Sprites/Item/ManaPotion"), 100, 5,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 1 && Input.GetMouseButtonDown(mouseBtn))
                {
                    Player.Instance.AddEffect(EffectType.ManaRegeneration, 4, 4, Player.Instance);
                    SoundManager.Instance.PlayOneShot("Drink", 2.0f);
                    return true;
                }
                return false;
            }
    );
    public static readonly ItemType GTYBong = new(nameof(GTYBong), "금태용 봉", Resources.Load<Sprite>("Sprites/Item/GTYBong"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("latestGTYBong", -1f);
                    var latObj = Player.Instance.Extras["latestGTYBong"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > .12f) && Player.Instance.Mana >= 2)
                    {
                        Player.Instance.Mana -= 2;
                        Player.Instance.Extras["latestGTYBong"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 3f,
                                "Fxxk", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 7);
                        p.MoveAxis = axis.normalized;

                        SoundManager.Instance.PlayOneShot("Fxxk_" + UnityEngine.Random.Range(1, 2 + 1), 1.5f);
                        Camera.main.gameObject.GetComponent<CameraMove>().Shake(.2f, .4f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/GTYBong"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType SolarLaser = new(nameof(SolarLaser), "솔라이저", Resources.Load<Sprite>("Sprites/Item/SolarLaser"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("latestSolarLaser", -1f);
                    var latObj = Player.Instance.Extras["latestSolarLaser"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > .3f) && Player.Instance.Mana >= 40)
                    {
                        Player.Instance.Mana -= 40;
                        Player.Instance.Extras["latestSolarLaser"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;

                        for (var i = -20; i <= 20; i += 8)
                        {
                            var a = (angle + i) * Mathf.Deg2Rad;
                            var ballAxis = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                            var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 3.2f,
                                    "SolarBall", Player.Instance);
                            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
                            p.MoveAxis = ballAxis.normalized;
                        }

                        SoundManager.Instance.PlayOneShot("Beam", 1.5f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/SolarLaser"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType MultiBow = new(nameof(MultiBow), "다중활", Resources.Load<Sprite>("Sprites/Item/MultiBow"), 100, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    Player.Instance.Extras.TryAdd("LatestMultiBow", -1f);
                    var latObj = Player.Instance.Extras["LatestMultiBow"];
                    if (!(latObj is float)) return false;
                    var lat = (float)latObj;
                    if ((lat == -1f || Time.time - lat > 3.5f) && Player.Instance.Mana >= 35)
                    {
                        Player.Instance.Mana -= 35;
                        Player.Instance.Extras["LatestMultiBow"] = Time.time;
                        itemStack.Extras["MultiBowCnt"] = 3;
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/MultiBow"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));

            },
            itemStack =>
            {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);

                itemStack.Extras.TryAdd("MultiBowCnt", 0);
                Player.Instance.Extras.TryAdd("LatestMultiBowShot", -1f);

                var cnt = (int)itemStack.Extras["MultiBowCnt"];
                var latProj = (float)Player.Instance.Extras["LatestMultiBowShot"];
                if (cnt > 0 && (latProj == -1f || Time.time - latProj > 0.3f))
                {
                    itemStack.Extras["MultiBowCnt"] = cnt - 1;
                    var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.4f,
                            "Arrow", Player.Instance);
                    p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 35);
                    p.MoveAxis = axis.normalized;
                    Player.Instance.Extras["LatestMultiBowShot"] = Time.time;

                    SoundManager.Instance.PlayOneShot("Arrow", 1.5f);
                    Camera.main.gameObject.GetComponent<CameraMove>().Shake(.1f, .1f);
                }

                if (Player.Instance.Inventory.GetItemStack(Player.Instance.Inventory.SelectedSlot) != itemStack)
                {
                    itemStack.Extras["MultiBowCnt"] = 0;
                }
            }
    );
    public static readonly ItemType Greenic = new(nameof(Greenic), "그리닉", Resources.Load<Sprite>("Sprites/Item/Bayonet"), 200, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.IsAttackEnded())
                    {
                        SoundManager.Instance.PlayOneShot("Bayonet", 6.0f);
                        Player.Instance.Sweep(new Color(0, 1, .5f, .8f), 3.6f);
                        Player.Instance.SweepAttackNearby(3.6f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var angle = -Time.time * 360 / 7;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Bayonet"), angle, 45 + angle, 3.6f);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 35));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Add, 4.6f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackSpeed, AttributeModifier.Type.Multiply, 0.6f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, .7f));
            }
    );

    public string DisplayName { get; private set; }
    public Sprite Sprite { get; private set; }
    public int Durability { get; private set; }
    public int MaxAmount { get; private set; }
    public ItemUseAction OnUse { get; private set; }
    public Action<ItemStack> OnUpdate { get; private set; }
    public Action<ItemStack> OnUpdateInHand { get; private set; }

    private ItemType(string name, string displayName, Sprite sprite,
            int durability = -1, int maxAmount = 10,
            ItemUseAction onUse = null,
            Action<ItemStack> onUpdateInHand = null,
            Action<ItemStack> onUpdate = null)
        : base(name)
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