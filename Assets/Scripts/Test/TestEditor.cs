using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestEditor : MonoBehaviour 
{
    [SerializeField] private GameObject trashObject;
    [SerializeField] private LineElement signature;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private LineElement linePrefab;
    [SerializeField] private Button addLineButton;
    [SerializeField] private Button insertButton;
    [SerializeField] private RectTransform gutter;

    private MonoBehaviourPooler<AST.Line, LineElement> lines;

    private void Awake()
    {
        lines = new MonoBehaviourPooler<AST.Line, LineElement>(linePrefab,
                                                               lineContainer,
                                                               (l, e) => e.Setup(l));

        addLineButton.onClick.AddListener(AddLine);
        insertButton.onClick.AddListener(InsertLine);

        insertButton.gameObject.SetActive(false);
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

    private AST.Line hoveredLine;
    private bool hoveredAbove;

    private void Update()
    {
        Vector2 point;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gutter, Input.mousePosition, null, out point);
        bool show = gutter.rect.Contains(point);

        hoveredLine = null;

        foreach (var line in lines.Shortcuts)
        { 
            var rtrans = lines.Get(line).transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtrans, Input.mousePosition, null, out point);

            var rect = rtrans.rect;
            rect.height += 4;

            bool inside = rect.Contains(point);

            if (inside)
            {
                bool above = point.y > 0;

                Vector3 pos = insertButton.transform.position;
                pos.y = rtrans.position.y + (above ? 14 : -14);
                insertButton.transform.position = pos;

                hoveredLine = line;
                hoveredAbove = above;

                break;
            }
        }

        Debug.LogFormat("Hovered Line {0} Above? {1}", hoveredLine, hoveredAbove);

        insertButton.gameObject.SetActive(show);
    }

    public void SetFunction(AST.Function function)
    {
        this.function = function;

        signature.Setup(function.signature);
        lines.SetActive(function.definition);

        insertButton.transform.SetAsLastSibling();
        addLineButton.transform.SetAsLastSibling();
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
        EndDrag(dragging);

        SetFunction(function);
    }

    public void SwapLine(AST.Line line)
    {
        function.definition.Remove(dragging);
        int index = function.definition.IndexOf(line);
        function.definition.Insert(index + 1, dragging);
        EndDrag(dragging);

        SetFunction(function);
    }

    public void AddLine()
    {
        function.definition.Add(new AST.Line
        {
            arguments = new[]
                {
                    new AST.Component("set"),
                    new AST.Component("next", AST.Type.Position),
                    new AST.Component("to"),
                    new AST.Component("the first <i>position</i> in"),
                    new AST.Component("path", AST.Type.Position, true),
                },
        });

        SetFunction(function);
    }

    public void OnHoverGutterBegin()
    {
        insertButton.gameObject.SetActive(true);
    }

    public void OnHoverGutterEnd()
    {
        insertButton.gameObject.SetActive(false);
    }

    public void InsertLine()
    {
        Assert.IsNotNull(hoveredLine, "No line hovered to insert relative to!");

        var line = new AST.Line
        {
            arguments = new[]
            {
                new AST.Component("set"),
                new AST.Component("next", AST.Type.Position),
                new AST.Component("to"),
                new AST.Component("the first <i>position</i> in"),
                new AST.Component("path", AST.Type.Position, true),
            },
        };

        int index = function.definition.IndexOf(hoveredLine);
        function.definition.Insert(index + (hoveredAbove ? 0 : 1), line);

        SetFunction(function);
    }
}
