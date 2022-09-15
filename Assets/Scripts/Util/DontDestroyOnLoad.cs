using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        while(transform.childCount > 0)
        {
            var t = transform.GetChild(0);
            t.SetParent(null);
            DontDestroyOnLoad(t.gameObject);
        }
    }
}
