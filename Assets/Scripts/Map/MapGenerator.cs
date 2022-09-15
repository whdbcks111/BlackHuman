using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{

    public static MapGenerator Instance { get; private set; }

    public readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    [SerializeField]
    private int _generalRoomSize, _bossRoomSize;
    [SerializeField]
    private Tilemap _wallTilemap, _floorTilemap, _doorTilemap, _minimapTilemap;
    [SerializeField]
    private RuleTile _wallRuleTile;
    [SerializeField]
    private TileBase[] _floorTiles;
    [SerializeField]
    private TileBase _minimapFloorTile;
    [SerializeField]
    private TileBase _doorFloorTile, _doorWallTile;

    [SerializeField]
    private MapTheme[] _themes;
    [SerializeField]
    private Formation[] _formations;

    private Dictionary<string, Formation> _formationMap = new();
    private Dictionary<Vector2Int, Room> _rooms = new();
    private bool _isFinalized = false;
    private MapTheme _curTheme;



    private void Awake() 
    {
        Instance = this;

        foreach(var f in _formations)
        {
            _formationMap[f.Name] = f;
        }
    }

    private void Start() 
    {
        GenerateRooms(13);
    }

    public float GetCompleteLevel()
    {
        int complete = 0, max = 0;
        foreach(var room in _rooms.Values)
        {
            if(room.WasOnceClosed && !room.IsClosed) complete++;
            max++;
        }
        return (float)complete / max;
    }

    private void Update() {
        var playerPos = Player.Instance.transform.position;
        
        foreach(var room in _rooms.Values)
        {
            if(!IsInAABB((Vector2)playerPos, 
                    _floorTilemap.CellToWorld((Vector3Int)room.BoundStart) + Vector3.up * 0.8f + Vector3.right * 0.3f, 
                    _floorTilemap.CellToWorld((Vector3Int)room.BoundEnd) + Vector3.down * 0.2f + Vector3.right * 0.7f)) continue;
            Player.Instance.CurrentRoom = room;
            if(room.Type != RoomType.Monster && room.Type != RoomType.Boss) continue;
            if(!room.IsClosed && !room.WasOnceClosed) 
            {
                CloseRoom(room);
            }
            else 
            {
                var isEnemyIn = false;
                foreach(var enemy in Enemy.GetEnemies())
                {
                    var enemyPos = enemy.transform.position;
                    if(IsInAABB(enemyPos, room.BoundStart, room.BoundEnd + Vector2Int.one))
                    {
                        isEnemyIn = true;
                        break;
                    }
                }

                if(!isEnemyIn) OpenRoom(room);
            }
        }
    }

    private static bool IsInAABB(Vector2 pos, Vector2 start, Vector2 end)
    {
        return start.x <= pos.x && pos.x <= end.x
                && start.y <= pos.y && pos.y <= end.y;
    }

    public void CloseRoom(Room room)
    {
        if(room.IsClosed) return;
        room.IsClosed = true;

        if(room.Type == RoomType.Boss) SoundManager.Instance.BgmAudioSource.pitch = 1.15f;

        for(var x = room.BoundStart.x; x <= room.BoundEnd.x; x++)
        {    
            var startPos = new Vector3Int(x, room.BoundStart.y - 1);
            var endPos = new Vector3Int(x, room.BoundEnd.y + 1);
            CloseDoor(startPos);
            CloseDoor(endPos);
        }
        for(var y = room.BoundStart.y; y <= room.BoundEnd.y; y++)
        {    
            var startPos = new Vector3Int(room.BoundStart.x - 1, y);
            var endPos = new Vector3Int(room.BoundEnd.x + 1, y);
            CloseDoor(startPos);
            CloseDoor(endPos);
        }
        GenerateShadowDefault(_doorTilemap);
        SoundManager.Instance.PlayOneShot("Door", 7f);

        if(room.Type != RoomType.Boss) 
        {
            Enemy[] enemyPrefabs = _curTheme.Enemies;

            foreach(var enemyPos in room.Enemies)
            {
                Enemy target = enemyPrefabs[Mathf.Clamp(Random.Range(enemyPos.EnemyIdxMin, enemyPos.EnemyIdxMax + 1), 
                        0, enemyPrefabs.Length)]; 
                Enemy.SpawnEnemy(target, Formation.GetPivot(room.BoundStart, room.BoundEnd, enemyPos.Pivot) 
                        + Vector2Int.FloorToInt(enemyPos.Pos));
            }
        }
        else
        {
            Enemy[] bosses = _curTheme.Bosses;
            Enemy boss = bosses[Random.Range(0, bosses.Length)]; 

            Enemy.SpawnEnemy(boss, _floorTilemap.CellToWorld((Vector3Int)(room.BoundStart + room.BoundEnd) / 2));
        }
    }

    public void OpenRoom(Room room)
    {
        if(!room.IsClosed) return;
        room.IsClosed = false;

        if(room.Type == RoomType.Boss) SoundManager.Instance.BgmAudioSource.pitch = 1f;

        for(var x = room.BoundStart.x; x <= room.BoundEnd.x; x++)
        {    
            var startPos = new Vector3Int(x, room.BoundStart.y - 1);
            var endPos = new Vector3Int(x, room.BoundEnd.y + 1);
            OpenDoor(startPos);
            OpenDoor(endPos);
        }
        for(var y = room.BoundStart.y; y <= room.BoundEnd.y; y++)
        {
            var startPos = new Vector3Int(room.BoundStart.x - 1, y);
            var endPos = new Vector3Int(room.BoundEnd.x + 1, y);
            OpenDoor(startPos);
            OpenDoor(endPos);
        }
        GenerateShadowDefault(_doorTilemap);
        SoundManager.Instance.PlayOneShot("Door", 7f);

        if(room.Type == RoomType.Monster && Random.value < 0.3f)
        {
            var treasureBox = Block.SetBlock(room.Pos, "TreasureBox");
            ParticleManager.Instance.SpawnParticle(treasureBox.gameObject.transform.position, 
                    ParticleType.HorizontalExplode, 1f, 0, 10);
        }
        else if(room.Type == RoomType.Boss)
        {
            var portal = NextStagePortal.PlacePortal(_floorTilemap.CellToWorld((Vector3Int)room.Pos));
            ParticleManager.Instance.SpawnParticle(portal.gameObject.transform.position, 
                    ParticleType.HorizontalExplode, Color.magenta, 1f, 0, 10);
        }
    }

    private void CloseDoor(Vector3Int pos)
    {      
        if(_floorTilemap.GetTile(pos) == _doorFloorTile)
        {
            _doorTilemap.SetTile(pos, _doorWallTile);
            _floorTilemap.SetTile(pos, null);
        }
    }

    private void OpenDoor(Vector3Int pos)
    {      
        if(_doorTilemap.GetTile(pos) == _doorWallTile) 
        {
            _floorTilemap.SetTile(pos, _doorFloorTile);
            _doorTilemap.SetTile(pos, null);
        }
    }

    public void GenerateRooms(int roomCnt)
    {
        _curTheme = _themes[Random.Range(0, _themes.Length)];
        _isFinalized = false;
        _rooms.Clear();
        _wallTilemap.ClearAllTiles();
        _floorTilemap.ClearAllTiles();
        _doorTilemap.ClearAllTiles();
        _minimapTilemap.ClearAllTiles();
        var newRoom = new Room(0, Vector2Int.zero, null);
        _rooms.Add(Vector2Int.zero, newRoom);
        TryCreateTransition(newRoom, roomCnt);
        GenerateFinalize();
    }

    private void TryCreateTransition(Room room, int roomCnt)
    {
        if (room.TotalLength >= roomCnt * .4f) {
            return;
        }
        var cnt = Random.Range(1, 4);
        if (_rooms.Count <= 1) cnt = 4;
        List<Vector2Int> directions = new(Directions);
        while (cnt > 0)
        {
            if (roomCnt <= _rooms.Count)
            {
                return;
            }
            var idx = Random.Range(0, directions.Count);
            var dir = directions[idx];
            directions.RemoveAt(idx);
            cnt--;

            var pos = room.Pos + dir * _bossRoomSize;
            if (_rooms.ContainsKey(pos)) continue;
            var newRoom = new Room(room.TotalLength + 1, pos, room, RoomType.Monster);
            _rooms.Add(pos, newRoom);
            room.Children.Add(newRoom);
            TryCreateTransition(newRoom, roomCnt);
        }
    }

    private void GenerateFinalize()
    {
        if (_isFinalized) return;
        _isFinalized = true;

        Room farthestRoom = null;
        foreach (var room in _rooms.Values)
        {
            if (farthestRoom == null 
                    || farthestRoom.TotalLength < room.TotalLength
                    || (farthestRoom.TotalLength == room.TotalLength && room.Pos.magnitude > farthestRoom.Pos.magnitude)) 
                farthestRoom = room;
        }
        farthestRoom.Type = RoomType.Boss;

        List<Room> monsterRooms = new();
        foreach (var room in _rooms.Values) 
        {
            if(room.Type == RoomType.Monster) monsterRooms.Add(room);
        } 

        for (var i = 0; i < (int)(_rooms.Count * 0.1f + 1) && monsterRooms.Count > 0; i++)
        {
            var room = monsterRooms[Random.Range(0, monsterRooms.Count)];
            room.Type = RoomType.Shop;
            monsterRooms.Remove(room);
        }

        if(Random.value < 0.4)
            for (var i = 0; i < (int)(_rooms.Count * 0.05f + 1) && monsterRooms.Count > 0; i++)
            {
                var room = monsterRooms[Random.Range(0, monsterRooms.Count)];
                room.Type = RoomType.Treasure;
                monsterRooms.Remove(room);
            }

        GenerateTiles();
    }
    private void GenerateTiles() {
        var cellPos = (Vector2Int)_floorTilemap.WorldToCell(Vector3.zero);
        foreach (var room in _rooms.Values)
        {
            var size = room.Type == RoomType.Boss ? _bossRoomSize : _generalRoomSize - Random.Range(-1, 2) * 2;
            var start = -size / 2;
            var end = size / 2;
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

        foreach(var room in _rooms.Values)
        {
            switch(room.Type)
            {
                case RoomType.Treasure:
                    Block.SetBlock(room.Pos, "TreasureBox");
                    break;
                case RoomType.Shop:
                    var center = _floorTilemap.CellToWorld((Vector3Int)room.Pos);
                    for(var j = -1; j <= 1; j++)
                    {
                        List<ItemType> types = ItemType.GetAll<ItemType>();
                        var itemType = types[Random.Range(0, types.Count)];
                        ShopItem.PlaceItem(new(itemType), Random.Range(50, 300), 
                                center + Vector3.right * j * 2.5f + Vector3.down);
                    }
                    Npc.SpawnNpc("Shopper", center + Vector3.up * 0.5f);
                    break;
            }
        }

        foreach (var room in _rooms.Values)
        {
            var size = room.BoundEnd.x - room.BoundStart.x + 1;
            foreach(var child in room.Children) {
                if(child == room.Parent) continue;
                var dir = (child.Pos - room.Pos);
                var childSize = child.BoundEnd.x - child.BoundStart.x + 1;
                for(var h = size / 2 - 6; h < _bossRoomSize - childSize / 2 + 6; h++) {
                    for(var w = -3; w < 3; w++) {
                        var tilePos = (Vector3Int)(
                            cellPos + room.Pos + (dir.x != 0 ? 
                                new Vector2Int((dir.x > 0 ? 1 : -1) * h, w) 
                                : new Vector2Int(w, (dir.y > 0 ? 1 : -1) * h))
                        );
                        if(_floorTilemap.GetTile(tilePos) != null) continue;
                        var isChildDir = IsInAABB((Vector2Int)tilePos, child.BoundStart - Vector2Int.one, child.BoundEnd + Vector2Int.one);
                        var doGenerateDoor = (isChildDir && (child.Type == RoomType.Monster || child.Type == RoomType.Boss)) 
                                || (!isChildDir && (room.Type == RoomType.Monster || room.Type == RoomType.Boss));
                        if(-3 < w && w < 2) {
                            _floorTilemap.SetTile(tilePos, 
                                    (_wallTilemap.GetTile(tilePos) == null || !doGenerateDoor) ?
                                    _floorTiles[Random.Range(0, _floorTiles.Length)] : _doorFloorTile
                            );
                            _wallTilemap.SetTile(tilePos, null);
                        }
                        else {
                            if(_wallTilemap.GetTile(tilePos) == null)
                                _wallTilemap.SetTile(tilePos, _wallRuleTile);
                        }
                    }
                }
            }

            if(room.Type == RoomType.Monster) 
            {
                var formations = _curTheme.Formations;
                var formation = _formationMap[formations[Random.Range(0, formations.Length)]];
                room.Enemies = formation.Enemies.ToArray();

                foreach(var blockPos in formation.Blocks)
                {
                    Block.SetBlock(Formation.GetPivot(room.BoundStart, room.BoundEnd, blockPos.Pivot) + blockPos.Pos, blockPos.Block);
                }
            }
        }
        
        GenerateMinimapTiles();
        GenerateShadow(_wallTilemap);
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

    private void GenerateShadow(Tilemap tilemap) {
        StartCoroutine(nameof(GenerateShadowC), tilemap);
    }

    private IEnumerator GenerateShadowC(Tilemap tilemap) {
        yield return null;
        yield return null;
        tilemap.GetComponent<AutoShadowClosedTilemap>().Generate();
    }

    private void GenerateShadowDefault(Tilemap tilemap) {
        StartCoroutine(nameof(GenerateShadowDefaultC), tilemap);
    }

    private IEnumerator GenerateShadowDefaultC(Tilemap tilemap) {
        yield return null;
        yield return null;
        tilemap.GetComponent<AutoShadowClosedTilemap>().GenerateDefault();
    }
}