using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingSlime : Boss
{


    private bool _doFollowPlayer = true;
    private int _pattern = 0;

    protected override void Awake() 
    {
        base.Awake();
        lifeBack.transform.localPosition = Vector2.up * 1.6f;
        DropGoldAmount = 100;

    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 1.5f);
        Attribute.SetDefaultValue(AttributeType.AttackSpeed, 1.5f);
        Attribute.SetDefaultValue(AttributeType.AttackDamage, 15f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, 1500);
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
        StartCoroutine(PatternCoroutine());
    }

    private IEnumerator PatternCoroutine()
    {
        while(true)
        {
            switch(_pattern)
            {
                case 0:
                    for(var i = 0; i < 6; i++)
                    {
                        yield return YieldCache.WaitForSeconds(.55f);
                        for(var j = 0f; j < 360f; j += 90f)
                        {
                            var p = Projectile.SpawnProjectile(transform.position, 
                                    "KingSlimeBall", this, new[]{ "Player" });
                            p.SetAxisAngle(j + i * 30f);
                            p.transform.position += (Vector3)p.MoveAxis.normalized * Size.x * 0.5f;
                            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
                        }
                        SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 0.9f);
                    }
                    yield return YieldCache.WaitForSeconds(3f);
                    _pattern = 1;
                    break;
                case 1:
                    var axis = Player.Instance.transform.position - transform.position;
                    var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
                    for(var i = 0f; i < 360f; i += 15f)
                    {
                        yield return YieldCache.WaitForSeconds(.2f);
                        for(var j = 0f; j < 360f; j += 180f)
                        {
                            var p = Projectile.SpawnProjectile(transform.position, 
                                    "KingSlimeBall", this, new[]{ "Player" });
                            p.SetAxisAngle(i + angle + j);
                            p.transform.position += (Vector3)p.MoveAxis.normalized * Size.x * 0.5f;
                            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
                            SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 0.6f);
                        }
                    }
                    yield return YieldCache.WaitForSeconds(3f);
                    _pattern = 2;
                    break;
                case 2:

                    for(var r = 0; r < 3; r++)
                    {
                        yield return YieldCache.WaitForSeconds(0.7f);

                        Jump(20f);
                        while(IsJumping)
                        {
                            yield return null;
                            Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Add, 5.5f));
                        }

                        yield return YieldCache.WaitForSeconds(0.1f);

                        ParticleManager.Instance.SpawnParticle(transform.position + Vector3.down * Size.x * 0.5f, 
                                ParticleType.HorizontalExplode, Color.white, 1, 0, 20);
                        AttackNearby(1.0f + Size.x * .5f, 1, DamageType.Normal, true, new[] { "Player" });
                    }

                    _pattern = 3;
                    break;
                case 3:

                    for(var c = 0; c < 5; c++)
                    {
                        var r = Random.Range(0f, 360f);
                        for(var i = 0f; i < 320f; i += 20f)
                        {
                            var p = Projectile.SpawnProjectile(transform.position, 
                                    "KingSlimeBall", this, new[]{ "Player" });
                            p.SetAxisAngle(i + r);
                            p.MoveSpeed = 7f;
                            p.MoveAcceleration = 0f;
                            p.transform.position += (Vector3)p.MoveAxis.normalized * Size.x * 0.5f;
                            p.Attribute.SetDefaultValue(AttributeType.AttackDamage, 15);
                        }
                        SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 0.9f);

                        yield return YieldCache.WaitForSeconds(2.1f);
                    }

                    _pattern = 4;
                    break;
                case 4:
                    for(var t = 0f; t < 5f; t += Time.deltaTime)
                    {
                        yield return null;
                        Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, 4));
                    }

                    _pattern = 0;
                    break;
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
        if(_doFollowPlayer) FollowPlayer();
        AttackNearby(.4f + Size.x * .5f, 1, DamageType.Normal, true, new[] { "Player" });
    }

    protected override IEnumerator KillEffect()
    {
        StopCoroutine(PatternCoroutine());
        yield return base.KillEffect();
    }

}