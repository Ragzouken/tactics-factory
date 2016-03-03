using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LineElement : MonoBehaviour 
{
    [SerializeField] private Image background;
    [SerializeField] private LineComponent componentPrefab;
    [SerializeField] private RectTransform componentContainer;

    private MonoBehaviourPooler<AST.Component, LineComponent> arguments;

    private void Awake()
    {
        arguments = new MonoBehaviourPooler<AST.Component, LineComponent>(componentPrefab,
                                                                          componentContainer,
                                                                          (c, a) => a.Setup(c));
    }

    public void Setup(AST.Line line)
    {
        arguments.SetActive(line.arguments);
    }
}
