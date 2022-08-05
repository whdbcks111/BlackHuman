using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{

    protected override void Awake() 
    {
        base.Awake();
        Name = "Slime";
        MoveSpeed = 2.0f;
        hpBack.transform.localPosition = Vector2.up * .8f;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        AttackSpeed *= 0.6f;
        AttackDamage = Size.x * 4f;
    }

    protected override void OnEarlyUpdate()
    {
        base.OnEarlyUpdate();
        MaxHp *= Mathf.Pow(1.5f, Size.x - 5);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FollowPlayer();
        AttackNearby(.5f + Size.x * .5f, 1, true, new[] { "Player" });
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
        if(Size.x > 1) 
        {
            var theta = Random.Range(0f, Mathf.PI * 2);
            for(var i = 0; i < 2; i++)
            {
                var slimeObj = Instantiate(Resources.Load<GameObject>("Enemies/Slime"), transform.position, Quaternion.identity);
                var slime = slimeObj.GetComponent<Slime>();
                slime.Size = Size - Vector2Int.one;
                slimeObj.transform.localScale = transform.localScale - Vector3.one;
                slime.AddForce(new Vector3(Mathf.Cos(theta), Mathf.Sin(theta)), 5f);
                theta += Mathf.PI;
            }
        }
    }

}