using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{

    private static int s_wallLayer = -1;

    public string Name { get; private set; }
    public Attribute Attribute = new();
    public float MoveSpeed, RotateSpeed, MoveAcceleration, RotateAcceleration, Duration, MinSpeed, MaxSpeed;
    public bool RotateBackwards, DestoryInCollision, DestroyInWall, DestroyInBlock, DamageOnce;
    public Vector2 MoveAxis;

    public string[] TargetTags;
    public LivingEntity Self;

    private HashSet<Damageable> _damaged = new();

    public static Projectile SpawnProjectile(Vector3 pos, string name, LivingEntity self, string[] targetTags = null)
    {
        var p = ObjectPool.Instance.GetProjectile(name, pos);
        p.Self = self;
        p.TargetTags = targetTags;
        p.Name = name;
        return p;
    }

    public void SetAxisAngle(float angle)
    {
        MoveAxis = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    protected virtual void Awake()
    {
        if (s_wallLayer == -1) s_wallLayer = LayerMask.NameToLayer("Wall");
    }

    public virtual void Init()
    {
        _damaged.Clear();
        StartCoroutine(DestroyCoroutine());
    }

    protected virtual void Update()
    {
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(MoveAxis.y, MoveAxis.x) * Mathf.Rad2Deg);
        transform.Translate(MoveAxis.normalized * MoveSpeed * Time.deltaTime, Space.World);

        MoveSpeed += MoveAcceleration * Time.deltaTime;
        MoveSpeed = Mathf.Clamp(MoveSpeed, MinSpeed, MaxSpeed);

        var angle = Mathf.Atan2(MoveAxis.y, MoveAxis.x) * Mathf.Rad2Deg;
        angle += RotateSpeed * (RotateBackwards ? -1 : 1) * Time.deltaTime;
        MoveAxis = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        RotateSpeed = Mathf.Max(0f, RotateSpeed + RotateAcceleration * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var damageable = other.gameObject.GetComponent<Damageable>();
        if (damageable != null && damageable != Self)
        {
            if (damageable is LivingEntity && TargetTags != null && !TargetTags.Contains(other.gameObject.tag)) return;
            if (DestoryInCollision) ObjectPool.Instance.DestroyProjectile(this);
            else if (DestroyInBlock && damageable is Block) ObjectPool.Instance.DestroyProjectile(this);
            if (DamageOnce && _damaged.Contains(damageable)) return;
            if (!damageable.Invulerable) OnCollision(damageable);
            _damaged.Add(damageable);
        }
        else if (other.gameObject.layer == s_wallLayer)
        {
            OnCollisionInWall();
            if (DestoryInCollision || DestroyInWall) ObjectPool.Instance.DestroyProjectile(this);
        }
    }

    protected virtual void OnCollision(Damageable damageable)
    {
        damageable.Hit(Attribute, DamageType.Normal);
    }

    protected virtual void OnCollisionInWall()
    {
    }

    private IEnumerator DestroyCoroutine()
    {
        while (Duration > 0f)
        { 
            yield return null;
            Duration -= Time.deltaTime;
        }
        ObjectPool.Instance.DestroyProjectile(this);
    }

}
