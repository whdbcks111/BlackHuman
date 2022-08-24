using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attribute
{
    private Dictionary<AttributeType, float> _defaultValues = new();
    private List<AttributeModifier> _modifiers = new();
    private Dictionary<AttributeType, float> additionValues = new(), multiplyValues = new();

    public Attribute()
    {
        foreach(var type in AttributeType.GetAll<AttributeType>())
        {
            _defaultValues[type] = type.DefaultValue;
        }
        ResetValues();
    }

    public float GetValue(AttributeType type)
    {
        return (GetDefaultValue(type) + additionValues[type]) * multiplyValues[type];
    }

    public float GetDefaultValue(AttributeType type)
    {
        return _defaultValues[type];
    }

    public void SetDefaultValue(AttributeType type, float newDefault)
    {
        _defaultValues[type] = newDefault;
    }

    public void AddModifier(AttributeModifier modifier)
    {
        _modifiers.Add(modifier);
    }

    public void OnLateUpdate()
    {
        UpdateValues();
    }

    private void ResetValues()
    {
        foreach(var type in AttributeType.GetAll<AttributeType>())
        {
            additionValues[type] = 0f;
            multiplyValues[type] = 1f;
        }
    }

    // apply modifiers to values
    private void UpdateValues()
    {
        ResetValues();

        foreach(var modifier in _modifiers)
        {
            switch(modifier.ModifyType)
            {
                case AttributeModifier.Type.Add:
                    additionValues[modifier.AttributeType] += modifier.Amount;
                    break;
                case AttributeModifier.Type.Multiply:
                    multiplyValues[modifier.AttributeType] *= modifier.Amount;
                    break;
            }
        }

        _modifiers.Clear();
    }
}