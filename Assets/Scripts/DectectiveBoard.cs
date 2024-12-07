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
    [SerializeField] private Transform _cameraTarget;
    public Camera _camera;
    private DectectiveBoardItem _currentSelectedObject;
    private Vector3 _currentSelectionOffset = Vector3.zero;

    public void MouseActionUpdate(PlayerInteraction.MouseAction mouseAction)
    {
        switch (mouseAction)
        {
            case PlayerInteraction.MouseAction.Pressed:
                _currentSelectedObject = RayCastMouseClick();
                if (_currentSelectedObject != null)
                    _currentSelectedObject.OnMovingItem();
                break;
            case PlayerInteraction.MouseAction.Released:
                if (_currentSelectedObject != null)
                {
                    var currentMousePosition = Input.mousePosition;
                    currentMousePosition.z = _cameraTarget.localPosition.z - 0.05f;
                    currentMousePosition.z -= (AmountOfItemsOnPlacement() - 1) * 0.005f;
                    _currentSelectedObject.transform.position = Camera.main.ScreenToWorldPoint(currentMousePosition) + _currentSelectionOffset;

                    _currentSelectedObject.OnPlacingItem();
                }
                _currentSelectedObject = null;
                _currentSelectionOffset = Vector3.zero;
                break;
            case PlayerInteraction.MouseAction.Drag:
                if(_currentSelectedObject != null)
                {
                    var currentMousePosition = Input.mousePosition;
                    currentMousePosition.z = _cameraTarget.localPosition.z - 0.1f;
                    _currentSelectedObject.transform.position = Camera.main.ScreenToWorldPoint(currentMousePosition) + _currentSelectionOffset;
                }
                break;
        }
    }

    private DectectiveBoardItem RayCastMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100, 1 << 8);

        DectectiveBoardItem ClosestItem = null;

        float highestZPos = 0;
        for(int i = 0 ; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            GameObject item = hit.transform.gameObject;
            if (item.transform.localPosition.z > highestZPos)
            {
                ClosestItem = item.GetComponent<DectectiveBoardItem>();
                highestZPos = item.transform.localPosition.z;

                var currentMousePosition = Input.mousePosition;
                currentMousePosition.z = _cameraTarget.localPosition.z - highestZPos;
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(currentMousePosition);
                _currentSelectionOffset = item.transform.position - clickPos;
                Debug.Log(_currentSelectionOffset);
            }
        }

        return ClosestItem;
    }

    private int AmountOfItemsOnPlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100, 1 << 8);

        return hits.Length;
    }
}