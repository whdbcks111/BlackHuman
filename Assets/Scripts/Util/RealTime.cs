using UnityEngine;

public class RealTime : MonoBehaviour
{
    private static RealTime s_instance;

    private float _deltaTime = 0f;
    private float _prev = 0f;

    private void Awake() {
        s_instance = this;
    }

    private void Update() {
        _deltaTime = Time.realtimeSinceStartup - _prev;
        _prev = Time.realtimeSinceStartup;
    }

    public static float deltaTime 
    {
        get
        {
            return s_instance._deltaTime;
        }
        private set {}
    }
}