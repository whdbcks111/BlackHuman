using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class KeyInput
{
    private static readonly KeyCode[] _keyCodes =
        System.Enum.GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(k => ((int)k < (int)KeyCode.Mouse0))
            .ToArray();
 
    public static IEnumerable<KeyCode> GetCurrentKeysDown()
    {
        if (Input.anyKeyDown)
            for (int i = 0; i < _keyCodes.Length; i++)
                if (Input.GetKeyDown(_keyCodes[i]))
                    yield return _keyCodes[i];
    }
 
    public static IEnumerable<KeyCode> GetCurrentKeys()
    {
        if (Input.anyKey)
            for (int i = 0; i < _keyCodes.Length; i++)
                if (Input.GetKey(_keyCodes[i]))
                    yield return _keyCodes[i];
    }
 
    public static IEnumerable<KeyCode> GetCurrentKeysUp()
    {
        for (int i = 0; i < _keyCodes.Length; i++)
            if (Input.GetKeyUp(_keyCodes[i]))
                yield return _keyCodes[i];
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

    public static bool GetMouseButtonDown(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetMouseButtonDown(btn);
    }

    public static bool GetMouseButton(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetMouseButton(btn);
    }

    public static bool GetMouseButtonUp(int btn)
    {
        return GameManager.Instance.CanUseKeyInput() && Input.GetMouseButtonUp(btn);
    }

    public static Vector2 MouseScrollDelta {
        get {
            return GameManager.Instance.CanUseKeyInput() ? Input.mouseScrollDelta : Vector2.zero;
        }
        private set  
        {    
        }
    }
}