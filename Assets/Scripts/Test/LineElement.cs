using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class LineElement : MonoBehaviour, 
                           IBeginDragHandler, 
                           IEndDragHandler,
                           IDropHandler
{
    [SerializeField] private TestEditor editor;

    [SerializeField] private Image background;
    [SerializeField] private LineComponent componentPrefab;
    [SerializeField] private RectTransform componentContainer;

    [SerializeField] private Color ruleColor;
    [SerializeField] private Color actionColor;

    private MonoBehaviourPooler<AST.Component, LineComponent> arguments;

    private AST.Line line;

    private void Awake()
    {
        arguments = new MonoBehaviourPooler<AST.Component, LineComponent>(componentPrefab,
                                                                          componentContainer,
                                                                          (c, a) => a.Setup(c));
    }

    public void Setup(AST.Line line)
    {
        this.line = line;

        if (line.arguments[0].comment == "if") background.color = actionColor;
        if (line.arguments[0].comment == "set") background.color = ruleColor;

        arguments.SetActive(line.arguments);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        editor.BeginDrag(line);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        editor.EndDrag(line);
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        editor.SwapLine(line);
    }
}
