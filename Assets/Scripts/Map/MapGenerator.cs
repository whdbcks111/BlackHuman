using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{

    public readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    [SerializeField]
    private int _generalRoomSize, _bossRoomSize;
    [SerializeField]
    private Tilemap _wallTilemap, _floorTilemap, _minimapTilemap;
    [SerializeField]
    private RuleTile _wallRuleTile;
    [SerializeField]
    private TileBase[] _floorTiles;
    [SerializeField]
    private TileBase _minimapFloorTile;
    [SerializeField]
    private GameObject _roomTemp;

    private Dictionary<Vector2Int, Room> rooms = new();
    private bool _isFinalized = false;

    void Start()
    {
        GenerateRooms(10);
    }

    public void GenerateRooms(int roomCnt)
    {
        _isFinalized = false;
        rooms.Clear();
        _wallTilemap.ClearAllTiles();
        _floorTilemap.ClearAllTiles();
        var newRoom = new Room(0, Vector2Int.zero, null);
        rooms.Add(Vector2Int.zero, newRoom);
        TryCreateTransition(newRoom, roomCnt);
        GenerateFinalize();
    }

    private void TryCreateTransition(Room room, int roomCnt)
    {
        if (room.TotalLength >= roomCnt * .35f) {
            print("trasition end (long)");
            return;
        }
        var cnt = Random.Range(1, 4);
        if (rooms.Count <= 1) cnt = 4;
        List<Vector2Int> directions = new(Directions);
        while (cnt > 0)
        {
            if (roomCnt <= rooms.Count)
            {
                return;
            }
            var idx = Random.Range(0, directions.Count);
            var dir = directions[idx];
            directions.RemoveAt(idx);
            cnt--;

            var pos = room.Pos + dir * _bossRoomSize;
            if (rooms.ContainsKey(pos)) continue;
            var newRoom = new Room(room.TotalLength + 1, pos, room);
            rooms.Add(pos, newRoom);
            room.Children.Add(newRoom);
            TryCreateTransition(newRoom, roomCnt);
        }
    }

    private void GenerateFinalize()
    {
        if (_isFinalized) return;
        _isFinalized = true;

        Room farthestRoom = null;
        foreach (var room in rooms.Values)
        {
            if (farthestRoom == null 
                    || farthestRoom.TotalLength < room.TotalLength
                    || (farthestRoom.TotalLength == room.TotalLength && room.Pos.magnitude > farthestRoom.Pos.magnitude)) 
                farthestRoom = room;
        }
        farthestRoom.IsBossRoom = true;

        GenerateTiles();
    }
    private void GenerateTiles() {
        var cellPos = (Vector2Int)_floorTilemap.WorldToCell(Vector3.zero);
        foreach (var room in rooms.Values)
        {
            var size = room.IsBossRoom ? _bossRoomSize : _generalRoomSize - Random.Range(0, 5);
            var start = -size / 2;
            var end = size / 2 - 1;
            room.BoundStart = room.Pos + new Vector2Int(start, start);
            room.BoundEnd = room.Pos + new Vector2Int(end, end);
            for(var x = start; x <= end; x++) {
                _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + Vector2Int.right * x + Vector2Int.up * (start - 1)), _wallRuleTile);
                _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + Vector2Int.right * x + Vector2Int.up * (end + 1)), _wallRuleTile);
                _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + Vector2Int.up * x + Vector2Int.right * (start - 1)), _wallRuleTile);
                _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + Vector2Int.up * x + Vector2Int.right * (end + 1)), _wallRuleTile);
                for(var y = start; y <= end; y++) {
                    _floorTilemap.SetTile((Vector3Int)(cellPos + room.Pos + new Vector2Int(x, y)), 
                            _floorTiles[Random.Range(0, _floorTiles.Length)]);
                }
            }
            _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + new Vector2Int(start - 1, end + 1)), _wallRuleTile);
            _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + new Vector2Int(end + 1, end + 1)), _wallRuleTile);
            _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + new Vector2Int(start - 1, start - 1)), _wallRuleTile);
            _wallTilemap.SetTile((Vector3Int)(cellPos + room.Pos + new Vector2Int(end + 1, start - 1)), _wallRuleTile);
        }

        List<Vector3Int> wallCheck = new();

        foreach (var room in rooms.Values)
        {
            var size = room.IsBossRoom ? _bossRoomSize : _generalRoomSize;
            var start = -size / 2;
            var end = size / 2 - 1;
            foreach(var child in room.Children) {
                if(child == room.Parent) continue;
                var dir = (child.Pos - room.Pos);
                var childSize = child.IsBossRoom ? _bossRoomSize : _generalRoomSize;
                for(var h = size / 2 - 6; h < _bossRoomSize - childSize / 2 + 6; h++) {
                    for(var w = -3; w < 3; w++) {
                        var tilePos = (Vector3Int)(
                            cellPos + room.Pos + (dir.x != 0 ? 
                                new Vector2Int((dir.x > 0 ? 1 : -1) * h, w) 
                                : new Vector2Int(w, (dir.y > 0 ? 1 : -1) * h))
                        );
                        if(_floorTilemap.GetTile(tilePos) != null) continue;
                        if(-3 < w && w < 2) {
                            _wallTilemap.SetTile(tilePos, null);
                            _floorTilemap.SetTile(tilePos, _floorTiles[Random.Range(0, _floorTiles.Length)]);
                        }
                        else {
                            if(_wallTilemap.GetTile(tilePos) == null)
                                _wallTilemap.SetTile(tilePos, _wallRuleTile);
                            else {
                                wallCheck.Add(tilePos);
                            }
                        }
                    }
                }
            }
        }
        
        GenerateMinimapTiles();
        GenerateShadow();
    }

    private void GenerateMinimapTiles() 
    {
        var bounds = _floorTilemap.cellBounds;
        for(var x = bounds.xMin; x <= bounds.xMax; x++) 
        {
            for(var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y);
                if(_floorTilemap.HasTile(pos)) _minimapTilemap.SetTile(pos, _minimapFloorTile);
            }
        }
    }

    private void GenerateShadow() {
        StartCoroutine(nameof(GenerateShadowC));
    }

    private IEnumerator GenerateShadowC() {
        yield return null;
        yield return null;
        _wallTilemap.GetComponent<AutoShadowClosedTilemap>().Generate();
    }

    public class Room
    {
        public int TotalLength;
        public List<Room> Children = new();
        public Room Parent;
        public bool IsBossRoom = false;
        public Vector2Int Pos;
        public Vector2Int BoundStart, BoundEnd;

        public Room(int totalLen, Vector2Int pos, Room parent)
        {
            Pos = pos;
            Parent = parent;
            TotalLength = totalLen;
        }
    }
}
