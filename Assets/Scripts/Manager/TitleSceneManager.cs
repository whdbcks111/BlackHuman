using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _loading;

    public void GameStart()
    {
        _loading.SetActive(true);
        SceneManager.LoadSceneAsync("GameScene");
    }

    public void GameExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void Awake()
    {
        SoundManager.Instance.PlayMusic("TitleMusic");
    }
}
