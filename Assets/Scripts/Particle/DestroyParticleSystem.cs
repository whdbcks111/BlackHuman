using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticleSystem : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    void Update()
    {
        if(!_particleSystem.isPlaying) Destroy(gameObject);
    }
}
