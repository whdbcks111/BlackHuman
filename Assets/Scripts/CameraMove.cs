using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private Vector3 offset;

    private void Awake()
    {
        offset = transform.position - target.position;
    }

    private void FixedUpdate() {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.fixedDeltaTime * 10);
    }
}
