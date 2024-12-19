using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _inspectViewUI;
    [SerializeField] private BoardView _boardViewUI;

    public void Start()
    {
        if(PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnAddItem += AddItemToInventory;
    }

    public void AddItemToInventory(GameObject item)
    {
        _boardViewUI.AddItemToInventory(item);
    }
}
