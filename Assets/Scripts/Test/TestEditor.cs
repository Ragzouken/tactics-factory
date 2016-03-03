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
                arguments = new []
                {
                    new AST.Component("set"),
                    new AST.Component("next", AST.Type.Position),
                    new AST.Component("to"),
                    new AST.Component("the first <i>position</i> in"),
                    new AST.Component("path", AST.Type.Position, true),
                },
            },

            new AST.Line
            {
                arguments = new []
                {
                    new AST.Component("set"),
                    new AST.Component("rest", AST.Type.Position, true),
                    new AST.Component("to"),
                    new AST.Component("path", AST.Type.Position, true),
                    new AST.Component("after skipping"),
                    new AST.Component("1", AST.Type.Number),
                    new AST.Component("<i>position</i>"),
                },
            },

            new AST.Line
            {
                arguments = new []
                {
                    new AST.Component("set"),
                    new AST.Component("unblocked", AST.Type.Boolean),
                    new AST.Component("to"),
                    new AST.Component("whether"),
                    new AST.Component("vehicle", AST.Type.Object),
                    new AST.Component("can reach"),
                    new AST.Component("next", AST.Type.Position),
                },
            },

            new AST.Line
            {
                arguments = new []
                {
                    new AST.Component("if"),
                    new AST.Component("unblocked", AST.Type.Boolean),
                    new AST.Component("then"),
                    new AST.Component("move"),
                    new AST.Component("vehicle", AST.Type.Object),
                    new AST.Component("to"),
                    new AST.Component("next", AST.Type.Position),
                },
            },

            new AST.Line
            {
                arguments = new []
                {
                    new AST.Component("if"),
                    new AST.Component("unblocked", AST.Type.Boolean),
                    new AST.Component("then"),
                    new AST.Component("have"),
                    new AST.Component("vehicle", AST.Type.Object),
                    new AST.Component("follow"),
                    new AST.Component("rest", AST.Type.Position, true),
                },
            },
        });
    }
}
