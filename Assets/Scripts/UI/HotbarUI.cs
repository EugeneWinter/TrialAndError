using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotsParent;
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    public Color selectedColor = new Color(0.8f, 0.8f, 0.3f, 0.9f);

    private Image[] slotImages;
    private Text[] countTexts;

    void Start()
    {
        slotImages = new Image[9];
        countTexts = new Text[9];

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slotImages[i] = slot.GetComponent<Image>();
            countTexts[i] = slot.GetComponentInChildren<Text>();
        }
    }

    void Update()
    {
        if (Inventory.Instance == null) return;

        for (int i = 0; i < 9; i++)
        {
            slotImages[i].color = (i == Inventory.Instance.selectedSlot) ? selectedColor : normalColor;

            ItemStack stack = Inventory.Instance.slots[i];
            if (stack.IsEmpty)
            {
                countTexts[i].text = "";
            }
            else
            {
                countTexts[i].text = stack.count.ToString();
            }
        }
    }
}