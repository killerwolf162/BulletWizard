using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemChest
{
    public static readonly Dictionary<Collider2D, ItemChest> collLookup = new Dictionary<Collider2D, ItemChest>();
    public event Action<ItemChest> ChestOpened;

    [Header("General")]
    private Collider2D _col;
    public GameObject _gameObject { get; private set; }
    private AbstractItem _item;

    public ItemChest(GameObject gameObject)
    {
        _gameObject = gameObject;
        _col = _gameObject.GetComponent<Collider2D>();

        collLookup[_col] = this;
        _item = ItemPool.TakeNext();
    }

    public void OnActivation(PlayerController player)
    {
        if (_item != null)
        {
            // The item knows how to apply itself — the chest just calls it.
            _item.Apply(player);
            Debug.Log($"Player received: {_item.Name} — {_item.Description}");
        }

        collLookup.Remove(_col);
        ChestOpened?.Invoke(this);
    }
}
