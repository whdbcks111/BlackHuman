using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LivingEntity : MonoBehaviour
{
    public const float DefaultMoveSpeed = 3.0f, 
            DefaultHp = 100f, 
            DefaultMp = 100f, 
            DefaultAttackSpeed = 1f,
            DefaultAttackDamage = 12f;

    [SerializeField]
    protected Animator animator;
    [SerializeField]
    protected Rigidbody2D rigid;
    [SerializeField]
    protected SpriteRenderer spriteRenderer;


    private string _name;
    public bool IsDead { get; private set; }
    public string Name { get { return _name; } set { _name = value; } }
    public string curState { get; private set; }
    private float _hp = DefaultHp, _mp = DefaultMp;
    [HideInInspector]
    public float MoveSpeed = DefaultMoveSpeed, AttackSpeed = DefaultAttackSpeed;
    [HideInInspector]
    public float AttackDamage = DefaultAttackDamage;
    [HideInInspector]
    public float MaxHp = DefaultHp, MaxMp = DefaultMp;
    [HideInInspector]
    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value, 0, MaxHp); } }
    [HideInInspector]
    public float Mp { get { return _mp; } set { _mp = Mathf.Clamp(value, 0, MaxMp); } }
    protected Vector2 axis = Vector2.zero;
    private Vector2 _force;
    private float _latestAttack = -1;

    protected void ChangeAnimationState(string state)
    {
        if (curState == state) return;
        
        animator.Play(state);
        curState = state;
    }

    public virtual void AttackNearby(float radius, int count = 1, bool knockback = true, string[] targetTags = null)
    {
        if(!IsAttackEnded()) return;
        _latestAttack = Time.time;
        var colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        HashSet<LivingEntity> alreadyDamaged = new();
        foreach(var collider in colliders)
        {
            if(targetTags != null && !targetTags.Contains(collider.gameObject.tag)) continue;
            LivingEntity entity = collider.gameObject.GetComponent<LivingEntity>();
            if(entity == this || entity == null || alreadyDamaged.Contains(entity)) continue;
            entity.ShowDamageEffect();
            entity.Damage(15);
            alreadyDamaged.Add(entity);
            if(knockback) entity.AddForce(entity.transform.position - transform.position, 7);
        }
    }

    public virtual void Damage(float amount) {
        Hp -= amount;
    }

    public bool IsAttackEnded() 
    {
        return _latestAttack < 0 || Time.time - _latestAttack > 1f / AttackSpeed;
    }

    public void AddForce(Vector2 axis, float force) {
        _force += axis.normalized * force;
    }

    protected void ShowDamageEffect()
    {
        StopCoroutine(nameof(ShowDamageEffectCoroutine));
        StartCoroutine(nameof(ShowDamageEffectCoroutine), .5f);
    }

    private IEnumerator ShowDamageEffectCoroutine(float time)
    {
        var ratio = 1f;
        while (ratio > 0f)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, ratio);
            ratio -= Time.deltaTime / time;
            yield return null;
        }
        spriteRenderer.color = Color.white;
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        ChangeAnimationState(Name + "Idle");
    }

    private void Update() {
        OnEarlyUpdate();
        OnUpdate();
        OnLateUpdate();
    }

    protected virtual void OnEarlyUpdate()
    {
        ResetValues();
    }

    protected virtual void OnUpdate()
    {
        if(Hp <= 0f && !IsDead) {
            Kill();
        }
    }

    protected virtual void ResetValues()
    {
        MaxMp = DefaultMp;
        MaxHp = DefaultHp;
        MoveSpeed = DefaultMoveSpeed;
        AttackDamage = DefaultAttackDamage;
        AttackSpeed = DefaultAttackSpeed;
    }

    protected virtual void Kill()
    {
        IsDead = true;
        rigid.velocity = Vector3.zero;
        StopCoroutine(nameof(ShowDamageEffectCoroutine));
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
            spriteRenderer.color = toChange;
            yield return null;
        }
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.WhiteSmoke, .7f);
    }

    protected virtual void OnLateUpdate()
    {
        ValueUpdate();
        if(!IsDead) Move();
    }

    protected virtual void Move()
    {
        if (Mathf.Abs(axis.normalized.x) > 0.7f)
        {
            ChangeAnimationState(Name + (axis.x > 0 ? "Walk_Right" : "Walk_Left"));
        }
        else
        {
            if (Mathf.Abs(axis.normalized.y) > 0.1f)
            {
                ChangeAnimationState(Name + (axis.y > 0 ? "Walk_Up" : "Walk_Down"));
            }
            else
            {
                ChangeAnimationState(Name + "Idle");
            }
        }

        rigid.velocity = axis.normalized * MoveSpeed + _force;
        _force *= Mathf.Pow(0.1f, Time.deltaTime);
        axis = Vector2.zero;
    }

    protected virtual void ValueUpdate()
    {
        if (Hp > 0) Hp += 0.5f * Time.deltaTime;
        Mp += 1 * Time.deltaTime;
    }
}