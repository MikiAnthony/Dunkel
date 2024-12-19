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
    [SerializeField] private BoardView _boardView;
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
            case PlayerInteraction.MouseAction.RightClick:
                _currentSelectedObject = RayCastMouseClick();
                if (_currentSelectedObject != null)
                    RemoveItem();
                break;
        }
    }

    private DectectiveBoardItem RayCastMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100, LayerMask.GetMask("DectectiveBoardObject"));

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
            }
        }

        return ClosestItem;
    }

    private int AmountOfItemsOnPlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100, LayerMask.GetMask("DectectiveBoardObject"));

        return hits.Length;
    }

    public void AddNewItem(GameObject item, GameObject inventoryItem)
    {
        var currentMousePosition = Input.mousePosition;
        currentMousePosition.z = _cameraTarget.localPosition.z - 0.1f;

        GameObject newItem = Instantiate(item, Vector3.zero, item.transform.rotation, transform);
        newItem.transform.position = Camera.main.ScreenToWorldPoint(currentMousePosition);
        newItem.transform.localEulerAngles = new Vector3(0,0,0);

        _currentSelectedObject = newItem.GetComponent<DectectiveBoardItem>();
        _currentSelectedObject._inventoryItemRef = inventoryItem;
        _currentSelectedObject.OnMovingItem();
    }

    public void RemoveItem()
    {
        if (_currentSelectedObject._isStickyNote)
            return;

        PlayerInventory.Instance.AddItemToInventory(_currentSelectedObject._inventoryItemRef);
        Destroy(_currentSelectedObject.gameObject);
        _currentSelectedObject = null;
    }
}