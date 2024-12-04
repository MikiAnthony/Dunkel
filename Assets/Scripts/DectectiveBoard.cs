using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DectectiveBoard : MonoBehaviour
{

    public void MouseActionUpdate(PlayerInteraction.MouseAction mouseAction)
    {
        switch (mouseAction)
        {
            case PlayerInteraction.MouseAction.Pressed:
                Debug.Log("PRESS");
                break;
            case PlayerInteraction.MouseAction.Released:
                Debug.Log("RELEASE");
                break;
            case PlayerInteraction.MouseAction.Drag:
                Debug.Log("DRAG");
                break;
        }
    }
}