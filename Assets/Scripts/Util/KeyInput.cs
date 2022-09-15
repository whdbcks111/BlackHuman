using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public static class KeyInput
{
    private static readonly KeyCode[] _keyCodes =
        System.Enum.GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(k => ((int)k < (int)KeyCode.Mouse0))
            .ToArray();

    public static List<KeyCode> GetCurrentKeysDown()
    {
        List<KeyCode> result = new();
        if (Input.anyKeyDown)
            for (int i = 0; i < _keyCodes.Length; i++)
                if (Input.GetKeyDown(_keyCodes[i]))
                    result.Add(_keyCodes[i]);
        return result;
    }

    public static List<KeyCode> GetCurrentKeys()
    {
        List<KeyCode> result = new();
        if (Input.anyKey)
            for (int i = 0; i < _keyCodes.Length; i++)
                if (Input.GetKey(_keyCodes[i]))
                    result.Add(_keyCodes[i]);
        return result;
    }

    public static List<KeyCode> GetCurrentKeysUp()
    {
        List<KeyCode> result = new();
        for (int i = 0; i < _keyCodes.Length; i++)
            if (Input.GetKeyUp(_keyCodes[i]))
                result.Add(_keyCodes[i]);
        return result;
    }

    public static bool GetKeyDown(KeyCode code)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetKeyDown(code);
    }

    public static bool GetKey(KeyCode code)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetKey(code);
    }

    public static bool GetKeyUp(KeyCode code)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetKeyUp(code);
    }

    public static bool GetButtonDown(string button, bool ignorePrevent = false)
    {
        if(!GameManager.Instance.HasKeyBind(button)) return false;
        foreach(var info in GameManager.Instance.GetKeyBind(button).KeyInfos)
        {
            if((ignorePrevent && Input.GetKeyDown(info.Code)) || GetKeyDown(info.Code)) return true;
        }
        return false;
    }

    public static bool GetButton(string button, bool ignorePrevent = false)
    {
        if(!GameManager.Instance.HasKeyBind(button)) return false;
        foreach(var info in GameManager.Instance.GetKeyBind(button).KeyInfos)
        {
            if((ignorePrevent && Input.GetKey(info.Code)) || GetKey(info.Code)) return true;
        }
        return false;
    }

    public static bool GetButtonUp(string button, bool ignorePrevent = false)
    {
        if(!GameManager.Instance.HasKeyBind(button)) return false;
        foreach(var info in GameManager.Instance.GetKeyBind(button).KeyInfos)
        {
            if((ignorePrevent && Input.GetKeyUp(info.Code)) || GetKeyUp(info.Code)) return true;
        }
        return false;
    }

    public static bool GetMouseButtonDown(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() 
                && Input.GetMouseButtonDown(btn) 
                && !EventSystem.current.IsPointerOverGameObject();
    }

    public static bool GetMouseButton(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() 
                && Input.GetMouseButton(btn) 
                && !EventSystem.current.IsPointerOverGameObject();
    }

    public static bool GetMouseButtonUp(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() 
                && Input.GetMouseButtonUp(btn) 
                && !EventSystem.current.IsPointerOverGameObject();
    }

    public static Vector2 MouseScrollDelta
    {
        get
        {
            return GameManager.Instance.CanUseKeyInput() ? Input.mouseScrollDelta : Vector2.zero;
        }
        private set {}
    }

    private static Vector2 _latestMousePos = Input.mousePosition;
    public static Vector2 MousePosition
    {
        get
        {
            if(!GameManager.Instance.CanUseKeyInput()) return _latestMousePos;
            _latestMousePos = Input.mousePosition;
            return _latestMousePos;
        }
        private set {}
    }
}