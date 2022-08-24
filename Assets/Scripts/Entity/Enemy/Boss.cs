using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Boss : Enemy 
{
    protected override void OnLateUpdate()
    {
        base.OnLateUpdate();
        Player.Instance.BossBar.fillAmount = Life / Attribute.GetValue(AttributeType.MaxLife);
    }

    protected override void Start() {
        base.Start();

        Player.Instance.BossBarBack.gameObject.SetActive(true);
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
        Player.Instance.BossBarBack.gameObject.SetActive(false);
    }
}