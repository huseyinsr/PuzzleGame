using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] public ItemData item;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;
    public bool isSelected;

    public bool IsEmpty() => item == null;

    public void SetItem(ItemData newItem)
    {
        item = newItem;
        if (icon == null) return;
        icon.sprite = (item != null && item.icon != null) ? item.icon : null;
        icon.enabled = item != null && item.icon != null;
    }

    public void Clear()
    {
        item = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (icon != null) icon.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnSlotClicked(this);

            if (!IsEmpty())
                SoundEffect.Instance?.PlayInventory();
        }
    }
}
