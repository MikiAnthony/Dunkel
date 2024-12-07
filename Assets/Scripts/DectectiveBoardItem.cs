using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DectectiveBoardItem : MonoBehaviour
{
    [SerializeField] GameObject pin;
    [SerializeField] public bool isStickyNote; 
    public void OnMovingItem()
    {
        if(!isStickyNote && pin != null)
            pin.SetActive(false);
    }

    public void OnPlacingItem()
    {
        if (!isStickyNote && pin != null)
            pin.SetActive(true);
    }
}
