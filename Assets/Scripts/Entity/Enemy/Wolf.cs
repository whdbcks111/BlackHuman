using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Enemy
{

    protected override void Awake() 
    {
        base.Awake();
        Name = "Wolf";
        lifeBack.transform.localPosition = Vector2.up * .8f;
    }

    protected override void Start() 
    {
        base.Start();
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        while(!IsDead)
        {
            yield return new WaitForSeconds(3f + Random.value * 5);

            SoundManager.Instance.PlayOneShot("WolfBark_" + Random.Range(1, 2+1), 2.0f);
            SetForce(Player.Instance.transform.position - transform.position, 10f);
            for(var i = 0f; i < 0.5f; i += 0.1f) {
                ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, Color.red, .1f, (int)(10 / 0.1f));
                yield return new WaitForSeconds(0.1f);
            }
            AddForce(Player.Instance.transform.position - transform.position, 6f);
        }
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 3.6f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, .5f);
        Attribute.SetDefaultValue(AttributeType.AttackDamage, 8f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 120f);
    }

    protected override void AttackSound()
    {
        SoundManager.Instance.PlayOneShot("WolfBite_" + Random.Range(1, 2+1), 2.0f);
    }

    protected override void HitSound()
    {
        SoundManager.Instance.PlayOneShot("WolfDamaged_" + Random.Range(1, 2+1), 1.5f);
    }

    protected override void OnEarlyUpdate()
    {
        base.OnEarlyUpdate();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FollowPlayer();
        AttackNearby(.5f + Size.x * .5f, 1, DamageType.Normal, true, new[] { "Player", "Block" });
    }

    protected override IEnumerator KillEffect()
    {

        yield return base.KillEffect();
    }

}