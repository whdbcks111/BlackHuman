using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class LivingEntity : Damageable
{
    public readonly Dictionary<string, object> Extras = new();

    [SerializeField]
    protected Animator animator;
    [SerializeField]
    protected Rigidbody2D rigid;


    public static int PassingLayer = -1;
    public int OriginalLayer;

    private string _name;
    public bool IsDead { get; private set; }
    public string Name { get { return _name; } set { _name = value; } }
    public string curState { get; private set; }

    private float _mana, _stamina;
    [HideInInspector]
    public float Mana { get { return _mana; } set { _mana = Mathf.Clamp(value, 0, Attribute.GetValue(AttributeType.MaxMana)); } }
    [HideInInspector]
    public float Stamina { get { return _stamina; } set { _stamina = Mathf.Clamp(value, 0, Attribute.GetValue(AttributeType.MaxStamina)); } }
    public bool IsJumping
    {
        get 
        {
            return JumpVelocity > 0f || spriteRenderer.transform.localPosition.y > 0;
        }
        private set {}
    }

    protected Vector2 axis = Vector2.zero;
    private Vector2 _force;
    private float _jumpVelocity = 0f;
    public float JumpVelocity { get { return _jumpVelocity; } private set { _jumpVelocity = value; } }
    protected float latestAttack = -1;

    protected List<Effect> effects = new();
    private Queue<Effect> _removeEffects = new();

    protected override void Awake()
    {
        base.Awake();
        Name = GetType().ToString();
        InitializeDefaults();

        OriginalLayer = gameObject.layer;
        if(PassingLayer == -1) PassingLayer = LayerMask.NameToLayer("Passing");
    }

    protected override void Start()
    {
        base.Start();
        Mana = Attribute.GetValue(AttributeType.MaxMana);
        Stamina = Attribute.GetValue(AttributeType.MaxMana);
        ChangeAnimationState(Name + "Idle");
    }

    private void Update() {
        OnEarlyUpdate();
        OnUpdate();
        OnLateUpdate();
    }

    public virtual Effect AddEffect(EffectType type, int level, float duration, LivingEntity caster)
    {
        Effect newEff = new(type, level, duration, this, caster);
        foreach(var eff in effects)
        {
            if(eff.Type == newEff.Type)
            {
                if(eff.Level < newEff.Level || (eff.Level == newEff.Level && eff.Duration < newEff.Duration))
                {
                    RemoveEffect(eff);
                    break;
                }
                else return null;
            }
        }
        newEff.Type.OnEffectStart(newEff);
        if(newEff.Duration <= 0f) 
        {
            newEff.Type.OnEffectFinish(newEff);
            return null;
        }
        effects.Add(newEff);
        return newEff;
    }

    public virtual void RemoveEffect(Effect eff)
    {
        eff.Type.OnEffectFinish(eff);
        _removeEffects.Enqueue(eff);
    }

    public void RemoveEffect(EffectType type)
    {
        foreach(var eff in effects) 
        {
            if(eff.Type == type) RemoveEffect(eff);
        }
    }
    
    protected void ChangeAnimationState(string state)
    {
        if (curState == state) return;
        animator.Play(curState = state);
    }

    public virtual void Attack(Damageable damageable, Attribute attribute, DamageType type = DamageType.Normal, bool knockback = true)
    {
        if(spriteRenderer.transform.localPosition.y > 1) return; 
        latestAttack = Time.time;
        AttackSound();
        if(damageable is LivingEntity ent && ent.IsDead) return;
        damageable.Hit(attribute, type);
        if(knockback && damageable is LivingEntity livingEntity) 
            livingEntity.AddForce(damageable.transform.position - transform.position, Attribute.GetValue(AttributeType.Knockback));
    }

    public virtual void AttackNearby(float radius, int count = 1, DamageType type = DamageType.Normal, bool knockback = true, string[] targetTags = null)
    {
        if(!IsAttackEnded()) return;
        latestAttack = Time.time;
        var colliders = Physics2D.OverlapCircleAll(transform.position, radius, ~LayerMask.GetMask("Passing"));
        HashSet<Damageable> alreadyDamaged = new();
        foreach(var collider in colliders)
        {
            Damageable damageable = collider.gameObject.GetComponent<Damageable>();
            if(targetTags != null && !targetTags.Contains(collider.gameObject.tag)) continue;
            if(damageable == this || damageable == null || alreadyDamaged.Contains(damageable)) continue;
            if(damageable is LivingEntity ent && ent.IsDead) continue;
            Attack(damageable, Attribute, type, knockback);
            alreadyDamaged.Add(damageable);
        }
    }

    protected virtual void AttackSound()
    {
    }

    public bool IsAttackEnded() 
    {
        return latestAttack < 0 || Time.time - latestAttack > 1f / Attribute.GetValue(AttributeType.AttackSpeed);
    }

    public void AddForce(Vector2 axis, float force) {
        _force += axis.normalized * force;
    }

    public void SetForce(Vector2 axis, float force) {
        _force = axis.normalized * force;
    }

    protected virtual void OnEarlyUpdate()
    {

    }

    protected virtual void OnUpdate()
    {
        EffectUpdate();
    }

    protected virtual void EffectUpdate()
    {
        foreach(var eff in effects)
        {
            eff.Type.OnEffectUpdate(eff);
            if((eff.Duration -= Time.deltaTime) <= 0) RemoveEffect(eff);
        }
        foreach(var target in _removeEffects)
        {
            effects.Remove(target);
        }
        _removeEffects.Clear();
    }

    protected virtual void Kill()
    {
        IsDead = true;
        rigid.velocity = Vector3.zero;
        StartCoroutine(nameof(KillEffect));
    }

    public virtual void Revive()
    {
        foreach(var eff in effects) eff.Duration = 0f;
        _force = Vector2.zero;
        Life = Attribute.GetValue(AttributeType.MaxLife);
        Mana = Attribute.GetValue(AttributeType.MaxMana);
        Stamina = Attribute.GetValue(AttributeType.MaxStamina);
        IsDead = false;
        originalColor = Color.white;
    }

    protected virtual IEnumerator KillEffect()
    {
        Color c = spriteRenderer.color;
        Color targetCol = new Color(1f, 0f, 0f, 0f);
        for(var i = 0f; i <= 1f; i += Time.deltaTime / 0.5f)
        {
            var toChange = Color.Lerp(c, targetCol, i);
            toChange.a = Mathf.Lerp(c.a, targetCol.a, i);
            colors.Enqueue(toChange);
            yield return null;
        }
        spriteRenderer.color = Color.clear;
        originalColor = Color.clear;
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, 1f, 0, 9);
    }

    protected virtual void OnLateUpdate()
    {
        if(Life <= 0f && !IsDead) {
            Kill();
        }

        ValueUpdate();
        if(!IsDead) Move();
        JumpUpdate();

        Attribute.OnLateUpdate();
        ColorUpdate();
        LayerUpdate();
    }

    private void JumpUpdate()
    {
        var pos = spriteRenderer.transform.localPosition;
        pos.y = Mathf.Max(0f, pos.y + JumpVelocity * Time.deltaTime);
        if(pos.y > 0f) JumpVelocity -= 14 * Time.deltaTime;
        else JumpVelocity = 0f;
        spriteRenderer.transform.localPosition = pos;
        animator.speed = IsJumping ? 0f : 1f;
    }

    protected virtual void LayerUpdate()
    {
        var pos = spriteRenderer.transform.localPosition;
        gameObject.layer = pos.y > 0.5f ? PassingLayer : OriginalLayer;
    }

    public void Jump(float vel)
    {
        _jumpVelocity = vel;
    }

    public void AddColor(Color c)
    {
        colors.Enqueue(c);
    }

    protected virtual void Move()
    {
        if (axis.magnitude > 0.1f)
        {
            ChangeAnimationState(Name + "Walk");
            spriteRenderer.flipX = axis.x < 0;
        }
        else 
        {
            ChangeAnimationState(Name + "Idle");
        }

        rigid.velocity = axis.normalized * Attribute.GetValue(AttributeType.MoveSpeed) + _force;
        _force *= Mathf.Pow(0.1f, Time.deltaTime);
        axis = Vector2.zero;
    }

    protected virtual void ValueUpdate()
    {
        var maxStamina = Attribute.GetValue(AttributeType.MaxStamina);

        if (Life > 0) Life += Attribute.GetValue(AttributeType.LifeRegen) * Time.deltaTime;
        Mana += Attribute.GetValue(AttributeType.ManaRegen) * Time.deltaTime;
        Stamina += Attribute.GetValue(AttributeType.StaminaRegen) * Time.deltaTime;

        if(Stamina < maxStamina * .2f)
        {
            Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, 0.6f));
            Attribute.AddModifier(new(AttributeType.StaminaRegen, AttributeModifier.Type.Multiply, 0.2f));
        }
    }
}