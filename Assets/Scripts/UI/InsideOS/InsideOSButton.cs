using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class InsideOSButton : MonoBehaviour
{
    public UnityEvent OnPressed = null;
    public UnityEvent OnReleased = null;
    public UnityEvent OnDoubleClicked = null;
    
    public virtual void Select() {}
    public virtual void Deselect() {}

    public virtual void Clicked() {}

    public virtual void DoubleClicked()
    {
        OnDoubleClicked?.Invoke();
    }

    public virtual void Pressed()
    {
        OnPressed?.Invoke();
    }

    public virtual void Released()
    {
        OnReleased?.Invoke();
    }
}
