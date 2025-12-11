using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(PanelScript))]
public class Panel_Item : MonoBehaviour
{
    private static Stack<Panel_Item> items = new Stack<Panel_Item>();
    private static List<Panel_Item> staticPanelItems = new List<Panel_Item>();

    public enum UiPanelItemType
    {
        NormalPanel,
        Popup,
        MainPanel,
        StaticMenu
    }

    private PanelScript _panel;
    private PanelScript Panel => _panel ??= GetComponent<PanelScript>();

    [FormerlySerializedAs("_panelType")] [SerializeField] private UiPanelItemType uiPanelItemType = UiPanelItemType.NormalPanel;
    [SerializeField] private bool hideStaticMenu;

    [SerializeField] private UnityEvent OnBack;

    private void Awake()
    {
        if (uiPanelItemType == UiPanelItemType.MainPanel)
        {
            if (items.Count > 0)
            {
                Debug.LogError("Multiple MainPanel instances detected!");
            }
            else
            {
                items.Push(this);
            }
        }

        Panel.onDisableEvent += OnDisablePanel;
    }

    private void OnEnable()
    {
        switch (uiPanelItemType)
        {
            case UiPanelItemType.NormalPanel:
                if (items.Peek() == this) // we are not new
                    return;
                
                Panel_Item previos = null;
                if (items.Count > 0)
                     previos = items.Peek();

                items.Push(this);
                SetStaticPanelActive(!hideStaticMenu);

                if (previos)
                {
                    previos.Panel.SetActive(false);
                }

                break;
            case UiPanelItemType.Popup:
                if (items.Peek() == this) // we are not new
                    return;
                items.Push(this);

                break;
            case UiPanelItemType.StaticMenu:
                if (!staticPanelItems.Contains(this))
                {
                    staticPanelItems.Add(this);
                }

                break;
        }
    }

    public static void Back()
    {
        Debug.Log("back");
        if (items.Count > 1)
        {
            var item = items.Pop();
            item.OnBack?.Invoke();
            item.Panel.SetActive(false);
            print("back : " + item.gameObject.name);
            
            if (item.uiPanelItemType != UiPanelItemType.Popup)
            {
                SetStaticPanelActive(!items.Peek().hideStaticMenu);
                items.Peek().Panel.SetActive(true);
            }
        }
        else
        {
            items.Peek()?.OnBack?.Invoke();
        }
    }


    private void OnDisablePanel()
    {
        if (items.Count == 0 || items.Peek() != this)
            return;
        print("back");
        Back();
    }

    private void OnDestroy()
    {
        if (uiPanelItemType == UiPanelItemType.MainPanel)
        {
            staticPanelItems.Clear();
            items.Clear();
        }
        else
        {
            staticPanelItems.Remove(this);
        }
    }

    private static void SetStaticPanelActive(bool active)
    {
        foreach (var item in staticPanelItems)
        {
            item?.Panel.SetActive(active);
        }
    }
}