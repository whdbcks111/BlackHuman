using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private void Start() {
        SoundManager.Instance.PlayMusic("Theme_1");    
    }
}
