using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int slotCount = 9;
    public ItemStack[] slots;
    public int selectedSlot = 0;

    public ItemDatabase itemDatabase;

    private static readonly Key[] HotbarKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3,
        Key.Digit4, Key.Digit5, Key.Digit6,
        Key.Digit7, Key.Digit8, Key.Digit9
    };

    void Awake()
    {
        Instance = this;
        slots = new ItemStack[slotCount];
        slots[0] = new ItemStack { id = 1000, count = 1 };
        slots[1] = new ItemStack { id = 1001, count = 5 };
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        float scroll = InputManager.Instance.ScrollHotbar;
        if (scroll > 0.1f) selectedSlot--;
        if (scroll < -0.1f) selectedSlot++;

        if (selectedSlot < 0) selectedSlot = slotCount - 1;
        if (selectedSlot >= slotCount) selectedSlot = 0;

        if (Keyboard.current != null)
        {
            for (int i = 0; i < HotbarKeys.Length; i++)
            {
                if (Keyboard.current[HotbarKeys[i]].wasPressedThisFrame)
                {
                    selectedSlot = i;
                    break;
                }
            }
        }
    }

    public ushort GetSelectedBlockID()
    {
        ushort id = slots[selectedSlot].id;
        if (id == 0) return 0;

        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(id);
        return block != null ? id : (ushort)0;
    }

    public ItemSO GetSelectedTool()
    {
        ushort id = slots[selectedSlot].id;
        if (id == 0) return null;

        ItemSO item = itemDatabase.GetItem(id);
        if (item != null && item.itemType == ItemType.Tool) return item;

        return null;
    }

    public bool AddItem(ushort id, int count = 1)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].id == id && slots[i].count < 64)
            {
                slots[i].count += count;
                return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i] = new ItemStack { id = id, count = count };
                return true;
            }
        }

        return false;
    }

    public bool RemoveSelected()
    {
        if (slots[selectedSlot].IsEmpty) return false;

        slots[selectedSlot].count--;
        if (slots[selectedSlot].count <= 0)
            slots[selectedSlot] = ItemStack.Empty;

        return true;
    }
}