using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemType : Enumeration
{
    public static readonly ItemType Sword = new(nameof(Sword), "겁나멋진 검", Resources.Load<Sprite>("Sprites/Item/Sword"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    if(Player.Instance.IsAttackEnded()) 
                    {
                        SoundManager.Instance.PlayOneShot("Sweep", 6.0f);
                        Player.Instance.Sweep(new Color(1, 1, .8f, .7f));
                        Player.Instance.SweepAttackNearby(3.4f);
                    }
                }
                return false;
            },
            ItemStack => {
                var angle = -Time.time * 360 / 7;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Sword"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, .8f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackSpeed, AttributeModifier.Type.Multiply, 1.2f));
                Player.Instance.Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, .8f));
            }
    );
    public static readonly ItemType SungchansMagicBong = new(nameof(SungchansMagicBong), "성찬쓰 매직 봉", Resources.Load<Sprite>("Sprites/Item/SungchansMagicBong"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    Player.Instance.Extras.TryAdd("latestMagicBong", -1f);
                    var latObj = Player.Instance.Extras["latestMagicBong"];
                    if(!(latObj is float)) return false;
                    var lat = (float) latObj;
                    if((lat == -1f || Time.time - lat > .5f) && Player.Instance.Mana >= 10) 
                    {
                        Player.Instance.Mana -= 10;
                        Player.Instance.Extras["latestMagicBong"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position);
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
            ItemStack => {
                var axis = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position; 
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/SungchansMagicBong"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType GTYBong = new(nameof(GTYBong), "금태용 봉", Resources.Load<Sprite>("Sprites/Item/GTYBong"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    Player.Instance.Extras.TryAdd("latestGTYBong", -1f);
                    var latObj = Player.Instance.Extras["latestGTYBong"];
                    if(!(latObj is float)) return false;
                    var lat = (float) latObj;
                    if((lat == -1f || Time.time - lat > .15f) && Player.Instance.Mana >= 5) 
                    {
                        Player.Instance.Mana -= 5;
                        Player.Instance.Extras["latestGTYBong"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position);
                        var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 3f, 
                                "Fxxk", Player.Instance);
                        p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 7);
                        p.MoveAxis = axis.normalized;

                        SoundManager.Instance.PlayOneShot("Fxxk_" + UnityEngine.Random.Range(1, 2+1), 1.5f);
                        Camera.main.gameObject.GetComponent<CameraMove>().Shake(.2f, .4f);
                    }
                }
                return false;
            },
            ItemStack => {
                var axis = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position; 
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/GTYBong"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType SolarLaser = new(nameof(SolarLaser), "솔라이저", Resources.Load<Sprite>("Sprites/Item/SolarLaser"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    Player.Instance.Extras.TryAdd("latestSolarLaser", -1f);
                    var latObj = Player.Instance.Extras["latestSolarLaser"];
                    if(!(latObj is float)) return false;
                    var lat = (float) latObj;
                    if((lat == -1f || Time.time - lat > 1.5f) && Player.Instance.Mana >= 8) 
                    {
                        Player.Instance.Mana -= 8;
                        Player.Instance.Extras["latestSolarLaser"] = Time.time;

                        var axis = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position);
                        var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;

                        for(var i = -20; i <= 20; i += 10)
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
            ItemStack => {
                var axis = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position; 
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/SolarLaser"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));
            }
    );
    public static readonly ItemType MultiBow = new(nameof(MultiBow), "다중활", Resources.Load<Sprite>("Sprites/Item/MultiBow"), 100, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    Player.Instance.Extras.TryAdd("LatestMultiBow", -1f);
                    var latObj = Player.Instance.Extras["LatestMultiBow"];
                    if(!(latObj is float)) return false;
                    var lat = (float) latObj;
                    if((lat == -1f || Time.time - lat > 4.5f) && Player.Instance.Mana >= 35) 
                    {
                        Player.Instance.Mana -= 35;
                        Player.Instance.Extras["LatestMultiBow"] = Time.time;
                        Player.Instance.Extras["MultiBowCnt"] = 3;
                    }
                }
                return false;
            },
            itemStack => {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position); 
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/MultiBow"), angle, 45 + angle);
                Player.Instance.Attribute.AddModifier(new(AttributeType.AttackDamage, AttributeModifier.Type.Add, 15));
                Player.Instance.Attribute.AddModifier(new(AttributeType.Knockback, AttributeModifier.Type.Multiply, 1.4f));

            },
            itemStack => {
                var axis = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - Player.Instance.transform.position); 
                
                Player.Instance.Extras.TryAdd("MultiBowCnt", 0);
                Player.Instance.Extras.TryAdd("LatestMultiBowShot", -1f);

                var cnt = (int)Player.Instance.Extras["MultiBowCnt"];
                var latProj = (float)Player.Instance.Extras["LatestMultiBowShot"];
                if(cnt > 0 && (latProj == -1f || Time.time - latProj > 0.3f))
                {
                    Player.Instance.Extras["MultiBowCnt"] = cnt - 1;
                    var p = Projectile.SpawnProjectile(Player.Instance.transform.position + (Vector3)axis.normalized * 2.4f, 
                            "Arrow", Player.Instance);
                    p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 35);
                    p.MoveAxis = axis.normalized;
                    Player.Instance.Extras["LatestMultiBowShot"] = Time.time;

                    SoundManager.Instance.PlayOneShot("Arrow", 1.5f);
                    Camera.main.gameObject.GetComponent<CameraMove>().Shake(.1f, .1f);
                }

                if(Player.Instance.Inventory.GetItemStack(Player.Instance.Inventory.SelectedSlot) != itemStack)
                {
                    Player.Instance.Extras["MultiBowCnt"] = 0;
                }
            }
    );
    public static readonly ItemType Bayonet = new(nameof(Bayonet), "그리닉", Resources.Load<Sprite>("Sprites/Item/Bayonet"), 200, 1, 
            (mouseBtn, itemStack) => {
                if(mouseBtn == 0) 
                {
                    if(Player.Instance.IsAttackEnded()) 
                    {
                        SoundManager.Instance.PlayOneShot("Bayonet", 6.0f);
                        Player.Instance.Sweep(new Color(0, 1, .5f, .8f));
                        Player.Instance.SweepAttackNearby(4.4f);
                    }
                }
                return false;
            },
            ItemStack => {
                var angle = -Time.time * 360 / 7;
                Player.Instance.DisplayFloatingItem(Resources.Load<Sprite>("Sprites/Item/Bayonet"), angle, 45 + angle);
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