using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class ObjectPool : MonoBehaviour {
    public static ObjectPool Instance { get; private set; }

    private Queue<TextMeshProUGUI> _disabledDamageTexts = new();
    private Queue<Gold> _disabledGolds = new();
    private Dictionary<string, Queue<Projectile>> _disabledProjectiles = new();

    private Dictionary<string, Projectile> _projectilePrefabs = new();

    
    private Transform _worldCanvas;
    private TextMeshProUGUI _damageTextPrefab;
    private Gold _goldPrefab;
    private Transform _goldContainer;
    private Transform _projectileContainer;

    private void Awake() {
        Instance = this;

        _goldPrefab = Resources.Load<Gold>("Interactable/Gold");
        _damageTextPrefab = Resources.Load<TextMeshProUGUI>("UI/DamageDisplay");
    }

    private void Start() {
        _worldCanvas = Storage.Get("WorldCanvas").transform;
        _goldContainer = Storage.Get("GoldContainer").transform;
    }

    public Projectile GetProjectile(string name, Vector3 pos)
    {
        if(!_projectilePrefabs.ContainsKey(name)) _projectilePrefabs[name] = Resources.Load<Projectile>("Projectiles/" + name);
        var prefab = _projectilePrefabs[name];
        if(!_disabledProjectiles.ContainsKey(name)) _disabledProjectiles[name] = new();
        if(_disabledProjectiles[name].Count > 0)
        {
            var p = _disabledProjectiles[name].Dequeue();
            p.transform.position = pos;
            p.gameObject.SetActive(true);
            p.MoveSpeed = prefab.MoveSpeed;
            p.RotateSpeed = prefab.RotateSpeed;
            p.Duration = prefab.Duration;
            p.Init();
            return p;
        }
        else 
        {
            var p = Instantiate(_projectilePrefabs[name], pos, Quaternion.identity);
            p.Init();
            return p;
        }
    }

    public void DestroyProjectile(Projectile p)
    {
        p.gameObject.SetActive(false);
        p.StopAllCoroutines();
        _disabledProjectiles[p.Name].Enqueue(p);
    }

    public TextMeshProUGUI GetDamageText()
    {
        if(_disabledDamageTexts.Count > 0)
        {
            var text = _disabledDamageTexts.Dequeue();
            text.gameObject.SetActive(true);
            return text;
        }
        return Instantiate(_damageTextPrefab, _worldCanvas);
    }

    public void DestroyDamageText(TextMeshProUGUI text)
    {
        text.gameObject.SetActive(false);
        _disabledDamageTexts.Enqueue(text);
    }

    public Gold GetGold()
    {
        if(_disabledGolds.Count > 0)
        {
            var gold = _disabledGolds.Dequeue();
            gold.gameObject.SetActive(true);
            return gold;
        }
        return Instantiate(_goldPrefab, _goldContainer);
    }

    public void DestroyGold(Gold gold)
    {
        gold.gameObject.SetActive(false);
        _disabledGolds.Enqueue(gold);
    }

}