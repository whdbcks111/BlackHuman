using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour, InventoryHolder
{
    public static Player Instance { get; private set; }
    public const float DefaultMoveSpeed = 5F, DefaultHp = 100F, DefaultMp = 100F;
    public const byte HotbarSize = 5; 
    public static readonly KeyCode[] AlphaKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Rigidbody2D _rigid;
    [SerializeField]
    private BoxCollider2D _collider;
    [SerializeField]
    private GameObject _hpBarObject, _mpBarObject;
    [SerializeField]
    private TextMeshProUGUI _hpText, _mpText;
    [SerializeField]
    private Image _hpBarImg, _mpBarImg;
    [SerializeField]
    private GameObject _slotPrefab, _hotbar;
    [SerializeField]
    private RectTransform _slotSelector;

    private float _hp = DefaultHp, _mp = DefaultMp; 
    private string _curState;
    [HideInInspector]
    public float MoveSpeed = DefaultMoveSpeed;
    [HideInInspector]
    public float MaxHp = DefaultHp;
    [HideInInspector]
    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value, 0, MaxHp); } }
    [HideInInspector]
    public float MaxMp = DefaultMp;
    [HideInInspector]
    public float Mp { get { return _mp; } set { _mp = Mathf.Clamp(value, 0, MaxMp); } }
    private Vector2 _axis;
    private Vector2 _originalColSize, _originalColOffset;
    private readonly SlotInfo[] _slots = new SlotInfo[HotbarSize];
    public Inventory Inventory { get; private set; }
    private float _slotWidth;

    private void Awake()
    {
        Instance = this;
        Inventory = new Inventory(this);
        _originalColSize = _collider.size;
        _originalColOffset = _collider.offset;

        _slotWidth = _slotPrefab.GetComponent<RectTransform>().rect.width;
        var hotbarWidth = HotbarSize * _slotWidth;
        for(var i = 0; i < HotbarSize; i++) {
            var slot = Instantiate(_slotPrefab, _hotbar.transform);
            var rt = slot.GetComponent<RectTransform>();
            var p = rt.localPosition;
            p.x = hotbarWidth * -0.5f + (i + .5f) * _slotWidth;
            rt.localPosition = p;

            var itemImage = slot.transform.GetChild(0).GetComponent<Image>();
            var itemCount = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            itemImage.gameObject.SetActive(false);
            itemCount.gameObject.SetActive(false);

            _slots[i] = new(itemImage, itemCount);
        }

        Inventory.AddItemStack(new ItemStack(ItemType.Sword), 1);
    }

    private void Start()
    {
        ChangeAnimationState("PlayerIdle");
    }

    private void Update()
    {
        Move();
        ValueUpdate();
        UIUpdate();
    }

    private void ValueUpdate() {
        if(Hp > 0) Hp += 0.5f * Time.deltaTime;
        Mp += 1 * Time.deltaTime;
    }

    private void UIUpdate() {
        _hpText.SetText(string.Format("{0}%", (int)Mathf.Ceil(Hp)));
        _hpBarImg.fillAmount = Mathf.Clamp01(Hp / MaxHp);
        var hs = _hpBarObject.transform.localScale;
        hs.x = Mathf.Clamp01(Hp / MaxHp);
        _hpBarObject.transform.localScale = hs;

        _mpText.SetText(string.Format("{0}%", (int)Mathf.Ceil(Mp)));
        _mpBarImg.fillAmount = Mathf.Clamp01(Mp / MaxMp);
        var ms = _mpBarObject.transform.localScale;
        ms.x = Mathf.Clamp01(Mp / MaxMp);
        _mpBarObject.transform.localScale = ms;

        
        for(var i = 0; i < _slots.Length; i++) {
            var slotInfo = _slots[i];
            var itemStack = this.Inventory.GetItemStack(i);
            slotInfo.ItemCount.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.sprite = itemStack?.ItemType?.Sprite;
            slotInfo.ItemCount.SetText(itemStack?.Amount.ToString());
        }

        var scrollY = Input.mouseScrollDelta.y;
        if(scrollY != 0) {
            Inventory.SelectedSlot = (byte)((Inventory.SelectedSlot + (scrollY > 0 ? -1 : 1) + HotbarSize) % HotbarSize);
        }
        for(byte i = 0; i < Mathf.Min(HotbarSize, AlphaKeys.Length); i++) {
            if(Input.GetKeyDown(AlphaKeys[i])) Inventory.SelectedSlot = i;
        }
        
        _slotSelector.localPosition = Vector3.right * (HotbarSize * _slotWidth * -0.5f + (Inventory.SelectedSlot + .5f) * _slotWidth);
    }

    private void Move()
    {
        _axis = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) _axis += Vector2.up;
        if (Input.GetKey(KeyCode.A)) _axis += Vector2.left;
        if (Input.GetKey(KeyCode.S)) _axis += Vector2.down;
        if (Input.GetKey(KeyCode.D)) _axis += Vector2.right;


        if (Mathf.Abs(_axis.x) > 0.1f)
        {
            ChangeAnimationState(_axis.x > 0 ? "PlayerWalk_Right" : "PlayerWalk_Left");
            var s = _collider.size;
            s.x = _originalColSize.x * 0.43f;
            _collider.size = s;
            _collider.offset = _originalColOffset + Vector2.right * (_axis.x > 0 ? 1 : -1) * -0.07f;
        }
        else
        {
            _collider.size = _originalColSize;
            _collider.offset = _originalColOffset;
            if (Mathf.Abs(_axis.y) > 0.1f)
            {
                ChangeAnimationState(_axis.y > 0 ? "PlayerWalk_Up" : "PlayerWalk_Down");
            }
            else
            {
                ChangeAnimationState("PlayerIdle");
            }
        }

        var runSpeed = 1f;
        if(Input.GetKey(KeyCode.LeftShift)) {
            runSpeed = 1.5f;
            Mp -= 3 * Time.deltaTime;
        }

        _rigid.velocity = _axis.normalized * MoveSpeed * runSpeed;
    }

    private void ChangeAnimationState(string state)
    {
        if (_curState == state) return;

        _animator.Play(state);
        _curState = state;
    }

    private class SlotInfo {
        public Image ItemImage { get; private set; }
        public TextMeshProUGUI ItemCount { get; private set; }

        public SlotInfo(Image itemImage, TextMeshProUGUI itemCount)
        {
            ItemImage = itemImage;
            ItemCount = itemCount;
        }
    }
}
