using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Projectile
{
    public int EffectLevel = 1;
    public float EffectDuration = 5f;
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        if(entity is LivingEntity living) living.AddEffect(EffectType.Fire, EffectLevel, EffectDuration, Self);
        SoundManager.Instance.PlayOneShot("Explosion", .2f);
    }

    protected override void OnCollisionInWall()
    {
        base.OnCollisionInWall();
        SoundManager.Instance.PlayOneShot("Explosion", .2f);
    }

    public override void Init()
    {
        base.Init();
        StartCoroutine(ParticleCoroutine());
    }

    private IEnumerator ParticleCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            var p = ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, 
                    Color.Lerp(Color.red, Color.yellow, Random.Range(0.2f, 0.7f)), 0.1f, 0, 3, false);
            p.gameObject.transform.Rotate(new Vector3(0, 0, 90 + transform.rotation.eulerAngles.z), Space.World);
        }
    }
}