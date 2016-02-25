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

    private MonoBehaviourPooler<DAttribute, ListElement> attributes;
    
    private void Awake()
    {
        attributes = new MonoBehaviourPooler<DAttribute, ListElement>(elementPrefab,
                                                                      attributesContainer,
                                                                      InitialiseAttribute);
    }

    private void InitialiseAttribute(DAttribute attribute,
                                     ListElement element)
    {
        element.Setup(delegate { },
                      attribute.name,
                      attribute.type.ToString());
    }

    private void Start()
    {
        attributes.SetActive(new[]
        {
            new DAttribute { name = "mobile", type = DType.Tag        },
            new DAttribute { name = "range",  type = DType.Value      },
            new DAttribute { name = "owner",  type = DType.Collection },
        });
    }
}
