using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ZoneType
    {
        WorkArea,       // 工作区域（包饺子的地方）
        FillingBowl,    // 馅料碗
        DoughStack      // 饺子皮堆
    }

    public ZoneType zoneType;
    public List<DraggableItem.ItemType> acceptedTypes = new List<DraggableItem.ItemType>();

    private bool isHighlighted = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 高亮显示可放置区域
        DraggableItem item = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (item != null && CanAcceptItem(item))
        {
            HighlightZone(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HighlightZone(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        HighlightZone(false);
    }

    public bool CanAcceptItem(DraggableItem item)
    {
        return acceptedTypes.Contains(item.itemType);
    }

    public void OnItemDropped(DraggableItem item)
    {
        // 将物品放置到这个区域
        item.transform.SetParent(transform);

        // 通知DumplingGameManager
        DumplingGameManager manager = FindObjectOfType<DumplingGameManager>();
        if (manager != null)
        {
            manager.OnItemPlaced(item, this);
        }
    }

    void HighlightZone(bool highlight)
    {
        isHighlighted = highlight;

        // 可以添加视觉反馈，比如改变颜色
        UnityEngine.UI.Image image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            if (highlight)
            {
                image.color = new Color(1f, 1f, 0.5f, 0.5f); // 黄色高亮
            }
            else
            {
                image.color = new Color(1f, 1f, 1f, 0.3f); // 正常颜色
            }
        }
    }
}
