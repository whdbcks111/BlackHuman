using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    private Vector3 _offset, _shakeMovement = Vector3.zero;
    private float _shakeRange, _shakeTimer;

    private void Awake()
    {
        _offset = transform.position - _target.position;
    }

    private void FixedUpdate() {
        transform.position = Vector3.Lerp(transform.position, _target.position + _offset, Time.fixedDeltaTime * 10) + _shakeMovement;
    }

    public void Shake(float range, float time) {
        StopCoroutine(nameof(ShakeCoroutine));
        _shakeRange = range;
        _shakeTimer = time;
        StartCoroutine(nameof(ShakeCoroutine));
    }

    private IEnumerator ShakeCoroutine() {
        _shakeMovement = Vector3.zero;
        while(_shakeTimer > 0f) 
        {
            _shakeMovement= new Vector3(Random.Range(-_shakeRange, _shakeRange), Random.Range(-_shakeRange, _shakeRange));
            _shakeTimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _shakeMovement = Vector3.zero;
    }

}
