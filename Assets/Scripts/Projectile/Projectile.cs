using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{

    private static int s_wallLayer = -1;

    public Attribute Attribute = new();
    public float MoveSpeed, RotateSpeed, MoveAcceleration, RotateAcceleration, Duration, MinSpeed, MaxSpeed;
    public bool RotateBackwards, DestoryInCollision, DestroyInWall;
    public Vector2 MoveAxis;

    protected string[] targetTags;
    protected LivingEntity self;

    public static Projectile SpawnProjectile(Vector3 pos, string name, LivingEntity self, string[] targetTags = null)
    {
        var obj = Instantiate(Resources.Load<GameObject>("Projectiles/" + name), pos, Quaternion.identity);
        obj.transform.SetParent(Storage.Get("ProjectileContainer").transform);
        var p = obj.GetComponent<Projectile>();
        p.self = self;
        p.targetTags = targetTags;
        return p;
    }

    public void SetAxisAngle(float angle)
    {
        MoveAxis = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    protected virtual void Awake()
    {
        if (s_wallLayer == -1) s_wallLayer = LayerMask.NameToLayer("Wall");
        StartCoroutine(DestroyCoroutine());
    }

    protected virtual void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(MoveAxis.y, MoveAxis.x) * Mathf.Rad2Deg);
    }

    protected virtual void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(MoveAxis.y, MoveAxis.x) * Mathf.Rad2Deg);
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
        if (damageable != null && damageable != self)
        {
            if (damageable is LivingEntity && targetTags != null && !targetTags.Contains(other.gameObject.tag)) return;
            if (DestoryInCollision) Destroy(gameObject);
            OnCollision(damageable);
        }
        else if (other.gameObject.layer == s_wallLayer)
        {
            if (DestoryInCollision) Destroy(gameObject);
            else if (DestroyInWall) Destroy(gameObject);
            OnCollisionInWall();
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
        Destroy(gameObject);
    }
}
