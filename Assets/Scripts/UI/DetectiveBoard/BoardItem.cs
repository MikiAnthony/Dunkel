using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Canvas _canvas;
    [SerializeField] RectTransform _pictureObject;

    [SerializeField] GameObject _boardItemObject;
    
    private BoardView _boardView;
    private GameObject _inventoryItemRef;
    private PointerEventData.InputButton? _button;
    public void OnInstantiate(BoardView boardView, GameObject inventoryItem)
    {
        _boardView = boardView;
        _inventoryItemRef = inventoryItem;
    }
    public void OnHoverEnter()
    {
        _canvas.sortingOrder = 10;

        _pictureObject.offsetMax = new Vector2(_pictureObject.offsetMax.x, 0f);
        _pictureObject.offsetMin = new Vector2(_pictureObject.offsetMin.x, 0f);
    }

    public void OnHoverExit()
    {
        _canvas.sortingOrder = 0;

        _pictureObject.offsetMax = new Vector2(_pictureObject.offsetMax.x, -140f);
        _pictureObject.offsetMin = new Vector2(_pictureObject.offsetMin.x, -140f);
    }

    public void OnClick()
    {
        if (_button == null)
            return;

        if (_button == PointerEventData.InputButton.Left)
        {
            _boardView.AddItemToDectectiveBoard(_boardItemObject, _inventoryItemRef);
            Destroy(transform.gameObject);

            _button = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _button = eventData.button;
    }
}
