using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class BoardView : MonoBehaviour
{
    [SerializeField] private RectTransform _objectView;
    [SerializeField] private GameObject[] _inventoryItems;

    [SerializeField] private DectectiveBoard _dectectiveBoard;

    public void Awake()
    {
        /*
        foreach (GameObject item in _inventoryItems)
        {
            AddItemToInventory(item);
        }
        */
    }

    public void AddItemToDectectiveBoard(GameObject boardItem, GameObject inventoryItemRef)
    {
        _dectectiveBoard.AddNewItem(boardItem, inventoryItemRef);
    }

    public void AddItemToInventory(GameObject item)
    {
        BoardItem boardItem = Instantiate(item, _objectView).GetComponent<BoardItem>();

        boardItem.OnInstantiate(this, item);
    }

}
