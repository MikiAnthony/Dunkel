using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsideOSIcon : InsideOSButton
{
    [SerializeField] private Image _highlight;
    [SerializeField] private TextMeshProUGUI _title;
    
    public override void Select()
    {
        _highlight.enabled = true;
        _title.fontStyle = FontStyles.Underline | FontStyles.Bold;
    }

    public override void Deselect()
    {
        _highlight.enabled = false;
        _title.fontStyle = FontStyles.Normal;
    }

    public override void Clicked()
    {
        
    }
}
