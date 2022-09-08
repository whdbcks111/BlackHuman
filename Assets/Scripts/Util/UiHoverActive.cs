using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UiHoverActive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static Dictionary<GameObject, GameObject> _activeMap = new();

    [SerializeField]
    private GameObject _ui;

    public void OnPointerEnter(PointerEventData data)
    {
        _activeMap[_ui] = gameObject;
        _ui.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data)
    {
        StartCoroutine(ExitCoroutine());
    }

    private IEnumerator ExitCoroutine()
    {
        yield return null;
        yield return null;
        if(_activeMap.ContainsKey(_ui) && _activeMap[_ui] != gameObject) yield break;
        _ui.SetActive(false);
    }
}
