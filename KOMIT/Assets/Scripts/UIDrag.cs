using Alteruna;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Image image;
    private bool isInitialized = false; // Track if we have references

    bool checkforCollision = false;
    [SerializeField] private int answerId;
    public GameManager manager;

    void Awake()
    {
        Initialize();
    }

    // Moved initialization to a separate method so it can be called safely
    private void Initialize()
    {
        if (isInitialized) return;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();
        originalPosition = rectTransform.anchoredPosition;

        isInitialized = true;
    }

    public void SetAnswerId(int id)
    {
        answerId = id;
    }

    public void ResetDragObject()
    {
        // Ensure we have our references before trying to use them
        Initialize();

        this.enabled = true;
        if (image != null) image.enabled = true;
        if (rectTransform != null) rectTransform.anchoredPosition = originalPosition;

        checkforCollision = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Initialize(); // Safety check
        rectTransform.SetAsLastSibling();
        checkforCollision = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Initialize();
        rectTransform.anchoredPosition += eventData.delta /*/ canvas.scaleFactor*/;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        checkforCollision = true;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (checkforCollision)
        {
            if (collision.CompareTag("ColorZone"))
            {
                ColorZone zone = collision.GetComponent<ColorZone>();
                if (zone != null)
                {
                    manager.CheckRightColorMatch(answerId, zone.answerId);

                    if (answerId == zone.answerId)
                    {
                        if (image != null) image.enabled = false;
                        //this.enabled = false;
                    }
                    checkforCollision = false;
                }
            }
        }
    }
}