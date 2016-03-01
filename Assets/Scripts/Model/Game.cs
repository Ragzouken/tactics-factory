using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum DType
{
    Tag,
    State,
    Value,
    Collection,
}

public class Game
{
    public State state;
    public Rules rules;
}

public class Rules
{
    public ICollection<DAttribute> attributes;
}

public class State
{
    public ICollection<DObject> objects;
}

public class DObject
{
    public string name;
    public Dictionary<DAttribute, object> attributes 
        = new Dictionary<DAttribute, object>();

    public static DObject Copy(DObject original, 
                               string name = null)
    {
        return new DObject
        {
            name = name ?? original.name + "'",

            attributes = original.attributes.ToDictionary(pair => pair.Key,
                                                          pair => DAttribute.Copy(pair.Key, pair.Value)),
        };
    }
}

public class DAttribute
{
    public string name;
    public DType type;

    public static object Copy(DAttribute attribute, object value)
    {
        if (attribute.type == DType.Collection)
        {
            return new List<DObject>((List<DObject>) value);
        }
        else
        {
            return value;
        }
    }
}
