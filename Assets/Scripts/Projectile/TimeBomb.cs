using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBomb : Projectile
{
    public override void Init()
    {
        base.Init();
        StartCoroutine(SmokeCoroutine());
    }
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
    }

    private void OnDisable() {
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.HorizontalExplode, 
                Color.white, 0.1f, 0, 40);
        SoundManager.Instance.PlayOneShot("Explosion", 1f);
        foreach(var collider in Physics2D.OverlapCircleAll(transform.position, 2.5f))
        {
            var living = collider.gameObject.GetComponent<LivingEntity>();
            living?.Hit(Attribute.Damage(Attribute.GetValue(AttributeType.MagicDamage)));
        }
    }

    private IEnumerator SmokeCoroutine()
    {
        while(true)
        {
            yield return YieldCache.WaitForSeconds(0.1f);
            ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, 
                    Color.white, 0.1f, 0, 1);
        }
    }
}