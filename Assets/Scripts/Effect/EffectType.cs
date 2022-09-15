using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectType : Enumeration
{
    public static readonly EffectType Fire = new(nameof(Fire), 
    effect => 
    {
        effect.Extras["latestParticle"] = Time.time;
    }, 
    effect => 
    {
        effect.Target.AddColor(new(1f, .6f, .45f));
        effect.Target.Damage(Attribute.Damage(3 * Time.deltaTime * effect.Level), DamageType.Normal, false, false);
        if(Time.time - (float)effect.Extras["latestParticle"] > 0.3f)
        {
            effect.Extras["latestParticle"] = Time.time;
            ParticleManager.Instance.SpawnParticle(ParticleType.Smoke, 
                        Color.Lerp(Color.red, Color.yellow, UnityEngine.Random.Range(0.2f, 0.7f)), 
                        effect.Target.transform, 0.1f, 0, 1, false);
        }
    },
    effect => {});
    public static readonly EffectType Poison = new(nameof(Poison), 
    effect => 
    {
        effect.Extras["latestParticle"] = Time.time;
    }, 
    effect => 
    {
        effect.Target.AddColor(new(.56f, .7f, .3f));
        effect.Target.Damage(Attribute.Damage((3 + (1 - effect.Duration / effect.MaxDuration) * 4) * Time.deltaTime * effect.Level), DamageType.Normal, false, false);
        if(Time.time - (float)effect.Extras["latestParticle"] > 0.4f)
        {
            effect.Extras["latestParticle"] = Time.time;
            ParticleManager.Instance.SpawnParticle(ParticleType.Smoke, 
                        Color.Lerp(Color.red, Color.green, UnityEngine.Random.Range(0.5f, 0.8f)), 
                        effect.Target.transform, 0.1f, 0, 2, false);
        }
    }, 
    effect => {});
    public static readonly EffectType Blindness = new(nameof(Blindness), 
    effect => 
    {
        var mask = Storage.Get("BlindMask");
        effect.Extras["Blind"] = mask;
        if(!(effect.Target is Player)) 
        {
            effect.Duration = 0f;
            return;
        }
        var latest = (float)effect.Target.Extras.GetValueOrDefault("latestBlind", -1f);
        mask.gameObject.SetActive(true);
        if(Time.time - latest > 0.1f) mask.transform.localScale = Vector2.one * 20;
    }, 
    effect => 
    {
        var mask = (GameObject) effect.Extras["Blind"];
        var size = Mathf.Clamp(4.0f - effect.Level * 0.3f, 1f, 4f);

        mask.transform.localScale = Vector2.one * ExtraMath.AddTowards(
                mask.transform.localScale.x, 
                effect.Duration < .5f ? 20f : size, 
                Time.deltaTime * (effect.Duration < .5f ? 30f : 10f)
        );

        effect.Target.Extras["latestBlind"] = Time.time;
    }, 
    effect => 
    {
        var mask = (GameObject) effect.Extras["Blind"];
        mask.gameObject.SetActive(false);
    });
    public static readonly EffectType Regeneration = new(nameof(Regeneration), 
    effect => 
    {
    }, 
    effect => 
    {
        effect.Target.Attribute.AddModifier(new(AttributeType.LifeRegen, AttributeModifier.Type.Add, 5f * effect.Level));
    }, 
    effect => {});
    public static readonly EffectType ManaRegeneration = new(nameof(ManaRegeneration), 
    effect => 
    {
    }, 
    effect => 
    {
        effect.Target.Attribute.AddModifier(new(AttributeType.ManaRegen, AttributeModifier.Type.Add, 5f * effect.Level));
    }, 
    effect => {});

    public Action<Effect> OnEffectStart { get; private set; }
    public Action<Effect> OnEffectUpdate { get; private set; }
    public Action<Effect> OnEffectFinish { get; private set; }

    private EffectType(string name, Action<Effect> effectStartAction, Action<Effect> effectAction, Action<Effect> effectFinishAction)
        : base(name)
    {
        OnEffectStart = effectStartAction;
        OnEffectUpdate = effectAction;
        OnEffectFinish = effectFinishAction;
    }
}