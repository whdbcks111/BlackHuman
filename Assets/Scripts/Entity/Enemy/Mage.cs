using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Enemy
{

    private Vector2 _timeAxis = Vector2.zero;

    protected override void Awake() 
    {
        base.Awake();
        Name = "Mage";
        lifeBack.transform.localPosition = Vector2.up * .8f;
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 3f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, 0.1f);
        Attribute.SetDefaultValue(AttributeType.AttackDamage, 1f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 80);
    }

    protected override void AttackSound()
    {
        // SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), .2f);
    }

    protected override void HitSound()
    {
        // SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), .2f);
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(FireballLaunch());
        StartCoroutine(AxisChange());
    }

    private IEnumerator AxisChange()
    {
        while(!IsDead)
        {
            yield return new WaitForSeconds(1f);
            _timeAxis = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
    }

    private IEnumerator FireballLaunch()
    {
        while(!IsDead)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4.5f));
            
            if(IsDead) break;
            for(var i = -1; i < 2; i++) 
            {
                yield return new WaitForSeconds(0.2f);
                SoundManager.Instance.PlayOneShot("Fireball", 1.0f);
                var dir = Player.Instance.transform.position - transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle += i * 30 + Random.Range(-10, 10);
                angle *= Mathf.Deg2Rad;
                dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                
                var spawnPos = transform.position + dir * 1.5f;
                var p = Projectile.SpawnProjectile(spawnPos, "Fireball", this, new[]{ "Player" });
                ParticleManager.Instance.SpawnParticle(spawnPos, ParticleType.Smoke, 0.1f, 0, 10);
                p.MoveAxis = dir;
                p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 10);
            }
        }
    }

    protected override void OnEarlyUpdate()
    {
        base.OnEarlyUpdate();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        axis = _timeAxis;
        AttackNearby(.3f + Size.x * .5f, 1, DamageType.Normal, true, new[] { "Player" });
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
    }

}