using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LivingEntity : Damageable
{
    public readonly Dictionary<string, object> Extras = new();

    [SerializeField]
    protected Animator animator;
    [SerializeField]
    protected Rigidbody2D rigid;


    private string _name;
    public bool IsDead { get; private set; }
    public string Name { get { return _name; } set { _name = value; } }
    public string curState { get; private set; }

    private float _mana, _stamina;
    [HideInInspector]
    public float Mana { get { return _mana; } set { _mana = Mathf.Clamp(value, 0, Attribute.GetValue(AttributeType.MaxMana)); } }
    [HideInInspector]
    public float Stamina { get { return _stamina; } set { _stamina = Mathf.Clamp(value, 0, Attribute.GetValue(AttributeType.MaxStamina)); } }

    protected Vector2 axis = Vector2.zero;
    private Vector2 _force;
    protected float latestAttack = -1;

    protected override void Awake()
    {
        base.Awake();
        InitializeDefaults();
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
    
    protected void ChangeAnimationState(string state)
    {
        if (curState == state) return;
        animator.Play(state);
        curState = state;
    }

    public virtual void Attack(Damageable damageable, Attribute attribute, DamageType type = DamageType.Normal, bool knockback = true)
    {
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
        if(Life <= 0f && !IsDead) {
            Kill();
        }
    }

    protected virtual void Kill()
    {
        IsDead = true;
        rigid.velocity = Vector3.zero;
        StartCoroutine(nameof(KillEffect));
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
        ValueUpdate();
        if(!IsDead) Move();

        Attribute.OnLateUpdate();
        ColorUpdate();
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