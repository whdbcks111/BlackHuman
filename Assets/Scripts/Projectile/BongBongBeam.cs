using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BongBongBeam : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        if(entity is LivingEntity) Self.Life += 1.5f; 
    }
}