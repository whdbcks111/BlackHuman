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

    public ParticleSystem SpawnParticle(Vector3 pos, ParticleType type, Color color, float duration = 1f, 
            int countPerSecond = 0, int burst = 0, bool loop = false, Transform parent=null)
    {
        if(!_particles.ContainsKey(type)) throw new UnityException("Invalid Particle Name");
        var particleSystem = Instantiate(_particles[type], parent == null ? transform: parent);
        particleSystem.transform.localPosition = pos;
        var ps = particleSystem.GetComponent<ParticleSystem>();
        ps.Stop();

        var main = ps.main;
        main.duration = duration;
        main.loop = loop;
        if(color != Color.clear) main.startColor = color;

        var emission = ps.emission;
        emission.rateOverTime = countPerSecond;

        var burstOpt = emission.GetBurst(0);
        burstOpt.count = burst;
        emission.SetBurst(0, burstOpt);

        ps.Play();

        return ps;
    }

    public ParticleSystem SpawnParticle(Vector3 pos, ParticleType type, float duration = 1f, 
            int countPerSecond = 0, int burst = 0, bool loop = false, Transform parent=null) 
        => SpawnParticle(pos, type, Color.clear, duration, countPerSecond, burst, loop, parent);

    public ParticleSystem SpawnParticle(ParticleType type, Transform parent, float duration = 1f, 
            int count = 0, int burst = 0, bool loop = false)
         => SpawnParticle(Vector3.zero, type, Color.clear, duration, count, burst, loop, parent);

    public ParticleSystem SpawnParticle(ParticleType type, Color color, Transform parent, float duration = 1f, 
            int count = 0, int burst = 0, bool loop = false)
        => SpawnParticle(Vector3.zero, type, color, duration, count, burst, loop, parent);
}
