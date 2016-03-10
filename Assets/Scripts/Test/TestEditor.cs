﻿using UnityEngine;
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
    private AST.Function[] functions;

    private void Start()
    {
        var NUMBER = new AST.FullType { type = AST.Type.Number };

        var POSITION = new AST.FullType { type = AST.Type.Position };
        var POSITIONS = new AST.FullType { type = AST.Type.Position, collection = true };

        var OBJECT = new AST.FullType { type = AST.Type.Object };

        var BOOLEAN = new AST.FullType { type = AST.Type.Boolean };

        var element = new AST.Function
        {
            name = "element",
            signature = new[]
            {
                new AST.Reference("@result",  POSITION),
                new AST.Reference("offset",   NUMBER),
                new AST.Reference("sequence", POSITIONS),
            },

            comments = new[] { "element", "within", "" },
        };

        var canpass = new AST.Function
        {
            name = "pass",
            signature = new[]
            {
                new AST.Reference("@result", BOOLEAN),
                new AST.Reference("vehicle", OBJECT),
                new AST.Reference("from",    POSITION),
                new AST.Reference("to",      POSITION),
            },

            comments = new[] { "whether", "could pass from", "to", "" },
        };

        var skip = new AST.Function
        {
            name = "skip",
            signature = new[]
            {
                new AST.Reference("@result",  POSITIONS),
                new AST.Reference("sequence", POSITIONS),
                new AST.Reference("count",    NUMBER),
            },

            comments = new[] { "the elements of", "after skipping", "positions" },
        };

        var and = new AST.Function
        {
            name = "and",
            signature = new[]
            {
                new AST.Reference("@result",  BOOLEAN),
                new AST.Reference("a", BOOLEAN),
                new AST.Reference("b", BOOLEAN),
            },

            comments = new[] { "whether both", "and", "are true" },
        };

        var f = new AST.Function
        {
            name = "can follow path",
            signature = new[]
            {
                new AST.Reference("@result", BOOLEAN),
                new AST.Reference("vehicle", OBJECT),
                new AST.Reference("path",    POSITIONS),
            },

            comments = new[] { "whether", "can follow", "" },
        };

        var first  = new AST.Reference("first",  POSITION);
        var second = new AST.Reference("second", POSITION);
        var valid  = new AST.Reference("valid",  BOOLEAN);
        var rest   = new AST.Reference("rest",   POSITIONS);
        var others = new AST.Reference("others", BOOLEAN);
        var result = new AST.Reference("result", BOOLEAN);

        f.body = new List<AST.Line>
        {
            new AST.Line(element, first,  new AST.Reference("1", NUMBER), f.signature[2]),
            new AST.Line(element, second, new AST.Reference("2", NUMBER), f.signature[2]),
            new AST.Line(canpass, valid,  f.signature[1], first, second),
            new AST.Line(skip,    rest,   f.signature[2], new AST.Reference("1", NUMBER)),
            new AST.Line(f,       others, f.signature[1], rest),
            new AST.Line(and,     result, valid, others),
        };

        functions = new[]
        {
            element,
            canpass,
            skip,
            f,
            and,
        };

        SetFunction(f);
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

        //signature.Setup(function);
        lines.SetActive(function.body);

        addLineButton.transform.SetAsLastSibling();
    }

    public void EditInput(AST.Line line, int index)
    {
        selector.gameObject.SetActive(true);

        System.Func<AST.Reference, System.Action> makething = (AST.Reference reference) =>
        {
            return () =>
            {
                line.inputs[index] = reference;
                lines.Get(line).Refresh();
                selector.gameObject.SetActive(false);
            };
        };

        var components = function.GetLocalsFor(line)
                                 .Concat(function.signature)
                                 .ToArray();
        
        selector.SetCategories(new[]
        {
            new SelectorPopup.Category
            {
                title = "positions",
                items = components.Where(reference => reference.type.type == AST.Type.Position)
                                  .Select(reference => new SelectorPopup.Item { name = reference.name,
                                                                                icon = positionIcon,
                                                                                color = reference.type.collection ? collectionColor
                                                                                                                  : Color.white,
                                                                                action = makething(reference) })
                                  .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "numbers",
                items = components.Where(reference => reference.type.type == AST.Type.Number)
                                .Select(reference => new SelectorPopup.Item { name = reference.name,
                                                                              icon = numberIcon,
                                                                              color = reference.type.collection ? collectionColor
                                                                                                                : Color.white,
                                                                              action = makething(reference) })
                                .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "booleans",
                items = components.Where(reference => reference.type.type == AST.Type.Boolean)
                                .Select(reference => new SelectorPopup.Item { name = reference.name,
                                                                              icon = boolIcon,
                                                                              color = reference.type.collection ? collectionColor
                                                                                                                : Color.white,
                                                                              action = makething(reference) })
                                .ToArray(),
            },

            new SelectorPopup.Category
            {
                title = "objects",
                items = components.Where(reference => reference.type.type == AST.Type.Object)
                                  .Select(reference => new SelectorPopup.Item { name = reference.name,
                                                                                icon = objectIcon,
                                                                                color = reference.type.collection ? collectionColor
                                                                                                                  : Color.white,
                                                                                action = makething(reference) })
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
        function.body.Remove(dragging);
        EndDrag(dragging);

        SetFunction(function);
    }

    public void DropLine()
    {
        if (dragging == null) return;

        function.body.Remove(dragging);
        int index = function.body.IndexOf(hoveredLine);
        function.body.Insert(index + (hoveredAbove ? 0 : 1), dragging);

        EndDrag(dragging);

        SetFunction(function);
    }

    public void AddLine()
    {
        /*
        function.function.definition.Add(new AST.Line
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
        */

        SetFunction(function);
    }

    public void InsertLine()
    {
        Assert.IsNotNull(hoveredLine, "No line hovered to insert relative to!");

        /*
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

        int index = function.body.IndexOf(hoveredLine);
        function.body.Insert(index + (hoveredAbove ? 0 : 1), line);
        */

        SetFunction(function);
    }
}
