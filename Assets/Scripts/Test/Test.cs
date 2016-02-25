using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour 
{
    [SerializeField] private ListElement elementPrefab;
    [SerializeField] private RectTransform attributesContainer;
    [SerializeField] private RectTransform objectsContainer;
    [SerializeField] private RectTransform valuesContainer;

    [SerializeField] private Text objectNameText;

    private MonoBehaviourPooler<DAttribute, ListElement> attributes;
    private MonoBehaviourPooler<DObject, ListElement> objects;
    private MonoBehaviourPooler<DValue, ListElement> values;

    private struct DValue
    {
        public DObject @object;
        public DAttribute attribute;

        public T GetValue<T>()
        {
            return (T) @object.attributes[attribute];
        }
    }

    private void Awake()
    {
        attributes = new MonoBehaviourPooler<DAttribute, ListElement>(elementPrefab,
                                                                      attributesContainer,
                                                                      InitialiseAttribute);

        objects = new MonoBehaviourPooler<DObject, ListElement>(elementPrefab,
                                                                objectsContainer,
                                                                InitialiseObject);

        values = new MonoBehaviourPooler<DValue, ListElement>(elementPrefab,
                                                              valuesContainer,
                                                              InitialiseValue);
    }

    private void InitialiseAttribute(DAttribute attribute,
                                     ListElement element)
    {
        element.Setup(() => SelectAttribute(attribute),
                      delegate { },
                      attribute.name,
                      attribute.type.ToString());
    }

    private void InitialiseObject(DObject @object,
                                  ListElement element)
    {
        element.Setup(() => SelectObject(@object),
                      delegate { },
                      @object.name,
                      @object.attributes.Count.ToString());
    }

    private void InitialiseValue(DValue value,
                                 ListElement element)
    {
        element.Setup(() => SelectValue(value),
                      delegate { },
                      value.attribute.name,
                      ToString(value));
    }

    private void Start()
    {
        var mobile = new DAttribute { name = "mobile", type = DType.Tag };
        var range  = new DAttribute { name = "range",  type = DType.Value };
        var owner  = new DAttribute { name = "owner",  type = DType.Collection };

        attributes.SetActive(new[]
        {
            mobile,
            range,
            owner,
        });

        var tank = new DObject { name = "tank prefab" };
        AddAttribute(tank, mobile, true);
        AddAttribute(tank, range, 3);

        var hand = new DObject { name = "hand of cards" };
        AddAttribute(hand, owner, new DObject[] { tank });

        var cursor = new DObject { name = "cursor" };
        AddAttribute(cursor, owner, new DObject[] { tank } );
        AddAttribute(cursor, mobile, true);

        objects.SetActive(new[]
        {
            tank,
            hand,
            cursor,
        });
    }

    private void AddAttribute(DObject @object,
                              DAttribute attribute,
                              object value)
    {
        @object.attributes[attribute] = value;
    }

    private void SelectObject(DObject @object)
    {
        objects.MapActive((obj, element) => element.highlighted = false);
        attributes.MapActive((attr, element) => element.highlighted = @object.attributes.ContainsKey(attr));

        values.SetActive(@object.attributes.Select(pair => new DValue { attribute = pair.Key, @object = @object }));

        objectNameText.text = @object.name;
    }

    private void SelectAttribute(DAttribute attribute)
    {
        objects.MapActive((obj, element) => element.highlighted = obj.attributes.ContainsKey(attribute));
        attributes.MapActive((attr, element) => element.highlighted = false);

        values.Clear();
    }

    private void SelectValue(DValue value)
    {
        bool collection = value.attribute.type == DType.Collection;

        if (collection)
        {
            var objs = value.GetValue<DObject[]>();

            objects.MapActive((obj, element) => element.highlighted = objs.Contains(obj));
        }
        else
        {
            objects.MapActive((obj, element) => element.highlighted = false);
        }

        attributes.MapActive((attr, element) => element.highlighted = attr == value.attribute);
    }

    private string ToString(DValue value)
    {
        if (value.attribute.type == DType.Collection)
        {
            var objs = value.GetValue<DObject[]>();

            return string.Join(", ", objs.Select(obj => obj.name).ToArray());
        }
        else
        {
            return value.GetValue<object>().ToString();
        }
    }
}
