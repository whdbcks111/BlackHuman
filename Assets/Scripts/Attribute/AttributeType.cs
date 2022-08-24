using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttributeType : Enumeration
{
    public static readonly AttributeType MoveSpeed = new(nameof(MoveSpeed), 5, 0);
    public static readonly AttributeType AttackSpeed = new(nameof(AttackSpeed), 1.5f, 0, 1 / 0.4f);
    public static readonly AttributeType AttackDamage = new(nameof(AttackDamage), 12, 0);
    public static readonly AttributeType MagicDamage = new(nameof(MagicDamage), 20, 0);
    public static readonly AttributeType MaxMana = new(nameof(MaxMana), 100, 1);
    public static readonly AttributeType MaxLife = new(nameof(MaxLife), 100, 1);
    public static readonly AttributeType MaxStamina = new(nameof(MaxStamina), 100, 1);
    public static readonly AttributeType Knockback = new(nameof(Knockback), 4, 0);
    public static readonly AttributeType LifeRegen = new(nameof(LifeRegen), 1f, 0);
    public static readonly AttributeType ManaRegen = new(nameof(ManaRegen), 5f, 0);
    public static readonly AttributeType StaminaRegen = new(nameof(StaminaRegen), 15f, 0);
    public static readonly AttributeType Defend = new(nameof(Defend), 0, 0);
    public static readonly AttributeType MagicResistance = new(nameof(MagicResistance), 0, 0);
    public static readonly AttributeType DefendPenerate = new(nameof(DefendPenerate), 0, 0);
    public static readonly AttributeType MagicPenerate = new(nameof(MagicPenerate), 0, 0);
    public static readonly AttributeType CriticalChance = new(nameof(CriticalChance), 5, 0);
    public static readonly AttributeType CriticalDamage = new(nameof(CriticalDamage), 50, 0);
    public static readonly AttributeType AbsoluteDamage = new(nameof(AbsoluteDamage), 0, 0);


    public float DefaultValue { get; private set; }
    public float MaxValue { get; private set; }
    public float MinValue { get; private set; }

    private AttributeType(string name, float defaultValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        : base(name)
    {
        DefaultValue = defaultValue;
        MaxValue = maxValue;
        MinValue = minValue;
        
    }
}