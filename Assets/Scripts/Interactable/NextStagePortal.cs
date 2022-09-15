using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStagePortal : MonoBehaviour, IInteractable
{
    private static NextStagePortal s_prefab = null;
    private float _particleTimer = 0f, _particleSpan = 0.4f;

    public void Interact()
    {
        print("Next" + GameManager.Instance.Stage);
        GameManager.Instance.NextStage();
    }

    private void Update() {
        if((_particleTimer += Time.deltaTime) > _particleSpan)
        {
            ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, Color.magenta, 0, 0, 2);
            _particleTimer -= _particleSpan;
        }
    }

    public static NextStagePortal PlacePortal(Vector3 pos)
    {
        if(s_prefab == null) s_prefab = Resources.Load<NextStagePortal>("Interactable/NextStagePortal");

        var portal = Instantiate(s_prefab);
        GameManager.Instance.AddInteractable(portal);
        portal.transform.position = pos;
        return portal;
    }
}
