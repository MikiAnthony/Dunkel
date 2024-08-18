using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsideOSUIButton : InsideOSButton
{
    [SerializeField] private Image _normalEdge, _pressedEdge;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Vector3 _buttonPressedTextOffset = Vector2.zero;

    private Vector3 _textOrgPos = default;

    void Awake()
    {
        if (_title is not null)
            _textOrgPos = _title.rectTransform.anchoredPosition;
    }
    
    public override void Select()
    {
        // _highlight.enabled = true;
    }

    public override void Deselect()
    {
        // _highlight.enabled = false;
    }

    public override void Pressed()
    {
        base.Pressed();
        if (_title is not null)
            _title.rectTransform.anchoredPosition = _textOrgPos + _buttonPressedTextOffset;
        if (_normalEdge is not null)
            _normalEdge.enabled = false;
        if (_pressedEdge is not null)
            _pressedEdge.enabled = true;
    }

    public override void Released()
    {
        base.Released();
        if (_title is not null)
            _title.rectTransform.anchoredPosition = _textOrgPos;
        if (_normalEdge is not null)
            _normalEdge.enabled = true;
        if (_pressedEdge is not null)
            _pressedEdge.enabled = false;
    }
}
