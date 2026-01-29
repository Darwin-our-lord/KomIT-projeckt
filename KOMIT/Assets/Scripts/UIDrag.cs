using Alteruna;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    bool checkforCollision = false;
    [SerializeField] private int answerId; // ID of this answer
    public GameManager manager;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetAnswerId(int id)
    {
        answerId = id;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
        checkforCollision = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        checkforCollision = true;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (checkforCollision) 
        {
            if (collision.CompareTag("ColorZone"))
            {
                checkforCollision =false;
                manager.CheckRightColorMatch(answerId,collision.GetComponent<ColorZone>().answerId);
                if (answerId == collision.GetComponent<ColorZone>().answerId) Destroy(this);
            }
        }
    }
}
