using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private List<GameObject> _inventoryItems = new List<GameObject>();

    public delegate void InventoryManager(GameObject item);
    public event InventoryManager OnAddItem;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddItemToInventory(GameObject item)
    {
        _inventoryItems.Add(item);
        OnAddItem.Invoke(item);
    }
}
