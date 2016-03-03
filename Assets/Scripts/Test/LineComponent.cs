using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LineComponent : MonoBehaviour 
{
    [Header("Reference")]
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
    }

    public void Setup(AST.Component component)
    {
        if (component.comment == null)
        {
            typeImage.sprite = sprites[component.type.type];
            typeImage.color = component.type.collection ? collectionColor
                                                        : singleColor;

            nameText.text = component.name;
        }
        else
        {
            commentText.text = component.comment;
        }

        referenceObject.SetActive(component.comment == null);
        commentObject.SetActive(component.comment != null);
    }
}
