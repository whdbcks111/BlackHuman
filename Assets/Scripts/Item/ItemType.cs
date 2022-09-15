using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemType : Enumeration
{
    public static readonly ItemType Sword = new(nameof(Sword), "철검", 
            "<color=yellow>[좌클릭] 공격", 
            Resources.Load<Sprite>("Sprites/Item/Sword"), new() { Durability = 1000 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.IsAttackEnded())
                    {
                        Player.Instance.Inventory.DamageItem(itemStack, 1f);
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
    public static readonly ItemType SungchansMagicBong = new(nameof(SungchansMagicBong), "성찬쓰 매직 봉", 
            "<color=yellow>[마나소모] 10\n[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/SungchansMagicBong"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 10)
                    {
                        itemStack.SetCooldown(0.5f);
                        Player.Instance.Mana -= 10;

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
    public static readonly ItemType FireBottle = new(nameof(FireBottle), "화염병", 
            "화염병이다. 충돌할 시 시전자를 포함하여 주변으로 화염 효과를 적용한다.\n"
            + "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/FireBottle"), new() { Durability = 100 }, 5,
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
    public static readonly ItemType TimeBomb = new(nameof(TimeBomb), "시한폭탄", 
            "설치하고 3초 후 시전자를 포함한 주변에 큰 피해를 입힌다.\n"
            + "<color=yellow>[좌클릭] 설치", 
            Resources.Load<Sprite>("Sprites/Item/TimeBomb"), new() { Durability = 100 }, 5,
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
    public static readonly ItemType MagicWand = new(nameof(MagicWand), "매직 완드", 
            "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/MagicWand"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 60)
                    {
                        itemStack.SetCooldown(1f);
                        Player.Instance.Mana -= 60;

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
    public static readonly ItemType Shield = new(nameof(Shield), "방패", 
            "3초동안 투사체를 튕겨내는 마법 방어막을 전개합니다.\n"
            + "<color=yellow>[우클릭] 사용", 
            Resources.Load<Sprite>("Sprites/Item/Shield"), new() { Durability = 30 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 1)
                {
                    if (Player.Instance.Mana >= 40)
                    {
                        itemStack.SetCooldown(10f);
                        Player.Instance.Inventory.DamageItem(itemStack, 1);
                        Player.Instance.Mana -= 40;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.GetFloatingItem().transform.position,
                                "MagicShield", Player.Instance);
                        p.transform.SetParent(Player.Instance.transform);
                        p.transform.localPosition = Vector2.zero;
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 1);
                        p.MoveAxis = Vector2.up;

                        SoundManager.Instance.PlayOneShot("Shield", 3.5f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var angle = -Time.time * 360 / 2;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Shield"), angle, 0, 2.4f);
            }
    );
    public static readonly ItemType Boomerang = new(nameof(Boomerang), "부메랑", 
            "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/Boomerang"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    itemStack.SetCooldown(2.0f);

                    var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                    var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2f,
                            "Boomerang", Player.Instance);
                    p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 35);
                    p.MoveAxis = axis.normalized;
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
            "Lv.2 재생 효과를 3초간 부여한다.\n"
            + "<color=yellow>[우클릭] 시용", 
            Resources.Load<Sprite>("Sprites/Item/HealingPotion"), new(), 5,
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
            "Lv.4 마나 재생 효과를 4초간 부여한다.\n"
            + "<color=yellow>[우클릭] 사용", 
            Resources.Load<Sprite>("Sprites/Item/ManaPotion"), new(), 5,
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
    public static readonly ItemType Replin = new(nameof(Replin), "레플린", 
            "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/Replin"), new() { Durability = 500 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 4)
                    {
                        itemStack.SetCooldown(0.17f);
                        Player.Instance.Inventory.DamageItem(itemStack, 1);
                        Player.Instance.Mana -= 4;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.6f,
                                "BlackPiece", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 7);
                        p.MoveAxis = axis.normalized;
                        p.RotateSpeed = UnityEngine.Random.Range(0f, 20f);
                        p.RotateBackwards = UnityEngine.Random.value < 0.5f;

                        SoundManager.Instance.PlayOneShot("Explosion", 0.2f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Replin"), angle, 45 + angle, 3);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType SolarLaser = new(nameof(SolarLaser), "솔라이저", 
            "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/SolarLaser"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 40)
                    {
                        itemStack.SetCooldown(0.3f);
                        Player.Instance.Mana -= 40;

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
    public static readonly ItemType Toasta = new(nameof(Toasta), "토스타", 
            "3방향으로 토스트를 발사한다. 가끔씩 탄 토스트가 나온다.\n"
            + "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/Toasta"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 20)
                    {
                        itemStack.SetCooldown(1.5f);
                        Player.Instance.Mana -= 20;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                        var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;

                        for (var i = -45; i <= 45; i += 45)
                        {
                            var burnt = UnityEngine.Random.value < 0.2;
                            var a = (angle + i) * Mathf.Deg2Rad;
                            var ballAxis = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                            var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.7f,
                                    burnt ? "BurntToast" : "Toast", Player.Instance);
                            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, burnt ? 60 : 15);
                            p.MoveAxis = ballAxis.normalized;
                        }

                        SoundManager.Instance.PlayOneShot("Explosion", 0.9f);
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position;
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Toasta"), angle, angle, 3);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType MultiBow = new(nameof(MultiBow), "다중활", 
            "연속으로 3번 발사한다.\n"
            + "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/MultiBow"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 35)
                    {
                        itemStack.SetCooldown(3.0f);
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
    public static readonly ItemType FireGun = new(nameof(FireGun), "파이어건", 
            "연속으로 2번 발사한다.\n"
            + "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/FireGun"), new() { Durability = 100 }, 1,
            (mouseBtn, itemStack) =>
            {
                if (mouseBtn == 0)
                {
                    if (Player.Instance.Mana >= 25)
                    {
                        itemStack.SetCooldown(2.0f);
                        Player.Instance.Mana -= 25;
                        itemStack.Extras["FireGunCnt"] = 2;
                    }
                }
                return false;
            },
            itemStack =>
            {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/FireGun"), angle, 45 + angle, 2.7f);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));

            },
            itemStack =>
            {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(KeyInput.MousePosition) - Player.Instance.transform.position);

                itemStack.Extras.TryAdd("FireGunCnt", 0);
                Player.Instance.Extras.TryAdd("LatestFireGunShot", -1f);

                var cnt = (int)itemStack.Extras["FireGunCnt"];
                var latProj = (float)Player.Instance.Extras["LatestFireGunShot"];
                if (cnt > 0 && (latProj == -1f || Time.time - latProj > 0.3f))
                {
                    itemStack.Extras["FireGunCnt"] = cnt - 1;
                    var p = (Fireball)Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.4f,
                            "Fireball", Player.Instance);
                    p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 35  );
                    p.MoveAxis = axis.normalized;
                    p.EffectLevel = 2;
                    Player.Instance.Extras["LatestFireGunShot"] = Time.time;

                    SoundManager.Instance.PlayOneShot("Fireball", 1.5f);
                    Camera.main.gameObject.GetComponent<CameraMove>().Shake(.1f, .1f);
                }

                if (Player.Instance.Inventory.GetItemStack(Player.Instance.Inventory.SelectedSlot) != itemStack)
                {
                    itemStack.Extras["FireGunCnt"] = 0;
                }
            }
    );
    public static readonly ItemType Manaric = new(nameof(Manaric), "마나릭", 
            "소지할 시 최대 마나가 50 증가한다.", 
            Resources.Load<Sprite>("Sprites/Item/Manaric"), new() { Durability = 100 }, 1,
            null,null,
            itemStack =>
            {
                Player.Instance.Attribute.AddModifier(new(AttributeType.MaxMana, AttributeModifier.Type.Add, 50f));
            }
    );
    public static readonly ItemType Greenic = new(nameof(Greenic), "그리닉", 
            "주변을 크게 휘두른다.\n"
            + "<color=yellow>[좌클릭] 발사", 
            Resources.Load<Sprite>("Sprites/Item/Bayonet"), new() { Durability = 200 }, 1,
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
    public string Description { get; private set; }
    public Sprite Sprite { get; private set; }
    public Data ItemData { get; private set; }
    public int MaxAmount { get; private set; }
    public ItemUseAction OnUse { get; private set; }
    public Action<ItemStack> OnUpdate { get; private set; }
    public Action<ItemStack> OnUpdateInHand { get; private set; }

    private ItemType(string name, string displayName, string description, Sprite sprite,
            Data data = null, int maxAmount = 10,
            ItemUseAction onUse = null,
            Action<ItemStack> onUpdateInHand = null,
            Action<ItemStack> onUpdate = null)
        : base(name)
    {
        Description = description;
        DisplayName = displayName;
        Sprite = sprite;
        ItemData = data == null ? new() : data;
        MaxAmount = maxAmount;
        OnUse = onUse;
        OnUpdate = onUpdate;
        OnUpdateInHand = onUpdateInHand;

        if(maxAmount > 1 && ItemData.Durability > 0f) ItemData.Durability = -1f;
    }

    public delegate bool ItemUseAction(int mouseBtn, ItemStack itemStack);

    public class Data 
    {
        public float Durability = -1;
    }
}