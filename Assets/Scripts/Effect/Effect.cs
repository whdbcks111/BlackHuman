using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Effect
{
    public EffectType Type { get; private set; }
    public float Duration;
    public float MaxDuration { get; private set; }
    public int Level;
    public LivingEntity Target, Caster;
    public Dictionary<string, object> Extras = new();
    public Image CooldownPanel = null;

    public Effect(EffectType type, int level, float duration, LivingEntity target, LivingEntity caster = null)
    {
        Type = type;
        Level = level;
        Duration = MaxDuration = duration;
        Target = target;
        Caster = caster;
    }
}