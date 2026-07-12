using UnityEngine;

public class HeldItemDisplay : MonoBehaviour
{
    public Transform handAnchor;

    private GameObject currentHeldObject;
    private ushort currentDisplayedId = 0;
    private ItemSO currentItem;

    void Update()
    {
        if (Inventory.Instance == null) return;

        ushort selectedId = Inventory.Instance.slots[Inventory.Instance.selectedSlot].id;

        if (selectedId != currentDisplayedId)
        {
            UpdateHeldItem(selectedId);
            currentDisplayedId = selectedId;
        }

#if UNITY_EDITOR
        if (currentHeldObject != null && currentItem != null)
        {
            currentHeldObject.transform.localPosition = currentItem.heldPosition;
            currentHeldObject.transform.localEulerAngles = currentItem.heldRotation;
            currentHeldObject.transform.localScale = currentItem.heldScale;
        }
#endif
    }

    void UpdateHeldItem(ushort id)
    {
        if (currentHeldObject != null)
        {
            Destroy(currentHeldObject);
            currentHeldObject = null;
            currentItem = null;
        }

        if (id == 0) return;

        ItemSO item = Inventory.Instance.itemDatabase.GetItem(id);
        if (item == null || item.heldModel == null) return;

        currentItem = item;
        currentHeldObject = Instantiate(item.heldModel, handAnchor);
        currentHeldObject.transform.localPosition = item.heldPosition;
        currentHeldObject.transform.localEulerAngles = item.heldRotation;
        currentHeldObject.transform.localScale = item.heldScale;

        SetLayerRecursively(currentHeldObject, LayerMask.NameToLayer("HeldItem"));
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}