using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LineComponent : MonoBehaviour 
{
    [SerializeField] private TestEditor editor;

    [Header("Reference")]
    [SerializeField] private Button referenceEditButton;
    [SerializeField] private GameObject referenceObject;
    [SerializeField] private Image typeImage;
    [SerializeField] private Text nameText;

    [Header("Comment")]
    [SerializeField] private GameObject commentObject;
    [SerializeField] private Text commentText;

    [Header("Types")]
    [SerializeField] private Color singleColor;
    [SerializeField] private Color collectionColor;

    [SerializeField] private Sprite booleanSprite;
    [SerializeField] private Sprite numberSprite;
    [SerializeField] private Sprite objectSprite;
    [SerializeField] private Sprite positionSprite;
    [SerializeField] private Sprite actionSprite;

    private Dictionary<AST.Type, Sprite> sprites;

    private AST.Component component;

    private void Awake()
    {
        sprites = new Dictionary<AST.Type, Sprite>
        {
            { AST.Type.Boolean,  booleanSprite  },
            { AST.Type.Number,   numberSprite   },
            { AST.Type.Object,   objectSprite   },
            { AST.Type.Position, positionSprite },
            { AST.Type.Action,   actionSprite   },
        };

        referenceEditButton.onClick.AddListener(() => editor.EditInput(component.line, component.index));
    }

    public void Setup(AST.Component component)
    {
        this.component = component;

        Refresh();
    }

    public void Refresh()
    {
        if (component.comment == null)
        {
            var type = component.reference.type;

            typeImage.sprite = sprites[type.type];
            typeImage.color = type.collection ? collectionColor
                                              : singleColor;

            nameText.text = component.reference.name;
        }
        else
        {
            commentText.text = component.comment;
        }

        referenceObject.SetActive(component.comment == null);
        commentObject.SetActive(component.comment != null);
    }
}
