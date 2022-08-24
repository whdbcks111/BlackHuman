using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public int TotalLength;
    public List<Room> Children = new();
    public Room Parent;
    public RoomType Type;
    public Vector2Int Pos;
    public EnemyPos[] Enemies;
    private bool _isClosed = false;
    public bool IsClosed 
    { 
        get { return _isClosed; } 
        set { 
            _isClosed = value;
            if(value) WasOnceClosed = true;
        } 
    }
    public bool WasOnceClosed = false;
    public Vector2Int BoundStart, BoundEnd;

    public Room(int totalLen, Vector2Int pos, Room parent, RoomType roomType = RoomType.None)
    {
        Pos = pos;
        Parent = parent;
        TotalLength = totalLen;
        Type = roomType;
    }
}

public enum RoomType
{
    Monster,
    Treasure,
    Boss,
    None
}