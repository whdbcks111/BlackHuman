using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Gold : MonoBehaviour
{
    private static int s_wallLayer = -1;

    private int _amount = 1;
    private Vector2 _force;
    private Rigidbody2D _rigid;

    public static void SpawnGold(Vector3 pos, int amount)
    {
        var gold = ObjectPool.Instance.GetGold();
        gold.transform.position = pos;
        gold._amount = amount;
        var angle = Random.Range(0, Mathf.PI * 2);
        gold._force = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(2f, 4f);
    }

    private void Awake() 
    {
        if (s_wallLayer == -1) s_wallLayer = LayerMask.NameToLayer("Wall");
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void Update() 
    {
        _rigid.velocity = _force;
        _force *= Mathf.Pow(0.1f, Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.layer == s_wallLayer) _force *= -1f;
        if(!other.gameObject.CompareTag("Player")) return;
        Player.Instance.GoldAmount += _amount;
        SoundManager.Instance.PlayOneShot("Coin", 1f);
        ObjectPool.Instance.DestroyGold(this);
    }
}
