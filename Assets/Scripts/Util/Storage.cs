using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public static Storage Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, GameObject> data;

    private void Awake() {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static T Get <T> (string name)
    {
        return Instance.data[name].GetComponent<T>();
    }

    public static GameObject Get(string name)
    {
        return Instance.data[name];
    }

}
