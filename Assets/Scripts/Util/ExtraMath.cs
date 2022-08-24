public class ExtraMath
{
    public static bool IsAngleBetween(float target, float from, float to)
    {
        target = (target % 360f + 360f) % 360f;
        from = (from % 360f + 360f) % 360f;
        to = (to % 360f + 360f) % 360f;
        var rAngle = ((to - from) % 360f + 360f) % 360f;
        if(rAngle > 180)
        {
            var t = to;
            to = from;
            from = t;
        }
        if(from <= to) return from <= target && target <= to;
        return from <= target || target <= to;
    }
}