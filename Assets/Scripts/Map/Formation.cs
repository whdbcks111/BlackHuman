using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Formation
{
    public string Name;
    public BlockPos[] Blocks;
    public EnemyPos[] Enemies;

    public static Vector2Int GetPivotPos(Vector2Int start, Vector2Int end, Vector2Int pos, Pivot pivot)
    {
        var center = (start + end) / 2;
        switch(pivot)
        {
            case Pivot.LeftTop:
                return new Vector2Int(start.x, end.y) + pos;
            case Pivot.LeftMiddle:
                return new Vector2Int(start.x, center.y) + pos;
            case Pivot.LeftBottom:
                return new Vector2Int(start.x, start.y) + pos;
            case Pivot.MiddleTop:
                return new Vector2Int(center.x, end.y) + pos;
            case Pivot.Center:
                return new Vector2Int(center.x, center.y) + pos;
            case Pivot.MiddleBottom:
                return new Vector2Int(center.x, start.y) + pos;
            case Pivot.RightTop:
                return new Vector2Int(end.x, end.y) + pos;
            case Pivot.RightMiddle:
                return new Vector2Int(end.x, center.y) + pos;
            case Pivot.RightBottom:
                return new Vector2Int(end.x, start.y) + pos;
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
    public int EnemyIdx;
}

public enum Pivot
{
    LeftTop, MiddleTop, RightTop,
    LeftMiddle, Center, RightMiddle,
    LeftBottom, MiddleBottom, RightBottom
}