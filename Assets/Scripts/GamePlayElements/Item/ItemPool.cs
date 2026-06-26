using UnityEngine;
using System.Collections.Generic;

public static class ItemPool
{
    private static Queue<AbstractItem> _pool;

    public static void Initialize()
    {
        // Build the full list of all available items.
        var allItems = new List<AbstractItem>
        {
            new HealthUpgradeItem(),
            new ManaUpgradeItem(),
        };

        // Randomize itemchests
        for (int i = allItems.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allItems[i], allItems[j]) = (allItems[j], allItems[i]);
        }

        _pool = new Queue<AbstractItem>(allItems);
    }

    public static AbstractItem TakeNext()
    {
        if (_pool == null || _pool.Count == 0)
        {
            Debug.LogWarning("ItemPool is empty, no item");
            return null;
        }
        return _pool.Dequeue();
    }
}
