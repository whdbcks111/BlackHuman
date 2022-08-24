using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{

    protected override void Awake() 
    {
        base.Awake();
        Name = "Slime";
        lifeBack.transform.localPosition = Vector2.up * .8f;
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 3.5f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, .6f * Mathf.Pow(1.5f, 3 - Size.x));
        Attribute.SetDefaultValue(AttributeType.AttackDamage, Size.x * 4f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 
                Attribute.GetDefaultValue(AttributeType.MaxLife) * Mathf.Pow(1.5f, Size.x - 3)
        );
    }

    protected override void AttackSound()
    {
        SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 1.6f);
    }

    protected override void HitSound()
    {
        SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 1.6f);
    }

    protected override void Start()
    {
        base.Start();
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
        var randomAngle = Random.value < 0.5f ? 0 : 45;
        for(var angle = 0f; angle < 360f; angle += 360f / 4)
        {
            var p = Projectile.SpawnProjectile(transform.position, "SlimeBall", this, new[]{ "Player" });
            p.SetAxisAngle(angle + randomAngle);
            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 7);
        }

        yield return base.KillEffect();

        if(Size.x > 1) 
        {
            var theta = Random.Range(0f, Mathf.PI * 2);
            for(var i = 0; i < 2; i++)
            {
                var slime = Enemy.SpawnEnemy("Slime", transform.position);
                slime.Size = Size - Vector2Int.one;
                slime.gameObject.transform.localScale = transform.localScale - Vector3.one;
                var dir = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
                slime.transform.position += dir * 0.1f;
                slime.AddForce(dir, 5f);
                theta += Mathf.PI;
            }
        }
    }

}