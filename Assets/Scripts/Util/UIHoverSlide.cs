using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHoverSlide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Vector2 _slideAxis;
    [SerializeField]
    private RectTransform _rectTransform;

    private Vector2 _startPos;
    private bool _hover = false;

    private void Start() 
    {
        _startPos = _rectTransform.anchoredPosition;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, 
                _hover ? _startPos + _slideAxis : _startPos, Time.deltaTime * 10);
    }

    public void OnPointerEnter(PointerEventData data) => _hover = true;
    public void OnPointerExit(PointerEventData data) => _hover = false;
}
