using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestEditor : MonoBehaviour 
{
    [SerializeField] private SelectorPopup selector;

    [SerializeField] private GameObject trashObject;
    [SerializeField] private LineElement signature;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private LineElement linePrefab;
    [SerializeField] private Button addLineButton;
    [SerializeField] private Button insertButton;
    [SerializeField] private GameObject insertIconObject;
    [SerializeField] private RectTransform gutter;

    [SerializeField] private Sprite positionIcon;
    [SerializeField] private Sprite numberIcon;
    [SerializeField] private Sprite boolIcon;
    [SerializeField] private Sprite objectIcon;
    [SerializeField] private Color collectionColor;

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

        EditComponent(function.definition[3], 6);
    }

    private AST.Line hoveredLine;
    private bool hoveredAbove;

    private float velocity;

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
            rect.y -= 4;
            rect.height += 8;

            bool inside = rect.Contains(point);

            if (inside)
            {
                bool above = point.y > 0;

                Vector3 pos = insertButton.transform.position;
                pos.y = rtrans.position.y + (above ? 14 : -14);

                //pos.y = Mathf.SmoothDamp(pos.y, rtrans.position.y + (above ? 14 : -14), ref velocity, 0.1f);

                insertButton.transform.position = pos;

                hoveredLine = line;
                hoveredAbove = above;

                break;
            }
        }

        show |= dragging != null;

        insertButton.gameObject.SetActive(show && hoveredLine != null);
        insertIconObject.gameObject.SetActive(dragging == null);

        lines.MapActive((l, p) => p.bordered = l == dragging);
    }

    public void SetFunction(AST.Function function)
    {
        this.function = function;

        signature.Setup(function.signature);
        lines.SetActive(function.definition);

        addLineButton.transform.SetAsLastSibling();
    }

    public void EditComponent(AST.Component component)
    {
        var line = lines.Shortcuts.First(l => l.arguments.Contains(component));

        selector.gameObject.SetActive(true);

        EditComponent(line, line.arguments.ToList().IndexOf(component));
    }

    public void EditComponent(AST.Line line, int index)
    {
        System.Func<AST.Component, System.Action> makething = (AST.Component component) =>
        {
            return () =>
            {
                line.arguments[index] = new AST.Component(component.name, component.type.type, component.type.collection);
                lines.Get(line).Refresh();
                selector.gameObject.SetActive(false);
            };
        };

        var components = function.GetLocalsFor(line)
                                 .Concat(function.signature.arguments.Where(c => c.comment == null))
                                 .ToArray();
        
        selector.SetCategories(new[]
        {
            new SelectorPopup.Category
            {
                title = "positions",
                items = components.Where(component => component.type.type == AST.Type.Position)
                                  .Select(component => new SelectorPopup.Item { name = component.name,
                                                                                  icon = positionIcon,
                                                                                  color = component.type.collection ? collectionColor
                                                                                                                    : Color.white,
                                                                                  action = makething(component) })
                                  .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "numbers",
                items = components.Where(component => component.type.type == AST.Type.Number)
                                .Select(component => new SelectorPopup.Item { name = component.name,
                                                                              icon = numberIcon,
                                                                              color = component.type.collection ? collectionColor
                                                                                                                : Color.white,
                                                                              action = makething(component) })
                                .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "booleans",
                items = components.Where(component => component.type.type == AST.Type.Boolean)
                                .Select(component => new SelectorPopup.Item { name = component.name,
                                                                              icon = boolIcon,
                                                                              color = component.type.collection ? collectionColor
                                                                                                                : Color.white,
                                                                              action = makething(component) })
                                .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "objects",
                items = components.Where(component => component.type.type == AST.Type.Object)
                                  .Select(component => new SelectorPopup.Item { name = component.name,
                                                                                icon = objectIcon,
                                                                                color = component.type.collection ? collectionColor
                                                                                                                  : Color.white,
                                                                                action = makething(component) })
                                  .ToArray(),
            },
        });
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

    public void DropLine()
    {
        if (dragging == null) return;

        function.definition.Remove(dragging);
        int index = function.definition.IndexOf(hoveredLine);
        function.definition.Insert(index + (hoveredAbove ? 0 : 1), dragging);

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
