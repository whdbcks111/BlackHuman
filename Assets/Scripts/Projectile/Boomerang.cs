using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : Projectile
{
    protected override void Update() {
        base.Update();
        transform.rotation = Quaternion.Euler(0, 0, Time.time * 3f * 360f);
        if(MoveSpeed < 0) MoveAxis = transform.position - Self.transform.position;
        if(Vector2.Distance(transform.position, Self.transform.position) < 1f) ObjectPool.Instance.DestroyProjectile(this);
    }
}