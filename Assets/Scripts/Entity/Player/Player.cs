using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : LivingEntity
{
    public static Player Instance { get; private set; }
    public const byte HotbarSize = 5;
    public static readonly KeyCode[] AlphaKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

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
    [SerializeField]
    private Animator _sweepAnimator;
    [SerializeField]
    private GameObject _floatingItem;
    [SerializeField]
    private SpriteRenderer _floatingItemRenderer, _sweepRenderer;

    private Vector2 _originalColSize, _originalColOffset;
    private readonly SlotInfo[] _slots = new SlotInfo[HotbarSize];
    public readonly Inventory Inventory = new();
    private float _slotWidth;

    protected override void Awake()
    {
        base.Awake();

        GetComponent<SpriteRenderer>();

        Instance = this;
        Name = "Player";
        _originalColSize = _collider.size;
        _originalColOffset = _collider.offset;

        _slotWidth = _slotPrefab.GetComponent<RectTransform>().rect.width;
        var hotbarWidth = HotbarSize * _slotWidth;
        for (var i = 0; i < HotbarSize; i++)
        {
            var slot = Instantiate(_slotPrefab, _hotbar.transform);
            var rt = slot.GetComponent<RectTransform>();
            var p = rt.localPosition;
            p.x = hotbarWidth * -0.5f + (i + .5f) * _slotWidth;
            rt.localPosition = p;

            var itemImage = slot.transform.GetChild(2).GetComponent<Image>();
            var itemCount = slot.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            itemImage.gameObject.SetActive(false);
            itemCount.gameObject.SetActive(false);

            _slots[i] = new(itemImage, itemCount);
        }

        _slotSelector.transform.SetAsLastSibling();

        Inventory.AddItemStack(new ItemStack(ItemType.Sword), 1);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UIUpdate();
        InventoryUpdate();
    }

    protected override void ResetValues() {
        base.ResetValues();
        MoveSpeed = 5.0f;
        _floatingItem.SetActive(false);
    }

    public override void Damage(float amount)
    {
        base.Damage(amount);
        Camera.main.gameObject.GetComponent<CameraMove>().Shake(.2f, .2f);
    }

    public void DisplayFloatingItem(Sprite sprite, float angle=0, float itemAngle=0) {
        _floatingItem.SetActive(true);
        _floatingItem.transform.rotation = Quaternion.Euler(0, 0, angle);
        _floatingItemRenderer.sprite = sprite;
        _floatingItemRenderer.gameObject.transform.rotation = Quaternion.Euler(0, 0, itemAngle);
    }

    private void InventoryUpdate()
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var slotInfo = _slots[i];
            var itemStack = this.Inventory.GetItemStack(i);
            slotInfo.ItemCount.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.sprite = itemStack?.ItemType?.Sprite;
            slotInfo.ItemCount.SetText(itemStack?.Amount.ToString());
        }

        var scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0)
        {
            Inventory.SelectedSlot = (byte)((Inventory.SelectedSlot + (scrollY > 0 ? -1 : 1) + HotbarSize) % HotbarSize);
        }
        for (byte i = 0; i < Mathf.Min(HotbarSize, AlphaKeys.Length); i++)
        {
            if (Input.GetKeyDown(AlphaKeys[i])) Inventory.SelectedSlot = i;
        }

        _slotSelector.localPosition = Vector3.right * (HotbarSize * _slotWidth * -0.5f + (Inventory.SelectedSlot + .5f) * _slotWidth);

        for(var i = 0; i <= 1; i++) {
            if(Input.GetMouseButtonDown(i)) {
                Inventory.UseItem(Inventory.SelectedSlot, i);
            }
        }

        Inventory.Update();
    }

    private void UIUpdate()
    {
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
    }

    public void Sweep() {
        Sweep(Color.white);
    }

    public void Sweep(Color c) {
        if(IsSweeping()) return;
        _sweepRenderer.color = c;
        _sweepAnimator.gameObject.transform.rotation = _floatingItem.gameObject.transform.rotation;
        _sweepAnimator.gameObject.SetActive(true);
        _sweepAnimator.SetTrigger("Sweep");
        StartCoroutine(nameof(StopSweep));
    }

    public bool IsSweeping() {
        return _sweepAnimator.gameObject.activeSelf;
    }

    public void AttackNearbyEnemy<E>(float radius, int maxEnemyCnt) {
        
    }

    private IEnumerator StopSweep() {
        var length  = _sweepAnimator.GetCurrentAnimatorStateInfo(0).length;
        var rot = _floatingItem.gameObject.transform.rotation.eulerAngles;
        rot.z -= 10f;
        while(length > 0) {
            yield return null;
            rot.z -= Time.deltaTime * 360 / 0.33f;
            _floatingItem.gameObject.transform.rotation = Quaternion.Euler(rot);
            length -= Time.deltaTime;
        }
        _sweepAnimator.gameObject.SetActive(false);
    }

    protected override void Move()
    {
        if (Input.GetKey(KeyCode.W)) axis += Vector2.up;
        if (Input.GetKey(KeyCode.A)) axis += Vector2.left;
        if (Input.GetKey(KeyCode.S)) axis += Vector2.down;
        if (Input.GetKey(KeyCode.D)) axis += Vector2.right;

        var runSpeed = 1f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            runSpeed = 1.5f;
            Mp -= 3 * Time.deltaTime;
        }

        if (curState == Name + "Walk_Left" || curState == Name + "Walk_Right")
        {
            var s = _collider.size;
            s.x = _originalColSize.x * 0.43f;
            _collider.size = s;
            _collider.offset = _originalColOffset + Vector2.right * (axis.x > 0 ? 1 : -1) * -0.07f;
        }
        else
        {
            _collider.size = _originalColSize;
            _collider.offset = _originalColOffset;
        }

        var originMoveSpeed = MoveSpeed;
        MoveSpeed *= runSpeed;

        base.Move();
    }

    private class SlotInfo
    {
        public Image ItemImage { get; private set; }
        public TextMeshProUGUI ItemCount { get; private set; }

        public SlotInfo(Image itemImage, TextMeshProUGUI itemCount)
        {
            ItemImage = itemImage;
            ItemCount = itemCount;
        }
    }
}
