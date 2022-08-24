using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public abstract class Enumeration : IComparable
{
    public string Name { get; private set; }

    public int Id { get; private set; }

    protected Enumeration(string name) 
    {
        Name = name;
        Id = name.GetHashCode();
    }

    public override string ToString() => Name;

    public static List<T> GetAll<T>() where T : Enumeration
    {
        List<T> result = new();
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var info in fields)
        {
            var locatedValue = info.GetValue(null);

            if (locatedValue != null && locatedValue is T)
            {
                result.Add(locatedValue as T);
            }
        }
        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

    public override int GetHashCode()
    {
        return Id;
    }
}