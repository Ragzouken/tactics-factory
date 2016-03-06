using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SelectorElement : MonoBehaviour 
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text labelText;
    [SerializeField] private Button selectButton;

    private SelectorPopup.Item item;

    private void Awake()
    {
        selectButton.onClick.AddListener(() => item.action());
    }

    public void Setup(SelectorPopup.Item item)
    {
        this.item = item;

        iconImage.sprite = item.icon;
        iconImage.color = item.color;
        iconImage.gameObject.SetActive(item.icon != null);

        labelText.text = item.name;
    }
}
