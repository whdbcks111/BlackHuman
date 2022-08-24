using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClayGolem : Enemy
{

    protected override void Awake() 
    {
        base.Awake();
        Name = "ClayGolem";
        lifeBack.transform.localPosition = Vector2.up * .8f;
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 2f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, .6f);
        Attribute.SetDefaultValue(AttributeType.AttackDamage, 10f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 200);
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
        StartCoroutine(ClayLaunch());
    }

    private IEnumerator ClayLaunch()
    {
        while(!IsDead)
        {
            yield return new WaitForSeconds(3.25f + Random.value * 2.5f);
            
            if(IsDead) break;
            for(var i = 0; i < 340; i += Random.Range(25, 40)) 
            {
                // SoundManager.Instance.PlayOneShot("Arrow", 1.0f);
                var dir = Player.Instance.transform.position - transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle += i;
                angle *= Mathf.Deg2Rad;
                dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                
                var spawnPos = transform.position + dir * 1.5f;
                var p = Projectile.SpawnProjectile(spawnPos, "ClayBall", this, new[]{ "Player" });
                ParticleManager.Instance.SpawnParticle(spawnPos, ParticleType.HorizontalExplode, new Color(.6f, .3f, 0), 0.1f, 0, 4);
                p.MoveAxis = dir;
                p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 20);
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