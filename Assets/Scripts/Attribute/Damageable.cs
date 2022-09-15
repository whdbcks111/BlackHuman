using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    [SerializeField]
    public bool Invulerable = false;
    public bool IsDisplayingDamageEffect { get; private set; } 


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

    public virtual void Damage(Attribute attribute, DamageType type = DamageType.Normal, 
            bool isCritical = false, bool displayDamage = false)
    {
        if(Invulerable) return;
        var finalDamage = GetDamage(type, attribute, Attribute) 
                * (isCritical ? Attribute.GetValue(AttributeType.CriticalDamage) / 100f + 1 : 1);
        Life -= finalDamage;
        if(displayDamage && GameManager.Instance.CanDisplayDamage())
        {
            ObjectPool.Instance.StartCoroutine(DisplayDamageCoroutine(finalDamage, isCritical));
        }
    }

    private IEnumerator DisplayDamageCoroutine(float finalDamage, bool isCritical)
    {
        var damageText = ObjectPool.Instance.GetDamageText();
        damageText.SetText(string.Format("{0:0.0}", finalDamage));
        damageText.color = isCritical ? new Color(1, .2f, .2f) : Color.white;
        damageText.rectTransform.position = transform.position + Vector3.up * 0.8f;
        float vel = 3f;
        Color col;
        // 텍스트 움직임(중력)과 페이드 아웃 
        for(var t = 0f; t < .6f; t += Time.deltaTime)
        {
            damageText.rectTransform.position += Vector3.up * vel * Time.deltaTime;
            col = damageText.color;
            col.a = Mathf.Clamp01(1 - t / 0.6f);
            damageText.color = col;
            vel -= Time.deltaTime * 15f;
            yield return null;
        }
        ObjectPool.Instance.DestroyDamageText(damageText);
    }

    public virtual void Hit(Attribute attribute, DamageType type = DamageType.Normal)
    {
        if(!Invulerable) ShowDamageEffect();
        Damage(attribute, type, Random.value <= Attribute.GetValue(AttributeType.CriticalChance) / 100f, true);
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
        IsDisplayingDamageEffect = true;
        var ratio = 1f;
        while (ratio > 0f)
        {
            colors.Enqueue(Color.Lerp(Color.white, Color.red, ratio));
            ratio -= Time.deltaTime / time;
            yield return null;
        }
        IsDisplayingDamageEffect = false;
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