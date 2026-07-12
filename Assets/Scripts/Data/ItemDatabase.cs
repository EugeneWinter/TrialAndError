using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> items;

    private Dictionary<ushort, ItemSO> _lookup;

    public ItemSO GetItem(ushort id)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(id, out var item) ? item : null;
    }

    private void Initialize()
    {
        _lookup = new Dictionary<ushort, ItemSO>();
        foreach (var item in items)
        {
            if (item != null) _lookup[item.id] = item;
        }
    }
}