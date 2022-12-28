using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEnsureVisible : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform ScrollRectTransform;
    private RectTransform ContentPanel;
    private RectTransform SelectedRectTransform;
    private GameObject LastSelected;

    private Vector2 TargetPos;

    void Start()
    {
        ScrollRectTransform = GetComponent<RectTransform>();

        if (ContentPanel == null)
            ContentPanel = GetComponent<ScrollRect>().content;

        TargetPos = ContentPanel.anchoredPosition;
    }

    void Update()
    {
        if (!_mouseHover)
            Autoscroll();
    }


    public void Autoscroll()
    {
        if (ContentPanel == null)
            ContentPanel = GetComponent<ScrollRect>().content;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return;
        }
        if (selected.transform.parent != ContentPanel.transform)
        {
            return;
        }
        if (selected == LastSelected)
        {
            return;
        }

        SelectedRectTransform = (RectTransform)selected.transform;
        TargetPos.x = ContentPanel.anchoredPosition.x;
        TargetPos.y = -(SelectedRectTransform.localPosition.y) - (SelectedRectTransform.rect.height / 2);
        TargetPos.y = Mathf.Clamp(TargetPos.y, 0, ContentPanel.sizeDelta.y - ScrollRectTransform.sizeDelta.y);

        ContentPanel.anchoredPosition = TargetPos;
        LastSelected = selected;
    }

    bool _mouseHover;
    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseHover = false;
    }
}
