using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder : MonoBehaviour {
    public static PathFinder Instance { get; private set; }

    [SerializeField]
    private Tilemap _wallTilemap;

    private bool[,] map;

    private void Awake() {
        Instance = this;
    }

    private bool IsInMap(Vector2Int pos) {
        return pos.x >= 0 && pos.x < map.GetLength(0) && pos.y >= 0 && pos.y < map.GetLength(1);
    }

    public int GetPath(Stack<Vector3> paths, Vector3 from, Vector3 to, Vector2Int selfSize, 
            bool allowDiagonal = true, bool prohibitCorner = true) {
        paths.Clear();
        from -= new Vector3((selfSize.x - 1) * .5f, (selfSize.y - 1) * .5f);

        Vector2Int start = (Vector2Int)_wallTilemap.WorldToCell(from), end = (Vector2Int)_wallTilemap.WorldToCell(to);
        Vector2Int boundMin = new Vector2Int(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y));
        boundMin -= new Vector2Int(20, 20);
        Vector2Int boundMax = new Vector2Int(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
        boundMax += new Vector2Int(20, 20);
        map = new bool[
            (boundMax - boundMin).x - selfSize.x + 2, 
            (boundMax - boundMin).y - selfSize.y + 2
        ];

        for(var x = 0; x < map.GetLength(0); x++) {
            for(var y = 0; y < map.GetLength(1); y++) {
                bool isWall = false;
                Vector2Int cellPos = boundMin + new Vector2Int(x, y);
                for(var sx = 0; sx < selfSize.x; sx++) {
                    for(var sy = 0; sy < selfSize.y; sy++) {
                        var pos = cellPos + new Vector2Int(sx, sy);
                        if(_wallTilemap.GetTile((Vector3Int)pos) != null || Block.Blocks.ContainsKey(pos)) {
                            isWall = true;
                            break;
                        }
                    }
                }
                map[x,y] = isWall;
            }
        }
        
        List<Path> open = new(), close = new();
        open.Add(new Path(start, 0, Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y)));

        Vector3Int[] dirs;
        if(!allowDiagonal)
            dirs = new[]{ new Vector3Int(1, 0, 10), new Vector3Int(-1, 0, 10), new Vector3Int(0, 1, 10), new Vector3Int(0, -1, 10) };
        else 
            dirs = new[]{ new Vector3Int(1, 0, 10), new Vector3Int(-1, 0, 10), new Vector3Int(0, 1, 10), new Vector3Int(0, -1, 10),
                    new Vector3Int(1, 1, 14), new Vector3Int(1, -1, 14), new Vector3Int(-1, 1, 14), new Vector3Int(-1, -1, 14) };

        while(true) {
            if(open.Count == 0) break;
            var cur = open[0];
            for(var i = 1; i < open.Count; i++) {
                var o = open[i];
                if(o.G + o.H < cur.G + cur.H 
                        || (o.G + o.H == cur.G + cur.H && o.H < cur.H)) cur = o;
            }
            open.Remove(cur);
            close.Add(cur);
            
            if(cur.Pos == end) {
                var t = cur;
                var len = cur.G / 10;
                while(t.Parent != null) {
                    paths.Push(_wallTilemap.CellToWorld((Vector3Int)t.Pos) + new Vector3(selfSize.x * .5f, selfSize.y * .5f));
                    t = t.Parent;
                }
                return len;
            }

            foreach(var dir in dirs) {
                var dirPos = cur.Pos + (Vector2Int)dir;
                if(IsInMap(dirPos - boundMin) && !map[dirPos.x - boundMin.x, dirPos.y - boundMin.y] 
                        && close.Find(path => path.Pos == dirPos) == null) {
                    if(dir.x != 0 && dir.y != 0) {
                        if(map[dirPos.x - boundMin.x, cur.Pos.y - boundMin.y] == true 
                                && map[cur.Pos.x - boundMin.x, dirPos.y - boundMin.y] == true) {
                            continue;
                        }
                        if(prohibitCorner && (map[dirPos.x - boundMin.x, cur.Pos.y - boundMin.y] == true 
                                || map[cur.Pos.x - boundMin.x, dirPos.y - boundMin.y] == true)) {
                            continue;
                        }
                    }
                    var dirPath = open.Find(path => path.Pos == dirPos);
                    if(dirPath == null) dirPath = close.Find(path => path.Pos == dirPos);
                    var dirG = dirPath == null ? 0 : dirPath.G;
                    
                    if(open.Find(path => path.Pos == dirPos) == null 
                            || dirG > cur.G + dir.z) 
                    {
                        open.Add(new Path(dirPos, cur.G + dir.z, Mathf.Abs(dirPos.x - end.x) + Mathf.Abs(dirPos.y - end.y), cur));
                    }
                }
            }
        }
        return -1;
    }

    public class Path {
        public Vector2Int Pos;
        public int G, H;
        public Path Parent;

        public Path(Vector2Int pos, int g, int h, Path parent=null) {
            Pos = pos;
            G = g;
            H = h;
            Parent = parent;
        }
    }
}