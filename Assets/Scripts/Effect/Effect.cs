using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Effect
{
    public EffectType Type { get; private set; }
    public float Duration;
    public float MaxDuration { get; private set; }
    public int Level;
}