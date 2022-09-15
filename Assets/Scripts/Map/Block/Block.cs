using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Block : Damageable
{

    public static Dictionary<Vector2Int, Block> Blocks { get { return GameManager.Instance.Blocks; } }
    
    [SerializeField]
    private float _defaultLife = 200f;

    private Vector2Int _pos;
    public Vector2Int Pos { 
        get {
            return _pos;
        } 
        set {
            if(Storage.Get<Tilemap>("FloorTilemap").GetTile((Vector3Int)value) == null) return;
            Blocks.Remove(_pos);
            Blocks[value] = this;
            _pos = value;
            transform.position = (Vector2)Pos + Vector2.one * 0.5f;
        }
    }

    protected GameObject lifeBack;
    protected Image lifeBarImg;
    private GameObject _icon;

    public static void ClearAllBlocks()
    {
        foreach(var b in Blocks.Values) Destroy(b.gameObject);
    }

    protected override void Awake() {
        base.Awake();
        Blocks[Pos] = this;

        lifeBack = Instantiate(Resources.Load<GameObject>("Blocks/BlockLifeBar"), transform);
        lifeBarImg = lifeBack.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        _icon = Instantiate(Resources.Load<GameObject>("Blocks/BlockIcon"), transform);
    }

    protected override void InitializeDefaults()
    {
        base.InitializeDefaults();
        Attribute.SetDefaultValue(AttributeType.LifeRegen, 0f);
        Attribute.SetDefaultValue(AttributeType.MaxLife, _defaultLife);
    }

    protected override void Start() {
        base.Start();
        Pos = (Vector2Int)Vector3Int.FloorToInt(Storage.Get<Tilemap>("FloorTilemap").CellToWorld(Vector3Int.FloorToInt(transform.position)));
    }

    public static Block SetBlock(Vector2Int pos, GameObject block)
    {
        if(Storage.Get<Tilemap>("FloorTilemap").GetTile((Vector3Int)pos) == null) return null;
        if(Blocks.ContainsKey(pos)) Destroy(Blocks[pos].gameObject);
        var newBlock = Instantiate(block, Storage.Get("BlockContainer").transform).GetComponent<Block>();
        newBlock.Pos = pos;
        return newBlock;
    }

    public static Block SetBlock(Vector2Int pos, Block block)
    {
        return SetBlock(pos, block.gameObject);
    }

    public static Block SetBlock(Vector2Int pos, string blockName)
    {
        return SetBlock(pos, Resources.Load<GameObject>("Blocks/" + blockName));
    }

    protected virtual void Update()
    {
        DisplayUpdate();

        if(Life <= 0) Destroy(gameObject);
    }

    protected virtual void OnDestroy() {
        Blocks.Remove(Pos);
    }

    protected virtual void DisplayUpdate()
    {
        lifeBarImg.fillAmount = Life / Attribute.GetValue(AttributeType.MaxLife);
        lifeBack.SetActive(lifeBarImg.fillAmount < 1);
    }

    protected override IEnumerator ShowDamageEffectCoroutine(float time)
    {
        yield return null;
    }
}
