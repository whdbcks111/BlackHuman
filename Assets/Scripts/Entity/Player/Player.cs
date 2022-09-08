using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class Player : LivingEntity
{
    private static int s_playerLayer = -1, s_passingLayer = -1;

    public static Player Instance { get; private set; }
    public const byte HotbarSize = 5;
    public static readonly KeyCode[] AlphaKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

    [SerializeField]
    private Image _miniLifeBarImg, _miniManaBarImg;
    [SerializeField]
    private Image _lifeBarImg, _manaBarImg, _staminaBarImg;
    [SerializeField]
    private GameObject _hotbar, _invContents;
    [SerializeField]
    private RectTransform _slotSelector;
    [SerializeField]
    private Animator _sweepAnimator;
    [SerializeField]
    private GameObject _floatingItem;
    [SerializeField]
    private SpriteRenderer _floatingItemRenderer, _sweepRenderer;
    [SerializeField]
    private RectTransform _pointerHoldItem;
    [SerializeField]
    private Image _pointerHoldItemImage;
    [SerializeField]
    private TextMeshProUGUI _pointerHoldItemCount;
    [SerializeField]
    private TextMeshProUGUI _goldText;
    [SerializeField]
    private Transform _effectsContent;
    [SerializeField]
    private TextMeshProUGUI _actionbar;
    public Image BossBarBack, BossBar;

    private readonly SlotInfo[] _slots = new SlotInfo[HotbarSize];
    public readonly Inventory Inventory = new();
    private readonly SlotInfo[] _inventorySlots = new SlotInfo[Inventory.LineCount * HotbarSize];
    private float _slotWidth;
    private GameObject _slotPrefab, _invSlotPrefab, _effectIconPrefab;

    [HideInInspector]
    public Room CurrentRoom;
    [HideInInspector]
    public int GoldAmount = 0;
    private bool _floatingItemVisible = false;
    private float _latestDash = -1f;

    private Dictionary<string, ActionBarText> _actionBarTexts = new();

    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        if(s_passingLayer == -1) s_passingLayer = LayerMask.NameToLayer("Passing");
        if(s_playerLayer == -1) s_playerLayer = LayerMask.NameToLayer("Player");

        _slotPrefab = Resources.Load<GameObject>("UI/Slot");
        _invSlotPrefab = Resources.Load<GameObject>("UI/InventorySlot");
        _effectIconPrefab = Resources.Load<GameObject>("UI/Effect");
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

        _slotSelector.SetAsLastSibling();

        for (var i = 0; i < HotbarSize * Inventory.LineCount; i++)
        {
            var slot = Instantiate(_invSlotPrefab, _invContents.transform);
            var itemImage = slot.transform.GetChild(1).GetComponent<Image>();
            var itemCount = slot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var trigger = slot.GetComponent<InventorySlot>();
            trigger.SlotId = i;

            itemImage.gameObject.SetActive(false);
            itemCount.gameObject.SetActive(false);

            _inventorySlots[i] = new(itemImage, itemCount);
        }

        Inventory.AddItemStack(new(ItemType.MagicWand, 1));
        Inventory.AddItemStack(new(ItemType.HealingPotion, 3));
        Inventory.AddItemStack(new(ItemType.ManaPotion, 10));
    }

    public void ShowActionBar(string key, string text, float duration = 0.5f)
    {
        _actionBarTexts[key] = new(text, duration);
    }

    public void OnSlotClick(int slot)
    {
        var heldItem = Inventory.PointerHoldItem;
        Inventory.PointerHoldItem = Inventory.GetItemStack(slot);
        Inventory.SetItemStack(slot, heldItem);
    }

    public void OnSlotHoverExit(int slot)
    {
    }

    public void OnSlotHoverEnter(int slot)
    {
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MoveSpeed, 6);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEarlyUpdate()
    {
        base.OnEarlyUpdate();
        _floatingItemVisible = false;
    }

    protected override void OnLateUpdate()
    {
        base.OnLateUpdate();
        _floatingItem.SetActive(_floatingItemVisible);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UIUpdate();
        InventoryUpdate();
        KeyUpdate();
    }

    private void KeyUpdate()
    {
        if(KeyInput.GetButtonDown("Interact"))
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, 0.6f);
            foreach(var collider in colliders)
            {
                if(collider.CompareTag("Interactable"))
                {
                    collider.gameObject.GetComponent<IInteractable>()?.Interact();
                }
            }
        }
    }

    public override Effect AddEffect(EffectType type, int level, float duration, LivingEntity caster)
    {
        var eff = base.AddEffect(type, level, duration, caster);
        if(eff == null) return null;
        var icon = Instantiate(_effectIconPrefab, _effectsContent);
        var iconSprite = Resources.Load<Sprite>("EffectIcons/" + type.Name);
        icon.transform.GetChild(1).GetComponent<Image>().sprite = iconSprite;
        var iconFrontImage = icon.transform.GetChild(2).GetComponent<Image>();
        iconFrontImage.sprite = iconSprite;
        eff.EffectIcon = iconFrontImage;
        return eff;
    }

    public override void RemoveEffect(Effect eff)
    {
        base.RemoveEffect(eff);
        Destroy(eff.EffectIcon.transform.parent.gameObject);
    }

    public override void Hit(Attribute attribute, DamageType type = DamageType.Normal)
    {
        base.Hit(attribute, type);
        if(!IsDead) Camera.main.gameObject.GetComponent<CameraMove>().Shake(.3f, .2f);
    }

    public override void ShowDamageEffect()
    {
        base.ShowDamageEffect();
        gameObject.layer = s_passingLayer;
    }

    protected override void EffectUpdate()
    {
        base.EffectUpdate();
        foreach(var eff in effects)
        {
            eff.EffectIcon.fillAmount = Mathf.Clamp01(eff.Duration / eff.MaxDuration);
        }
    }

    protected override IEnumerator ShowDamageEffectCoroutine(float time)
    {
        yield return base.ShowDamageEffectCoroutine(time);
        gameObject.layer = s_playerLayer;
    }

    public void SweepAttackNearby(float radius, int count = 1, DamageType type = DamageType.Normal, 
            bool knockback = true, string[] targetTags = null)
    {
        if(!IsAttackEnded()) return;
        StartCoroutine(SweepAttackNearbyC( radius, count, type, knockback, targetTags ));
    }

    private IEnumerator SweepAttackNearbyC(float radius, int count, DamageType type, 
            bool knockback, string[] targetTags)
    {
        var mask = ~LayerMask.GetMask("Passing");
        HashSet<Damageable> alreadyDamaged = new();
        latestAttack = Time.time;

        var beforeRot = _floatingItem.transform.rotation.eulerAngles.z + 90;

        for(var i = 0f; i < 0.33f; i += Time.deltaTime)
        {
            yield return null;
            var rot = _floatingItem.transform.rotation.eulerAngles.z + 90;
            
            foreach(var angle in ExtraMath.GetAnglePoints(beforeRot, rot, 4))
            {
                var axis = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                var hits = Physics2D.RaycastAll(transform.position, axis, radius, mask);
                foreach(var hit in hits)
                {
                    if(targetTags != null && !targetTags.Contains(hit.collider.gameObject.tag)) continue;
                    Damageable damageable = hit.collider.gameObject.GetComponent<Damageable>();
                    if(damageable == this || damageable == null) continue;
                    if(alreadyDamaged.Contains(damageable)) continue;
                    Attack(damageable, Attribute, type, knockback);
                    alreadyDamaged.Add(damageable);
                }
            }

            beforeRot = rot;
        }
    }

    public void DisplayFloatingItem(Sprite sprite, float angle=0, float itemAngle=0, float distance=3.4f) {
        _floatingItemVisible = true;
        _floatingItem.transform.GetChild(0).localPosition = new Vector3(0, distance - 1.3f, 0);
        _floatingItem.transform.rotation = Quaternion.Euler(0, 0, angle);
        _floatingItemRenderer.sprite = sprite;
        _floatingItemRenderer.gameObject.transform.rotation = Quaternion.Euler(0, 0, itemAngle);
    }

    private void InventoryUpdate()
    {
        if(IsDead) return;

        if(Inventory.PointerHoldItem != null)
        {
            if(!GameManager.Instance.IsInventoryViewOpened())
            {
                Inventory.AddItemStack(Inventory.PointerHoldItem);
                Inventory.PointerHoldItem = null;
            }
            else
            {
                _pointerHoldItem.position = Camera.main.ScreenToViewportPoint(KeyInput.MousePosition) * new Vector2(Screen.width, Screen.height);
                _pointerHoldItemImage.sprite = Inventory.PointerHoldItem.ItemType.Sprite;
                _pointerHoldItemCount.SetText(Inventory.PointerHoldItem.Amount.ToString());
            }
        }
        _pointerHoldItem.gameObject.SetActive(Inventory.PointerHoldItem != null);

        for (var i = 0; i < _slots.Length; i++)
        {
            var slotInfo = _slots[i];
            var itemStack = this.Inventory.GetItemStack(i);
            slotInfo.ItemCount.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.sprite = itemStack?.ItemType?.Sprite;
            slotInfo.ItemCount.SetText(itemStack?.Amount.ToString());
        }

        for (var i = 0; i < _inventorySlots.Length; i++)
        {
            var slotInfo = _inventorySlots[i];
            var itemStack = this.Inventory.GetItemStack(i);
            slotInfo.ItemCount.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.sprite = itemStack?.ItemType?.Sprite;
            slotInfo.ItemCount.SetText(itemStack?.Amount.ToString());
        }
    
        var scrollY = KeyInput.MouseScrollDelta.y;
        var beforeSlot = Inventory.SelectedSlot;
        if (scrollY != 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            Inventory.SelectedSlot = (byte)((Inventory.SelectedSlot + (scrollY > 0 ? -1 : 1) + HotbarSize) % HotbarSize);
        }
        for (byte i = 0; i < Mathf.Min(HotbarSize, AlphaKeys.Length); i++)
        {
            if (KeyInput.GetKeyDown(AlphaKeys[i])) Inventory.SelectedSlot = i;
        }
        if(beforeSlot != Inventory.SelectedSlot) 
                ShowActionBar("SelectedItemName", Inventory.GetItemStack(Inventory.SelectedSlot)?.ItemType.DisplayName, 1f);

        _slotSelector.localPosition = Vector3.right * (HotbarSize * _slotWidth * -0.5f + (Inventory.SelectedSlot + .5f) * _slotWidth);

        for(var i = 0; i <= 1; i++) {
            if(KeyInput.GetMouseButton(i)) {
                Inventory.UseItem(Inventory.SelectedSlot, i);
            }
        }

        Inventory.Update();
    }

    private void UIUpdate()
    {
        var MaxLife = Attribute.GetValue(AttributeType.MaxLife);
        var MaxMana = Attribute.GetValue(AttributeType.MaxMana);
        var MaxStamina = Attribute.GetValue(AttributeType.MaxStamina);

        _lifeBarImg.fillAmount = Mathf.Clamp01(Life / MaxLife);
        _miniLifeBarImg.fillAmount = Mathf.Clamp01(Life / MaxLife);

        _manaBarImg.fillAmount = Mathf.Clamp01(Mana / MaxMana);
        _miniManaBarImg.fillAmount = Mathf.Clamp01(Mana / MaxMana);

        _staminaBarImg.fillAmount = Mathf.Clamp01(Stamina / MaxStamina);

        _goldText.SetText(GoldAmount.ToString());

        List<String> actionBarContents = new(), removeList = new();
        foreach(var actionBarText in _actionBarTexts)
        {
            actionBarContents.Add(actionBarText.Value.Content);
            if((actionBarText.Value.Duration -= Time.deltaTime) <= 0) removeList.Add(actionBarText.Key);
        }
        foreach(var key in removeList) _actionBarTexts.Remove(key);
        _actionbar.SetText(string.Join("\n", actionBarContents));
    }

    public void Sweep(float distance=2.6f) {
        Sweep(Color.white);
    }

    public void Sweep(Color c, float distance=2.6f) {
        if(IsSweeping()) return;
        _sweepRenderer.gameObject.transform.localScale = Vector2.one * distance / 2.6f * 1.5f;
        _sweepRenderer.color = c;
        _sweepAnimator.gameObject.transform.rotation = _floatingItem.gameObject.transform.rotation;
        _sweepAnimator.gameObject.SetActive(true);
        _sweepAnimator.SetTrigger("Sweep");
        StartCoroutine(nameof(StopSweep));
    }

    public bool IsSweeping() {
        return _sweepAnimator.gameObject.activeSelf;
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

    public GameObject GetFloatingItem()
    {
        return _floatingItem.transform.GetChild(0).gameObject;
    }

    protected override void Move()
    {
        if (KeyInput.GetButton("MoveUp")) axis += Vector2.up;
        if (KeyInput.GetButton("MoveLeft")) axis += Vector2.left;
        if (KeyInput.GetButton("MoveDown")) axis += Vector2.down;
        if (KeyInput.GetButton("MoveRight")) axis += Vector2.right;
        if ((_latestDash < 0f || Time.time - _latestDash > 1f) 
                && KeyInput.GetButton("Dash") && axis.magnitude > 0f && Stamina > 50f) 
        {
            _latestDash = Time.time;
            Stamina -= 30f;
            AddForce(axis, 8f);
        } 

        var maxStamina = Attribute.GetValue(AttributeType.MaxStamina);

        if (KeyInput.GetButton("Sprint") && Stamina >= 20)
        {
            Attribute.AddModifier(new(AttributeType.MoveSpeed, AttributeModifier.Type.Multiply, 1.5f));
            Stamina -= (Attribute.GetDefaultValue(AttributeType.StaminaRegen) + 7) * Time.deltaTime;
            if(Stamina < maxStamina * .2f) {
                Stamina = 0f;
            }
        }

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

public class ActionBarText
{
    public float Duration;
    public string Content;

    public ActionBarText(string content, float duration)
    {
        Duration = duration;
        Content = content;
    }
}