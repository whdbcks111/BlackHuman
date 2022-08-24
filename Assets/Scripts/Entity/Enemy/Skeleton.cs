using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{

    protected override void Awake() 
    {
        base.Awake();
        Name = "Skeleton";
        lifeBack.transform.localPosition = Vector2.up * .8f;
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 2f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, 1f);
        Attribute.SetDefaultValue(AttributeType.AttackDamage, 10f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 100);
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
        StartCoroutine(ArrowLaunch());
    }

    private IEnumerator ArrowLaunch()
    {
        while(!IsDead)
        {
            yield return new WaitForSeconds(1.25f + Random.value * 0.5f);

            if(IsDead) break;
            SoundManager.Instance.PlayOneShot("BowPull", 1.5f);
            yield return new WaitForSeconds(1f);
            
            if(IsDead) break;
            for(var i = 0; i < Random.Range(1, 2 + 1); i++) 
            {
                yield return new WaitForSeconds(0.1f);
                SoundManager.Instance.PlayOneShot("Arrow", 1.0f);
                var dir = Player.Instance.transform.position - transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle += Random.Range(-20f, 20f);
                angle *= Mathf.Deg2Rad;
                dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                
                var spawnPos = transform.position + dir * 1.5f;
                var p = Projectile.SpawnProjectile(spawnPos, "Arrow", this, new[]{ "Player" });
                ParticleManager.Instance.SpawnParticle(spawnPos, ParticleType.Smoke, 0.1f, 0, 10);
                p.MoveAxis = dir;
                p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
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
        FollowPlayer();
        AttackNearby(.3f + Size.x * .5f, 1, DamageType.Normal, true, new[] { "Player" });
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
    }

}