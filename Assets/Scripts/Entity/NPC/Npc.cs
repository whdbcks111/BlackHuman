using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Npc : LivingEntity
{
    private static List<Npc> _npcs = new();

    private GameObject _icon;
    
    public static Npc SpawnNpc(string name, Vector2 pos)
    {
        return SpawnNpc(Resources.Load<GameObject>("Npcs/" + name), pos);
    }
    
    public static Npc SpawnNpc(GameObject npc, Vector2 pos)
    {
        return SpawnNpc(npc.GetComponent<Npc>(), pos);
    }
    
    public static Npc SpawnNpc(Npc npc, Vector2 pos)
    {
        var obj = Instantiate(npc, Storage.Get("NpcContainer").transform);
        obj.transform.position = pos;
        return obj;
    }

    public static IEnumerable<Npc> GetNpcs()
    {
        foreach(var e in _npcs) yield return e;
    }

    protected override void Awake()
    {
        _npcs.Add(this);
        base.Awake();

        _icon = Instantiate(Resources.Load<GameObject>("Npcs/NpcIcon"), transform);
    }

    protected void ResizeIcon()
    {
        var s = _icon.transform.localScale;
        var ps = transform.localScale;
        _icon.transform.localScale = new Vector3(s.x / ps.x, s.y / ps.y);
    }

    private void OnDestroy() {
        _npcs.Remove(this);
    }

    protected override void Start()
    {
        base.Start();
        ResizeIcon();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
        _npcs.Remove(this);
        Destroy(gameObject);
    }
}
