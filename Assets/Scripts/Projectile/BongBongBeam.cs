using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BongBongBeam : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        Player.Instance.Life += 1.5f; 
    }
}