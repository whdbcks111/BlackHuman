using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Formation
{
    public string Name;
    public List<BlockPos> Blocks;
    public List<EnemyPos> Enemies;

    public static Vector2Int GetPivot(Vector2Int start, Vector2Int end, Pivot pivot)
    {
        var center = (start + end) / 2;
        switch(pivot)
        {
            case Pivot.LeftTop:
                return new Vector2Int(start.x, end.y);
            case Pivot.LeftMiddle:
                return new Vector2Int(start.x, center.y);
            case Pivot.LeftBottom:
                return new Vector2Int(start.x, start.y);
            case Pivot.MiddleTop:
                return new Vector2Int(center.x, end.y);
            case Pivot.Center:
                return new Vector2Int(center.x, center.y);
            case Pivot.MiddleBottom:
                return new Vector2Int(center.x, start.y);
            case Pivot.RightTop:
                return new Vector2Int(end.x, end.y);
            case Pivot.RightMiddle:
                return new Vector2Int(end.x, center.y);
            case Pivot.RightBottom:
                return new Vector2Int(end.x, start.y);
            default:
                return center;
        }
    }
}

[Serializable]
public class BlockPos
{
    public Pivot Pivot;
    public Vector2Int Pos;
    public Block Block;
}

[Serializable]
public class EnemyPos
{
    public Pivot Pivot;
    public Vector2 Pos;
    public int EnemyIdxMin, EnemyIdxMax;
}

public enum Pivot
{
    LeftTop, MiddleTop, RightTop,
    LeftMiddle, Center, RightMiddle,
    LeftBottom, MiddleBottom, RightBottom
}