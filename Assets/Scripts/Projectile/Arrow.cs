using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        SoundManager.Instance.PlayOneShot("Arrow", 1f);
    }
}