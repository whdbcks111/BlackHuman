using UnityEngine;
using System.Collections;

public class TreasureBox : Block
{

    public readonly static Color GoldColor = new Color(1, .6f, 0);

    private bool _isOpened = false;

    protected override void Update()
    {
        Life = Mathf.Max(Life, 1);
        base.Update();
        if (Life <= 1 && !_isOpened)
        {
            GetComponent<Animator>().SetTrigger("Open");
            _isOpened = true;
            StartCoroutine(OpenCoroutine());
        }
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.MaxLife, 80f);
    }

    public override void Hit(Attribute attribute, DamageType type = DamageType.Normal)
    {
        base.Hit(attribute, type);
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.HorizontalExplode, GoldColor, 1, 0, 4);
    }

    private IEnumerator OpenCoroutine()
    {
        yield return new WaitForSeconds(.6f);
        ParticleManager.Instance.SpawnParticle(transform.position, ParticleType.Smoke, GoldColor, 1, 0, 40);
        Destroy(gameObject);
        var types = ItemType.GetAll<ItemType>();
        ItemType type = null;
        for(var i = 0; i < 10 && (Player.Instance.Inventory.HasItem(type) || type == null); i++)
        {
            type = types[Random.Range(0, types.Count)];
        }
        if(type != null) Player.Instance.Inventory.AddItemStack(new(type));
    }
}