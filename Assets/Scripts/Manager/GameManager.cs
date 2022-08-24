using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameObject _optionPanel, _gameOverPanel;
    [SerializeField]
    private TextMeshProUGUI _uptimeText;
    [SerializeField]
    private Slider _bgmVolume, _seVolume;

    private void Awake() {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        if(!PlayerPrefs.HasKey("Options.BGMVol")) PlayerPrefs.SetFloat("Options.BGMVol", SoundManager.Instance.BgmAudioSource.volume);
        if(!PlayerPrefs.HasKey("Options.SEVol")) PlayerPrefs.SetFloat("Options.SEVol", SoundManager.Instance.SeAudioSource.volume);
        _bgmVolume.value = PlayerPrefs.GetFloat("Options.BGMVol");
        _seVolume.value = PlayerPrefs.GetFloat("Options.SEVol");
        StartCoroutine(OptionSaveCoroutine());
    }

    private IEnumerator OptionSaveCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            SaveOptions();
        }
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetFloat("Options.BGMVol", SoundManager.Instance.BgmAudioSource.volume);
        PlayerPrefs.SetFloat("Options.SEVol", SoundManager.Instance.SeAudioSource.volume);
    }

    public void ResetGame()
    {
        SaveOptions();
        SceneManager.LoadSceneAsync(0);
    }

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.F11))
        {
            if(Screen.fullScreen)
            {
                Screen.SetResolution(1280, 720, false);
            }
            else
            {
                Screen.SetResolution(Screen.width, Screen.height, true);
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _optionPanel.SetActive(!IsPaused());
        }

        if(IsPaused()) Time.timeScale = 0f;
        else Time.timeScale = 1f;

        _uptimeText.SetText(string.Format("{0:D2}:{1:D2}", (int)Time.time / 60, (int)Time.time % 60));

        if(Player.Instance.IsDead && !_gameOverPanel.activeSelf)
        {
            _gameOverPanel.SetActive(true);
        }
        
        SoundManager.Instance.BgmAudioSource.volume = _bgmVolume.value;
        SoundManager.Instance.SeAudioSource.volume = _seVolume.value;
    }

    public bool IsPaused()
    {
        return _optionPanel.activeSelf;
    }

    public bool CanUseKeyInput()
    {
        return !_optionPanel.activeSelf && !_gameOverPanel.activeSelf; 
    }
}
