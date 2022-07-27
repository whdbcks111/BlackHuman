using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeActivate : MonoBehaviour
{
    private void Awake() {
        for(var i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);
    }
}
