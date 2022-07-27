using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class MapManager : MonoBehaviour
{
    
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private SerializableDictionary<TileBase, float> _breakableTypes;

    private Dictionary<Vector2Int, BlockData> _blockDataMap = new();

    private void Awake() {
        foreach(var v3 in _tilemap.cellBounds.allPositionsWithin) {
            var v2 = (Vector2Int)v3;
            var type = _tilemap.GetTile(v3);
            if(type == null) continue;
            if(_breakableTypes.ContainsKey(type)) _blockDataMap[v2] = new BreakableBlockData(v2, type, _breakableTypes[type]);
            else _blockDataMap[v2] = new BlockData(v2, type);
        }
    }

    public void SetBlock(BlockData data) {
         _blockDataMap[(Vector2Int)data.Pos] = data;
        _tilemap.SetTile((Vector3Int)data.Pos, data.Type);
    }

    public void RemoveBlock(Vector2Int pos) {
         _blockDataMap[pos] = null;
        _tilemap.SetTile((Vector3Int)pos, null);
    }

    public Vector3Int GetCellPos(Vector3 pos) {
        return _tilemap.WorldToCell(pos);
    }

    public Vector3 GetWorldPos(Vector3Int pos) {
        return _tilemap.CellToWorld(pos);
    }

    
    public class BlockData {
        public Vector2Int Pos {get; private set;}
        public TileBase Type {get; private set;}
        
        public BlockData(Vector2Int position, TileBase type) {
            Pos = position;
            Type = type;
        }
    }

    public class BreakableBlockData : BlockData {
        public float MaxDurability;
        private float _durability;
        public float Durability {
            get { return _durability; } 
            set { _durability = Mathf.Clamp(value, 0, MaxDurability); } 
        }

        public BreakableBlockData(Vector2Int pos, TileBase type, float durability): base(pos, type) {
            MaxDurability = Durability = durability;
        }

    }
}
