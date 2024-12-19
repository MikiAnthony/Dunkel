using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DectectiveBoardItem : MonoBehaviour
{
    [SerializeField] GameObject _pin;
    [SerializeField] public bool _isStickyNote;

    public GameObject _inventoryItemRef;
    public void OnMovingItem()
    {
        if(!_isStickyNote && _pin != null)
            _pin.SetActive(false);
    }

    public void OnPlacingItem()
    {
        if (!_isStickyNote && _pin != null)
            _pin.SetActive(true);
    }
}
