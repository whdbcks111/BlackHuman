using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

// Project Settings에서 GameManager의 실행 순서를 -1로 설정함
public class GameManager : MonoBehaviour
{
    private GameObject _keyChangeBoxPrefab;
    private GameObject _keyButtonPrefab;
    private Transform _interactableContainer;

    public static GameManager Instance { get; private set; }

    [SerializeField]
    public Image StageClearPanel;
    [SerializeField]
    public RectTransform PointerHoldItem;
    [SerializeField]
    public Image PointerHoldItemImage;
    [SerializeField]
    public TextMeshProUGUI PointerHoldItemCount;
    [SerializeField]
    public GameObject ItemInfo, InvContents;
    [SerializeField]
    private GameObject _gotoTitleBtn, _resetBtn;
    [SerializeField]
    public TextMeshProUGUI ItemInfoName, ItemInfoDescription;
    [SerializeField]
    public Image ItemInfoImage;
    [SerializeField]
    private GameObject _optionPanel, _gameOverPanel, _inventoryView;
    [SerializeField]
    private TextMeshProUGUI _uptimeText;
    [SerializeField]
    private Slider _bgmVolume, _seVolume;
    [SerializeField]
    private TextMeshProUGUI _gameOverInfo;
    [SerializeField]
    private Transform _keyBoxesContainer;
    [SerializeField]
    private TextMeshProUGUI _keyInputAlert, _stageInfo;
    [SerializeField]
    private Toggle _displayDamageToggle;
    [SerializeField]
    private SerializableDictionary<Button, GameObject> _optionCategories;
    [Space]
    [SerializeField]
    public KeyBind[] DefaultKeyBinds;
    private SerializableDictionary<string, KeyBind> _keyBinds = new();

    private Dictionary<Button, TextMeshProUGUI> _buttonTextMap = new();
    public int Stage { get; private set; }

    private string _inputAlertStr = "";
    private float _inputAlertTime = 0f;
    private bool _isKeyInputting = false;

    private Button _defaultOptionCategory;
    private IEnumerator _editKeyCoroutine = null;
    private float _bgmVolMultiplier = 1f;

    [HideInInspector]
    public Dictionary<Vector2Int, Block> Blocks = new();
    [HideInInspector]
    public List<IInteractable> Interactables = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Stage = 1;
        _keyChangeBoxPrefab = Resources.Load<GameObject>("UI/KeyChangeBox");
        _keyButtonPrefab = Resources.Load<GameObject>("UI/Key");

