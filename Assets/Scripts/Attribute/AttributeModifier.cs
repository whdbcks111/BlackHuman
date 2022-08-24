using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttributeModifier 
{
    public AttributeType AttributeType;
    public Type ModifyType;
    public float Amount;

    public AttributeModifier(AttributeType attributeType, Type modifyType, float amount) 
    {
        AttributeType = attributeType;
        ModifyType = modifyType;
        Amount = amount;
    }

    public enum Type 
    {
        Add, Multiply
    }
}