using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicShield : Projectile
{

    protected override void Update()
    {
        base.Update();
        foreach(var col in Physics2D.OverlapCircleAll(transform.position, 1.3f))
        {
            var p = col.gameObject.GetComponent<Projectile>();
            if(p == null) continue;
            if(p.Self == Self) continue;
            p.MoveAxis = p.transform.position - transform.position;
            p.Self = Self;
            p.TargetTags = null;
        }
    }

    protected override void OnCollision(Damageable damageable)
    {
        base.OnCollision(damageable);
        if(damageable is LivingEntity living)
        {
            living.AddForce(living.transform.position - transform.position, 3);
        }
    }
}