        foreach (var btn in _optionCategories.Keys)
        {
            _buttonTextMap[btn] = btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            btn.onClick.AddListener(() =>
            {
                foreach (var panel in _optionCategories.Values)
                {
                    panel.SetActive(panel == _optionCategories[btn]);
                }
            });

            if (_optionCategories[btn].activeSelf)
            {
                _defaultOptionCategory = btn;
            }
        }
    }

    private void Start()
    {

        foreach (var bind in DefaultKeyBinds)
        {
            _keyBinds[bind.Name] = bind;
        }

        if (PlayerPrefs.HasKey("Options.KeyBinds"))
        {
            try
            {
                var savedBinds = JsonUtility.FromJson<SerializableDictionary<string, KeyBind>>(PlayerPrefs.GetString("Options.KeyBinds"));
                foreach (var key in savedBinds.Keys)
                {
                    // DB 호환 오류로 값에 공백이 들어있다면, 기본값으로 대체 ( 다음 분기로 이동 )
                    if(savedBinds[key].Name.Length == 0 || 
                            savedBinds[key].DisplayName.Length == 0 || 
                            savedBinds[key].KeyInfos.Count == 0) continue;
                    // 삭제 불가하며 수정 불가한 키가 저장된 키에 없다면, 추가한 후 적용
                    foreach (var info in _keyBinds[key].KeyInfos)
                    {
                        if(!info.IsRemovable && !info.IsEditable && 
                                savedBinds[key].KeyInfos.Find(i => 
                                    i.Code == info.Code && !i.IsEditable && !i.IsRemovable
                                ) == null)
                        {
                            savedBinds[key].KeyInfos.Add(info);
                        }
                    }
                    savedBinds[key].Name = key;
                    _keyBinds[key] = savedBinds[key];
                }
            }
            catch (Exception)
            {
            }
        }

        // 인스펙터에서 설정한 키의 순서를 보장하기 위해 _keyBinds.Values를 순회하지 않고 DefaultKeyBinds를 순회.
        foreach (var _bind in DefaultKeyBinds)
        {
            var bind = _keyBinds[_bind.Name];
            var keyChangeBox = Instantiate(_keyChangeBoxPrefab, _keyBoxesContainer);
            var buttonNameText = keyChangeBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var keyAddBtn = keyChangeBox.transform.GetChild(2).GetComponent<Button>();
            buttonNameText.SetText(bind.DisplayName);

            var keysContainer = keyChangeBox.transform.GetChild(1).GetChild(0).GetChild(0);
            foreach (var info in bind.KeyInfos)
            {
                CreateNewKeyButton(keysContainer, info, bind);
            }

            keyAddBtn.onClick.AddListener(() =>
            {
                if(bind.KeyInfos.Count >= 3 || _isKeyInputting) return;
                var newInfo = new BindKeyInfo();
                newInfo.Code = KeyCode.None;
                newInfo.IsEditable = true;
                newInfo.IsRemovable = true;
                bind.KeyInfos.Add(newInfo);

                var keyBtn = CreateNewKeyButton(keysContainer, newInfo, bind);
                var keyNameText = keyBtn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                StartEditKey(bind, newInfo, keyNameText, keyBtn);
            });
        }

        if (!PlayerPrefs.HasKey("Options.BGMVol")) PlayerPrefs.SetFloat("Options.BGMVol", SoundManager.Instance.BgmAudioSource.volume);
        if (!PlayerPrefs.HasKey("Options.SEVol")) PlayerPrefs.SetFloat("Options.SEVol", SoundManager.Instance.SeAudioSource.volume);
        if (!PlayerPrefs.HasKey("Options.DisplayDamage")) PlayerPrefs.SetInt("Options.DisplayDamage", _displayDamageToggle.isOn ? 1 : 0);
        _bgmVolume.value = PlayerPrefs.GetFloat("Options.BGMVol");
        _seVolume.value = PlayerPrefs.GetFloat("Options.SEVol");
        _displayDamageToggle.isOn = PlayerPrefs.GetInt("Options.DisplayDamage") != 0;

        StartCoroutine(OptionSaveCoroutine());

    }

    public void GotoTitle()
    {
        SceneManager.LoadScene("TitleScene");
        _optionPanel.SetActive(false);
    }

    public void AddInteractable(IInteractable interactable)
    {
        Interactables.Add(interactable);
        if(interactable is MonoBehaviour script)
        {
            script.transform.SetParent(Storage.Get("InteractableContainer").transform);
        }
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        Interactables.Remove(interactable);
    }

    public void ClearAllInteractables()
    {
        Queue<GameObject> destroyTargets = new();
        foreach(var e in Interactables) 
            if(e is MonoBehaviour script) destroyTargets.Enqueue(script.gameObject);
        Interactables.Clear();
        foreach(var e in destroyTargets) DestroyImmediate(e.gameObject);
    }

    public void NextStage()
    {
        StartCoroutine(NextStageCoroutine());
        Stage++;
    }

    public void ResetStage()
    {
        SoundManager.Instance.ResetMusic();
        Player.Instance.SetForce(Vector2.zero, 0);
        Player.Instance.transform.position = Vector3.zero;
        Camera.main.transform.position = Vector3.back * 10;
        Block.ClearAllBlocks();
        ClearAllInteractables();
        ObjectPool.Instance.ClearAll();
        Npc.ClearAllNpcs();
        Enemy.ClearAllEnemies();
        MapGenerator.Instance.GenerateRooms((Stage - 1) * 5 + 9);
        _optionPanel.SetActive(false);
        _gameOverPanel.SetActive(false);
    }

    public void ResetGame()
    {
        Stage = 1;
        Player.Instance.Revive();
        ResetStage();
    }

    private IEnumerator NextStageCoroutine()
    {
        var panel = StageClearPanel;
        var col = panel.color;
        for(var i = 0f; i < 1f; i += Time.deltaTime / 0.5f) 
        {
            yield return null;
            col.a = i;
            panel.color = col;
            _bgmVolMultiplier = 1f - i;
        }
        _bgmVolMultiplier = 0f;
        col.a = 1;
        panel.color = col;
        yield return YieldCache.WaitForSecondsRealtime(0.5f);
        ResetStage();
        _bgmVolMultiplier = 1f;
        for(var i = 1f; i >= 0f; i -= Time.deltaTime / 0.5f) 
        {
            yield return null;
            col.a = i;
            panel.color = col;
        }
        col.a = 0;
        panel.color = col;
    }

    private GameObject CreateNewKeyButton(Transform keysContainer, BindKeyInfo info, KeyBind bind)
    {
        var keyBtn = Instantiate(_keyButtonPrefab, keysContainer);
        var keyNameText = keyBtn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        keyNameText.SetText(info.Code == KeyCode.None ? "..." : info.Code.ToString());

        var editBtn = keyBtn.transform.GetChild(2).GetChild(1).GetComponent<Button>();
        var removeBtn = keyBtn.transform.GetChild(2).GetChild(2).GetComponent<Button>();

        editBtn.interactable = info.IsEditable;
        editBtn.onClick.AddListener(() => 
        {
            if(!info.IsEditable) return;
            StartEditKey(bind, info, keyNameText, keyBtn);
        });

        removeBtn.interactable = info.IsRemovable;
        removeBtn.onClick.AddListener(() =>
        {
            if(!info.IsRemovable) return;
            _isKeyInputting = false;
            Destroy(keyBtn);
            bind.KeyInfos.Remove(info);
        });
        return keyBtn;
    }

    private void StartEditKey(KeyBind bind, BindKeyInfo info, TextMeshProUGUI text, GameObject keyBtn)
    {
        if(_editKeyCoroutine != null) StopCoroutine(_editKeyCoroutine);
        _editKeyCoroutine = EditKeyCoroutine(bind, info, text, keyBtn);
        StartCoroutine(_editKeyCoroutine);
    }

    private IEnumerator EditKeyCoroutine(KeyBind keyBind, BindKeyInfo info, TextMeshProUGUI text, GameObject keyBtn)
    {
        _isKeyInputting = true;
        _inputAlertStr = "키를 인식중입니다. 적용할 키를 입력해주세요.";
        while (_isKeyInputting)
        {
            yield return null;
            print("Inputting");
            _inputAlertTime = 0.1f;
            if (Input.anyKeyDown)
            {
                var inputList = KeyInput.GetCurrentKeysDown();
                if(inputList.Count <= 0) continue;
                var code = inputList[0];
                var alreadyExists = false;
                foreach (var bind in _keyBinds.Values)
                {
                    foreach (var keyInfo in bind.KeyInfos)
                    {
                        if (keyInfo.Code == code)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (alreadyExists) break;
                }
                if (alreadyExists)
                {
                    if(info.Code == KeyCode.None)
                    {
                        keyBind.KeyInfos.Remove(info);
                        Destroy(keyBtn);
                    }
                    _inputAlertStr = "<color=#ff4444>이미 입력된 키입니다.</color>";
                    _inputAlertTime = 1f;
                }
                else
                {
                    info.Code = code;
                    text.SetText(code.ToString());
                }
                break;
            }
        }
        _isKeyInputting = false;
    }

    public bool HasKeyBind(string name) => _keyBinds.ContainsKey(name);

    public bool CanDisplayDamage() => _displayDamageToggle.isOn;

    public KeyBind GetKeyBind(string name) => _keyBinds[name];

    private IEnumerator OptionSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            SaveOptions();
        }
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetFloat("Options.BGMVol", SoundManager.Instance.BgmAudioSource.volume);
        PlayerPrefs.SetFloat("Options.SEVol", SoundManager.Instance.SeAudioSource.volume);
        PlayerPrefs.SetString("Options.KeyBinds", JsonUtility.ToJson(_keyBinds));
        PlayerPrefs.SetInt("Options.DisplayDamage", _displayDamageToggle.isOn ? 1 : 0);
    }

    private void Update()
    {
        for(var i = 0; i < _keyBoxesContainer.childCount; i++)
        {
            var keyChangeBox = _keyBoxesContainer.GetChild(i);
            var keyAddBtn = keyChangeBox.transform.GetChild(2).GetComponent<Button>();
            var keysContainer = keyChangeBox.transform.GetChild(1).GetChild(0).GetChild(0);

            keyAddBtn.interactable = keysContainer.childCount < 3 && !_isKeyInputting;
        }

        if (_inputAlertTime > 0f)
        {
            _keyInputAlert.gameObject.SetActive(true);
            _keyInputAlert.SetText(_inputAlertStr);
            _inputAlertTime -= RealTime.deltaTime;
        }
        else _keyInputAlert.gameObject.SetActive(false);

        if (KeyInput.GetButtonDown("FullScreen", true) && !_isKeyInputting)
        {
            if (Screen.fullScreen)
            {
                Screen.SetResolution(1280, 720, false);
            }
            else
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
        }

        if (KeyInput.GetButtonDown("Pause", true) && !_isKeyInputting)
        {
            if (_inventoryView.activeSelf) _inventoryView.SetActive(false);
            else
            {
                if (!IsPaused())
                {
                    foreach (var category in _optionCategories.Values) category.SetActive(false);
                    _optionCategories[_defaultOptionCategory].SetActive(true);
                }
                _optionPanel.SetActive(!IsPaused());
            }
        }

        if (SceneManager.GetActiveScene().name == "GameScene" && KeyInput.GetButtonDown("Inventory", true) && !_isKeyInputting)
        {
            if (_optionPanel.activeSelf) _optionPanel.SetActive(false);
            else _inventoryView.SetActive(!_inventoryView.activeSelf);
        }

        if (IsPaused()) Time.timeScale = 0f;
        else Time.timeScale = 1f;

        _uptimeText.SetText(string.Format("{0:D2}:{1:D2}", (int)RealTime.time / 60, (int)RealTime.time % 60));

        if (Player.Instance != null && Player.Instance.IsDead && !_gameOverPanel.activeSelf)
        {
            GameOver();
        }

        SoundManager.Instance.BgmAudioSource.volume = _bgmVolume.value * _bgmVolMultiplier;
        SoundManager.Instance.SeAudioSource.volume = _seVolume.value;

        foreach (var btn in _optionCategories.Keys)
        {
            _buttonTextMap[btn].color = _optionCategories[btn].activeSelf ? Color.white : Color.gray;
        }

        _stageInfo.gameObject.SetActive(SceneManager.GetActiveScene().name == "GameScene");
        _resetBtn.gameObject.SetActive(SceneManager.GetActiveScene().name == "GameScene");
        _gotoTitleBtn.gameObject.SetActive(SceneManager.GetActiveScene().name == "GameScene");
        _stageInfo.SetText("스테이지 " + Stage);
    }

    public void GameOver()
    {
        _gameOverInfo.SetText("스테이지 - " + Stage + " (" + (int)(MapGenerator.Instance.GetCompleteLevel() * 100) + "%)");
        _gameOverPanel.SetActive(true);
    }

    public bool IsInventoryViewOpened() => _inventoryView.activeSelf;

    public bool IsPaused() => _optionPanel.activeSelf;

    public bool CanUseKeyInput() => !_optionPanel.activeSelf && !_gameOverPanel.activeSelf;
}


[Serializable]
public class KeyBind
{
    public string Name;
    public string DisplayName;
    public List<BindKeyInfo> KeyInfos;
}

[Serializable]
public class BindKeyInfo
{
    public KeyCode Code;
    public bool IsEditable;
    public bool IsRemovable;
}
