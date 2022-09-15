using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        SoundManager.Instance.PlayOneShot("Arrow", 1f);
        if(entity is LivingEntity living) living.AddEffect(EffectType.Blindness, 2, 5, Self);
    }
}