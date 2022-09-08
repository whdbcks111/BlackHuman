using System.Collections.Generic;

public class ExtraMath
{

    public static float GetNormalAngle(float angle)
    {
        return (angle % 360f + 360f) % 360f;
    }
    public static bool IsAngleBetween(float target, float from, float to)
    {
        target = GetNormalAngle(target);
        var range = GetAngleRange(from, to);
        from = range.From;
        to = range.To;
        if(from <= to) return from <= target && target <= to;
        return from <= target || target <= to;
    }

    public static List<float> GetAnglePoints(float from, float to, float span)
    {
        List<float> result = new();
        var range = GetAngleRange(from, to);
        from = range.From;
        to = range.To;

        if(from <= to)
        {
            for(var a = from; a <= to; a += span)
                result.Add(a);
        }
        else
        {
            float a;
            for(a = from; a <= 360; a += span)
                result.Add(a);
            for(a = a - 360 + span; a <= to; a += span)
                result.Add(a);
        }

        return result;
    }

    public static AngleRange GetAngleRange(float from, float to)
    {
        from = GetNormalAngle(from);
        to = GetNormalAngle(to);
        var rAngle = GetNormalAngle(to - from);
        if(rAngle > 180)
        {
            var t = to;
            to = from;
            from = t;
        }
        return new(from, to);
    }

    public static float AddTowards(float from, float to, float span)
    {
        if(from < to) return from + span > to ? to : from + span;
        else return from - span < to ? to : from - span;
    }
}

public class AngleRange
{
    public float From, To;

    public AngleRange(float from, float to)
    {
        From = from;
        To = to;
    }
}