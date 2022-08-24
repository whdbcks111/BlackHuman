using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBall : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity);
        SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 2f);
    }
}