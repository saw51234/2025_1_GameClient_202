using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDatabase", menuName  = "Inventory/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    private Dictionary<int, ItemSO> itemById;
    private Dictionary<string, ItemSO> itemByName;

    public void Initialize()
    {
        itemById = new Dictionary<int, ItemSO>();
        itemByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)
        {
            itemById[item.id] = item;
            itemByName[item.itemName] = item;
        }
    }

    public ItemSO GetItemById(int id)
    {
        if(itemById == null)
        {
            Initialize();
        }
        if (itemById.TryGetValue(id, out ItemSO item))
            return item;
        return null;
    }

    public ItemSO GetItemByName(string name)
    {
        if (itemByName == null)
        {
            Initialize();
        }
        if (itemByName.TryGetValue(name, out ItemSO item))
            return item;

        return null;
    }

    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}
