using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum ItemType
    {
        DoughSkin,      // 饺子皮
        Filling,        // 馅料
        Tool            // 工具（如擀面杖、碗等）
    }

    public ItemType itemType;
    public string itemName;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        // 使物品在拖拽时半透明
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 将物品移到最上层
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 检查是否放置在有效区域
        bool placed = false;

        // 尝试与DropZone交互
        if (eventData.pointerEnter != null)
        {
            DropZone dropZone = eventData.pointerEnter.GetComponent<DropZone>();
            if (dropZone != null && dropZone.CanAcceptItem(this))
            {
                dropZone.OnItemDropped(this);
                placed = true;
            }
        }

        // 如果没有成功放置，返回原位
        if (!placed)
        {
            ReturnToOriginalPosition();
        }
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }

    public void SetOriginalPosition(Vector2 position, Transform parent)
    {
        originalPosition = position;
        originalParent = parent;
    }
}
