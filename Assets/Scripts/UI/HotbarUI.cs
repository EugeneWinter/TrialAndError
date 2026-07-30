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

    private ushort[] previousIds;
    private int[] previousCounts;

    void Start()
    {
        slotBackgrounds = new Image[9];
        slotIcons = new Image[9];
        countTexts = new Text[9];
        previousIds = new ushort[9];
        previousCounts = new int[9];

        for (int i = 0; i < 9; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slotBackgrounds[i] = slot.GetComponent<Image>();

            Transform iconTransform = slot.transform.Find("Icon");
            if (iconTransform != null)
            {
                slotIcons[i] = iconTransform.GetComponent<Image>();
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1, 1, 1, 0);
            }

            countTexts[i] = slot.GetComponentInChildren<Text>();
            countTexts[i].text = "";

            previousIds[i] = 0;
            previousCounts[i] = 0;
        }
    }

    void Update()
    {
        if (Inventory.Instance == null) return;

        for (int i = 0; i < 9; i++)
        {
            slotBackgrounds[i].color = (i == Inventory.Instance.selectedSlot) ? selectedColor : normalColor;

            ItemStack current = Inventory.Instance.slots[i];
            ushort currentId = current.id;
            int currentCount = current.count;

            // Обновляем только если что-то изменилось
            if (currentId != previousIds[i] || currentCount != previousCounts[i])
            {
                if (current.IsEmpty)
                {
                    if (slotIcons[i] != null)
                    {
                        slotIcons[i].sprite = null;
                        slotIcons[i].color = new Color(1, 1, 1, 0);
                    }
                    countTexts[i].text = "";
                }
                else
                {
                    Sprite icon = BlockIconGenerator.Instance != null
                        ? BlockIconGenerator.Instance.GetIcon(currentId)
                        : null;

                    if (slotIcons[i] != null)
                    {
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

                    countTexts[i].text = (currentCount > 1) ? currentCount.ToString() : "";
                }

                previousIds[i] = currentId;
                previousCounts[i] = currentCount;
            }
        }
    }
}