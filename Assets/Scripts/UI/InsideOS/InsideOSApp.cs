using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum AppType
{
    HeartMonitor,
    Painter
}

public abstract class InsideOSApp : MonoBehaviour
{
    public AppType App = default;
    [SerializeField] protected GameObject _windowContainer = null;

    protected InsideOS _insideOS = null;
    
    public abstract void UpdateApp(Vector2 cursorPosition);
    public abstract void InitializeApp();

    public virtual void Open(InsideOS insideOS)
    {
        _windowContainer.SetActive(true);
        _insideOS = insideOS;
        InitializeApp();
    }

    public virtual void Close()
    {
        _windowContainer.SetActive(true);
        _insideOS.CloseApp(App);
    }

}
