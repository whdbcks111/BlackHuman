using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<ParticleType, GameObject> _particles;

    private void Awake() 
    {
        Instance = this;
    }

    public void SpawnParticle(Vector3 pos, ParticleType type, float duration = 1f, 
            int totalCount = 0, bool loop = false, Transform parent=null)
    {
        if(!_particles.ContainsKey(type)) throw new UnityException("Invalid Particle Name");
        var particleSystem = Instantiate(_particles[type], parent == null ? transform: parent);
        particleSystem.transform.localPosition = pos;
        var ps = particleSystem.GetComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.duration = duration;
        main.loop = loop;
        var emission = ps.emission;
        if(totalCount > 0) emission.rateOverTime = totalCount / duration;
        ps.Play();
    }

    public void SpawnParticle(ParticleType type, Transform parent, float duration = 1f, int count = 0, bool loop = false)
    {
        SpawnParticle(Vector3.zero, type, duration, count, loop, parent);
    }
}
