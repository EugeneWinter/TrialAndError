using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotsParent;
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    public Color selectedColor = new Color(0.8f, 0.8f, 0.3f, 0.9f);

    private Image[] slotBackgrounds;
    private Image[] slotIcons;
    private Text[] countTexts;

    void Start()
    {
        slotBackgrounds = new Image[9];
        slotIcons = new Image[9];
        countTexts = new Text[9];

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slotBackgrounds[i] = slot.GetComponent<Image>();

            Transform iconTransform = slot.transform.Find("Icon");
            if (iconTransform != null)
                slotIcons[i] = iconTransform.GetComponent<Image>();

            countTexts[i] = slot.GetComponentInChildren<Text>();
        }
    }

    void Update()
    {
        if (Inventory.Instance == null) return;

        for (int i = 0; i < 9; i++)
        {
            slotBackgrounds[i].color = (i == Inventory.Instance.selectedSlot) ? selectedColor : normalColor;

            ItemStack stack = Inventory.Instance.slots[i];

            if (stack.IsEmpty)
            {
                countTexts[i].text = "";
                if (slotIcons[i] != null)
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                countTexts[i].text = stack.count.ToString();

                if (slotIcons[i] != null && BlockIconGenerator.Instance != null)
                {
                    Sprite icon = BlockIconGenerator.Instance.GetIcon(stack.id);
                    if (icon != null)
                    {
                        slotIcons[i].sprite = icon;
                        slotIcons[i].color = Color.white;
                    }
                    else
                    {
                        slotIcons[i].sprite = null;
                        slotIcons[i].color = new Color(1, 1, 1, 0);
                    }
                }
            }
        }
    }
}