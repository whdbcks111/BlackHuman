using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBottle : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
    }

    private void OnDisable() {
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.HorizontalExplode, 
                Color.Lerp(Color.red, Color.yellow, 0.3f), 0.1f, 0, 30);
        SoundManager.Instance.PlayOneShot("Fireball", 1f);
        foreach(var collider in Physics2D.OverlapCircleAll(transform.position, 2f))
        {
            var living = collider.gameObject.GetComponent<LivingEntity>();
            living?.AddEffect(EffectType.Fire, 3, Random.Range(8f, 19f), self);
        }
    }

    protected override void Update() {
        base.Update();
        transform.rotation = Quaternion.Euler(0, 0, Time.time * 5f * 360f);
    }
}