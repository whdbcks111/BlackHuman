using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
