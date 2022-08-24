using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

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
    private GameObject _slotPrefab, _hotbar;
    [SerializeField]
    private RectTransform _slotSelector;
    [SerializeField]
    private Animator _sweepAnimator;
    [SerializeField]
    private GameObject _floatingItem;
    [SerializeField]
    private SpriteRenderer _floatingItemRenderer, _sweepRenderer;
    public Image BossBarBack, BossBar;

    private readonly SlotInfo[] _slots = new SlotInfo[HotbarSize];
    public readonly Inventory Inventory = new();
    private float _slotWidth;

    public Room CurrentRoom;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
        Name = "Player";

        if(s_passingLayer == -1) s_passingLayer = LayerMask.NameToLayer("Passing");
        if(s_playerLayer == -1) s_playerLayer = LayerMask.NameToLayer("Player");

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
        _floatingItem.SetActive(false);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UIUpdate();
        InventoryUpdate();
    }

    public override void Damage(Attribute attribute, DamageType type = DamageType.Normal, bool isCritical = false)
    {
        base.Damage(attribute, type, isCritical);
        if(!IsDead) Camera.main.gameObject.GetComponent<CameraMove>().Shake(.3f, .2f);
    }

    public override void ShowDamageEffect()
    {
        base.ShowDamageEffect();
        gameObject.layer = s_passingLayer;
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
        print("attack start");
        var mask = ~LayerMask.GetMask("Passing");
        HashSet<Damageable> alreadyDamaged = new();
        latestAttack = Time.time;

        var beforeRot = _floatingItem.transform.rotation.eulerAngles.z;

        for(var i = 0f; i < 0.4f; i += Time.deltaTime)
        {
            yield return null;
            var rot = (_floatingItem.transform.rotation.eulerAngles.z + 90) % 360;

            var maxRot = Mathf.Max(rot, beforeRot);
            var minRot = Mathf.Min(rot, beforeRot);
            
            var cols = Physics2D.OverlapCircleAll(transform.position, radius, mask);
            foreach(var col in cols)
            {
                var colAxis = col.gameObject.transform.position - transform.position;
                var colRot = Mathf.Atan2(colAxis.y, colAxis.x) * Mathf.Rad2Deg;

                if(!ExtraMath.IsAngleBetween(colRot, minRot, maxRot)) continue;

                if(targetTags != null && !targetTags.Contains(col.gameObject.tag)) continue;
                Damageable damageable = col.gameObject.GetComponent<Damageable>();
                if(damageable == this || damageable == null) continue;
                if(alreadyDamaged.Contains(damageable)) continue;
                Attack(damageable, Attribute, type, knockback);
                alreadyDamaged.Add(damageable);
            }

            beforeRot = rot;
        }
        print("attack end");
    }

    public void DisplayFloatingItem(Sprite sprite, float angle=0, float itemAngle=0) {
        _floatingItem.SetActive(true);
        _floatingItem.transform.rotation = Quaternion.Euler(0, 0, angle);
        _floatingItemRenderer.sprite = sprite;
        _floatingItemRenderer.gameObject.transform.rotation = Quaternion.Euler(0, 0, itemAngle);
    }

    private void InventoryUpdate()
    {
        if(IsDead) return;
        for (var i = 0; i < _slots.Length; i++)
        {
            var slotInfo = _slots[i];
            var itemStack = this.Inventory.GetItemStack(i);
            slotInfo.ItemCount.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.gameObject.SetActive(itemStack != null);
            slotInfo.ItemImage.sprite = itemStack?.ItemType?.Sprite;
            slotInfo.ItemCount.SetText(itemStack?.Amount.ToString());
        }

        var scrollY = KeyInput.MouseScrollDelta.y;
        if (scrollY != 0)
        {
            Inventory.SelectedSlot = (byte)((Inventory.SelectedSlot + (scrollY > 0 ? -1 : 1) + HotbarSize) % HotbarSize);
        }
        for (byte i = 0; i < Mathf.Min(HotbarSize, AlphaKeys.Length); i++)
        {
            if (KeyInput.GetKeyDown(AlphaKeys[i])) Inventory.SelectedSlot = i;
        }

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
        if (KeyInput.GetKey(KeyCode.W)) axis += Vector2.up;
        if (KeyInput.GetKey(KeyCode.A)) axis += Vector2.left;
        if (KeyInput.GetKey(KeyCode.S)) axis += Vector2.down;
        if (KeyInput.GetKey(KeyCode.D)) axis += Vector2.right;

        var maxStamina = Attribute.GetValue(AttributeType.MaxStamina);

        if (KeyInput.GetKey(KeyCode.LeftShift) && Stamina >= 20)
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