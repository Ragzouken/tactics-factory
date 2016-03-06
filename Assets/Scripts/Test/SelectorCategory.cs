using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SelectorCategory : MonoBehaviour 
{
    [SerializeField] private Text titleText;
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private SelectorElement itemPrefab;

    private MonoBehaviourPooler<SelectorPopup.Item, SelectorElement> items;

    private SelectorPopup.Category category;

    private void Awake()
    {
        items = new MonoBehaviourPooler<SelectorPopup.Item, SelectorElement>(itemPrefab,
                                                                             itemContainer,
                                                                             (i, e) => e.Setup(i));
    }

    public void Setup(SelectorPopup.Category category)
    {
        this.category = category;

        titleText.text = category.title;

        items.SetActive(category.items);
    }

    public void Search(string query)
    {
        if (query == "")
        {
            items.SetActive(category.items);
        }
        else
        {
            items.SetActive(category.items.Where(item => item.name.Contains(query)));
        }
    }
}
