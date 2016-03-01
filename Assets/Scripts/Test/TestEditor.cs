using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestEditor : MonoBehaviour 
{
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private LineElement linePrefab;

    private MonoBehaviourPooler<AST.Line, LineElement> lines;

    private void Awake()
    {
        lines = new MonoBehaviourPooler<AST.Line, LineElement>(linePrefab,
                                                               lineContainer,
                                                               (l, e) => e.Setup(l));
    }

    private void Start()
    {
        lines.SetActive(new[]
        {
            new AST.Line
            {
                start = new AST.Component("next", "variable", AST.Type.Position),
                command = new AST.Component("first", "command", AST.Type.Position),
                arguments = new []
                {
                    new AST.Component("path", "sequence", AST.Type.Position, true),
                },
            },

            new AST.Line
            {
                start = new AST.Component("unblocked", "condition", AST.Type.Boolean),
                command = new AST.Component("move", "command", AST.Type.Action),
                arguments = new []
                {
                    new AST.Component("vehicle", "mover", AST.Type.Object),
                    new AST.Component("next", "destination", AST.Type.Position),
                },
            }
        });
    }
}
