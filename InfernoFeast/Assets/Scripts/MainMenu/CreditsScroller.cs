using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float exitPosition = 2000f;
    [SerializeField] private float startPosition = -800f;

    private RectTransform rectTransform;
    private float initialSpeed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialSpeed = scrollSpeed;
    }

    void OnEnable()
    {
        if (rectTransform != null)
        {
            // Reset de posición al punto de inicio
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startPosition);
            // Reset de velocidad por si se había detenido al llegar al final
            scrollSpeed = initialSpeed;
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y > exitPosition)
        {
            scrollSpeed = 0;
        }
    }
}