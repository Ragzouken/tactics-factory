using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestEditor : MonoBehaviour 
{
    [SerializeField] private GameObject trashObject;
    [SerializeField] private LineElement signature;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private LineElement linePrefab;

    private MonoBehaviourPooler<AST.Line, LineElement> lines;

    private void Awake()
    {
        lines = new MonoBehaviourPooler<AST.Line, LineElement>(linePrefab,
                                                               lineContainer,
                                                               (l, e) => e.Setup(l));
    }

    private AST.Function function;

    private void Start()
    {
        var function = new AST.Function
        {
            name = "follow path",
            signature = new AST.Line
            {
                arguments = new[]
                {
                    new AST.Component("have"),
                    new AST.Component("vehicle", AST.Type.Object),
                    new AST.Component("follow"),
                    new AST.Component("path", AST.Type.Position, true),
                },
            },
            definition = new List<AST.Line>
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
        }
        };

        SetFunction(function);
    }

    public void SetFunction(AST.Function function)
    {
        this.function = function;

        signature.Setup(function.signature);
        lines.SetActive(function.definition);
    }

    private AST.Line dragging;

    public void BeginDrag(AST.Line line)
    {
        dragging = line;
        trashObject.SetActive(true);
    }

    public void EndDrag(AST.Line line)
    {
        Assert.IsTrue(line == dragging, "Ending drag for line that isn't dragged!");

        dragging = null;
        trashObject.SetActive(false);
    }

    public void DeleteLine()
    {
        function.definition.Remove(dragging);
        lines.SetActive(function.definition);
        EndDrag(dragging);
    }

    public void SwapLine(AST.Line line)
    {
        function.definition.Remove(dragging);
        int index = function.definition.IndexOf(line);
        function.definition.Insert(index + 1, dragging);
        lines.SetActive(function.definition);
        EndDrag(dragging);
    }
}
