using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Projectile
{
    protected override void OnCollision(Damageable entity)
    {
        base.OnCollision(entity); 
        // SoundManager.Instance.PlayOneShot("Slime_" + Random.Range(1, 2 + 1), 2f);
    }

    protected override void Start()
    {
        base.Start();
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