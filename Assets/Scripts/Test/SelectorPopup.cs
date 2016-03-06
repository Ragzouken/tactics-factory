using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SelectorPopup : MonoBehaviour 
{
    public class Item
    {
        public string name;
        public Sprite icon;
        public Action action; 
    }

    public class Category
    {
        public string title;
        public IEnumerable<Item> items;
    }

    [SerializeField] private Text titleText;
    [SerializeField] private InputField searchInput;
    [SerializeField] private Button searchCancelButton;

    [Header("Content")]
    [SerializeField] private RectTransform categoryContainer;
    [SerializeField] private SelectorCategory categoryPrefab;

    private MonoBehaviourPooler<Category, SelectorCategory> categories;

    private void Awake()
    {
        searchInput.onValueChanged.AddListener(value => Refresh());
        searchCancelButton.onClick.AddListener(() => searchInput.text = "");

        categories = new MonoBehaviourPooler<Category, SelectorCategory>(categoryPrefab,
                                                                         categoryContainer,
                                                                         (c, e) => e.Setup(c));
    }

    public void SetCategories(IEnumerable<Category> categories)
    {
        this.categories.SetActive(categories);
    }

    public void Refresh()
    {
        categories.MapActive((c, e) => e.Search(searchInput.text));
    }
}
