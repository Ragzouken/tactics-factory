using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ListElement : MonoBehaviour 
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Button editButton;
    [SerializeField] private GameObject selectedObject;
    [SerializeField] private GameObject highlightedObject;

    [SerializeField] private Text textPrefab;
    [SerializeField] private RectTransform textContainer;

    public bool highlighted
    {
        set
        {
            highlightedObject.SetActive(value);
        }
    }

    private MonoBehaviourPooler<string, Text> labels;

    private Action select;
    private Action edit;

    private void Awake()
    {
        labels = new MonoBehaviourPooler<string, Text>(textPrefab,
                                                       textContainer,
                                                       (text, label) => label.text = text);

        toggle.onValueChanged.AddListener(OnToggled);
        editButton.onClick.AddListener(() => edit());
    }

    public void Setup(Action select,
                      Action edit, 
                      params string[] labels)
    {
        this.select = select;
        this.edit = edit;
        this.labels.SetActive(labels);
    }

    private void OnToggled(bool active)
    {
        if (active) select();
    }
}
