using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Project Settings에서 Storage의 실행 순서를 -2로 설정함
public class Storage : MonoBehaviour
{
    public static Storage Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, GameObject> data;

    private void Awake() {
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
