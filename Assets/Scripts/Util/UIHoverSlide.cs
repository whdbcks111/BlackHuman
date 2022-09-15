using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverSlide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Vector2 _slideAxis;

    private Vector2 _startPos;
    private bool _hover = false;

    private void Start() 
    {
        _startPos = transform.position;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        transform.position = Vector2.Lerp(transform.position, 
                _hover ? _startPos + _slideAxis : _startPos, Time.deltaTime * 10);
    }

    public void OnPointerEnter(PointerEventData data) => _hover = true;
    public void OnPointerExit(PointerEventData data) => _hover = false;
}
