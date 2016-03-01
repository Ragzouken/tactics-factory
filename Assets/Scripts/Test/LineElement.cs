using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LineElement : MonoBehaviour 
{
    [SerializeField] private LineComponent left;
    [SerializeField] private LineComponent command;

    [SerializeField] private Image typeImage;
    [SerializeField] private Sprite assignSprite;
    [SerializeField] private Sprite actionSprite;

    [SerializeField] private LineComponent argumentPrefab;
    [SerializeField] private RectTransform argumentContainer;

    private MonoBehaviourPooler<AST.Component, LineComponent> arguments;

    private void Awake()
    {
        arguments = new MonoBehaviourPooler<AST.Component, LineComponent>(argumentPrefab,
                                                                          argumentContainer,
                                                                          (c, a) => a.Setup(c));
    }

    public void Setup(AST.Line line)
    {
        left.Setup(line.start);
        command.Setup(line.command);

        arguments.SetActive(line.arguments);

        bool action = line.command.type.type == AST.Type.Action;

        typeImage.sprite = action ? actionSprite : assignSprite;
    }
}
