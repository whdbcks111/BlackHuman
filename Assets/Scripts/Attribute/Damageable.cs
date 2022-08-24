using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Damageable : MonoBehaviour
{
    public readonly Attribute Attribute = new();

    private float _life;
    [HideInInspector]
    public float Life { get { return _life; } set { _life = Mathf.Clamp(value, 0, Attribute.GetValue(AttributeType.MaxLife)); } }
    
    protected Queue<Color> colors = new();
    protected Color originalColor;
    
    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        originalColor = spriteRenderer.color;
        InitializeDefaults();
    }

    protected virtual void InitializeDefaults()
    {
    }

    protected virtual void Start()
    {
        Life = Attribute.GetValue(AttributeType.MaxLife);
    }

    public static float GetDamage(DamageType type, Attribute self, Attribute target)
    {
        switch(type)
        {
            case DamageType.Normal:
                return Mathf.Max(0, self.GetValue(AttributeType.AttackDamage) - 
                        Mathf.Max(target.GetValue(AttributeType.Defend) - self.GetValue(AttributeType.DefendPenerate))
                ) + self.GetValue(AttributeType.AbsoluteDamage);
            case DamageType.Magic:
                return Mathf.Max(0, self.GetValue(AttributeType.AttackDamage) - 
                        Mathf.Max(target.GetValue(AttributeType.Defend) - self.GetValue(AttributeType.DefendPenerate))
                ) + self.GetValue(AttributeType.AbsoluteDamage);
            default:
                return 0;
        }
    }

    public virtual void Damage(Attribute attribute, DamageType type = DamageType.Normal, bool isCritical = false)
    {
        Life -= GetDamage(type, attribute, Attribute) * (isCritical ? Attribute.GetValue(AttributeType.CriticalDamage) / 100f + 1 : 1);
    }

    public virtual void Hit(Attribute attribute, DamageType type = DamageType.Normal)
    {
        ShowDamageEffect();
        Damage(attribute, type, Random.value <= Attribute.GetValue(AttributeType.CriticalChance) / 100f);
        HitSound();
    }

    protected virtual void HitSound()
    {
    }

    public virtual void ShowDamageEffect()
    {
        StopCoroutine(nameof(ShowDamageEffectCoroutine));
        StartCoroutine(ShowDamageEffectCoroutine(.5f));
    }

    protected virtual IEnumerator ShowDamageEffectCoroutine(float time)
    {
        var ratio = 1f;
        while (ratio > 0f)
        {
            colors.Enqueue(Color.Lerp(Color.white, Color.red, ratio));
            ratio -= Time.deltaTime / time;
            yield return null;
        }
    }

    protected void ColorUpdate()
    {
        spriteRenderer.color = originalColor;
        while(colors.Count > 0)
        {
            spriteRenderer.color *= colors.Dequeue();
        }
    }
}

public enum DamageType 
{
    Normal, Magic
}