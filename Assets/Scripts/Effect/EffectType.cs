using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectType : Enumeration
{
    public static readonly EffectType Fire = new(nameof(Fire), 
    effect => {
        
    }, 
    effect => {

    }, 
    effect => {

    });

    public Action<Effect> OnEffectStart { get; private set; }
    public Action<Effect> OnEffectUpdate { get; private set; }
    public Action<Effect> OnEffectFinish { get; private set; }

    private EffectType(string name, Action<Effect> effectStartAction, Action<Effect> effectAction, Action<Effect> effectFinishAction)
        : base(name)
    {
        OnEffectStart = effectStartAction;
        OnEffectUpdate = effectAction;
        OnEffectFinish = effectFinishAction;
    }
}