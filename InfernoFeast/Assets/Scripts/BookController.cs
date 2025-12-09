using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BookController : MonoBehaviour
{
    [Header("UI References")]
    public Button openBook;
    public GameObject BookPanel;
    public Image pageImage;          // Image UI que mostrará la página
    public Button prevButton;        // Botón Anterior
    public Button nextButton;        // Botón Siguiente
    public Button closeButton;       // Botón Cerrar

    [Header("Pages")]
    public Sprite[] pages;           // Array de Sprites con las páginas

    [Header("Options")]
    public bool loopPages = false;   // ¿Volver al inicio cuando llegas al final?
    public bool enableRaycastDebug = true; // activar debug del raycast cuando se pulsa next (útil para encontrar UI encima)

    private int currentIndex = 0;

    void Awake()
    {
        Debug.Log("[BookController] Awake");
    }

    void Start()
    {
        Debug.Log("[BookController] Start - inicializando...");

        // Asegurarnos que el EventSystem existe (si falta, la UI no recibe clicks)
        if (EventSystem.current == null)
        {
            Debug.LogError("[BookController] EventSystem NO encontrado en la escena. Añade Window -> UI -> Event System.");
        }
        else
        {
            Debug.Log("[BookController] EventSystem encontrado: " + EventSystem.current.gameObject.name);
        }

        // Iniciar oculto (si existe)
        if (BookPanel != null)
        {
            BookPanel.SetActive(false);
            Debug.Log("[BookController] BookPanel asignado y ocultado.");
        }
        else
        {
            Debug.LogWarning("[BookController] BookPanel NO asignado en el inspector.");
        }

        // Comprobaciones básicas
        if (pageImage == null) Debug.LogError("[BookController] Asigna 'pageImage' en el inspector.");
        if (prevButton == null) Debug.LogError("[BookController] Asigna 'prevButton' en el inspector.");
        if (nextButton == null) Debug.LogError("[BookController] Asigna 'nextButton' en el inspector.");
        if (closeButton == null) Debug.LogError("[BookController] Asigna 'closebutton' en el inspector.");
        if (openBook == null) Debug.LogWarning("[BookController] No has asignado 'openBook' (botón que abre).");
        if (pages == null || pages.Length == 0) Debug.LogWarning("[BookController] No hay páginas asignadas en 'pages'.");

        // Forzar interactable para pruebas (si quieres eliminarlo quita estas dos líneas)
        if (prevButton != null) prevButton.interactable = true;
        if (nextButton != null) nextButton.interactable = true;

        // (Re)asignar listeners de forma segura
        TryAssignButton(prevButton, OnPrev, "prevButton");
        TryAssignButton(nextButton, OnNext, "nextButton");
        TryAssignButton(closeButton, OnClose, "closeButton");

        if (openBook != null)
        {
            // limpiamos y asignamos OnOpen
            openBook.onClick.RemoveAllListeners();
            openBook.onClick.AddListener(OnOpen);
            Debug.Log("[BookController] Listener añadido a openBook.");
        }

        // Añadir listener extra de debug para nextButton (no rompe nada, ayuda a detectar problemas)
        if (nextButton != null)
        {
            // No removemos listeners aquí (TryAssignButton ya lo hizo). Añadimos un listener adicional
            nextButton.onClick.AddListener(() =>
            {
                Debug.Log("[BookController] nextButton clicked (lambda listener)");
                if (enableRaycastDebug) DebugRaycastUnderPointer();
                // Llamamos explícitamente a OnNext (seguro aunque TryAssignButton ya lo haya asignado)
                OnNext();
            });
        }

        // Inicializar page index y UI
        currentIndex = 0;
        UpdatePage();
        UpdateButtonStates();

        Debug.Log($"[BookController] Start complete. pages count = {(pages != null ? pages.Length.ToString() : "null")} | currentIndex = {currentIndex}");
    }

    // Intento seguro de asignar listener y reportar problemas
    private void TryAssignButton(Button btn, UnityEngine.Events.UnityAction action, string name)
    {
        if (btn == null)
        {
            Debug.LogWarning($"[BookController] TryAssignButton: {name} es NULL, no se puede asignar listener.");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
        Debug.Log($"[BookController] Listener asignado a {name}. interactable={btn.interactable}");

        // Comprobar si hay CanvasGroup que bloquee (común al poner paneles encima)
        CanvasGroup cg = btn.GetComponentInParent<CanvasGroup>();
        if (cg != null)
        {
            Debug.Log($"[BookController] CanvasGroup encontrado en parent de {name}: interactable={cg.interactable} blocksRaycasts={cg.blocksRaycasts} alpha={cg.alpha}");
        }
    }

    // Métodos que llamamos desde listeners (y también públicos para poder asignarlos manualmente desde el inspector).
    public void OnNext()
    {
        Debug.Log($"[BookController] OnNext called. currentIndex BEFORE = {currentIndex}");
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("[BookController] OnNext: No hay páginas.");
            return;
        }

        if (currentIndex < pages.Length - 1)
        {
            currentIndex++;
            Debug.Log($"[BookController] OnNext: incremento -> {currentIndex}");
        }
        else if (loopPages)
        {
            currentIndex = 0;
            Debug.Log("[BookController] OnNext: loop -> 0");
        }
        else
        {
            Debug.Log("[BookController] OnNext: ya estás en la última página.");
        }

        UpdatePage();
        UpdateButtonStates();
    }

    public void OnPrev()
    {
        Debug.Log($"[BookController] OnPrev called. currentIndex BEFORE = {currentIndex}");
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("[BookController] OnPrev: No hay páginas.");
            return;
        }

        if (currentIndex > 0)
        {
            currentIndex--;
            Debug.Log($"[BookController] OnPrev: decremento -> {currentIndex}");
        }
        else if (loopPages)
        {
            currentIndex = pages.Length - 1;
            Debug.Log("[BookController] OnPrev: loop -> última");
        }
        else
        {
            Debug.Log("[BookController] OnPrev: ya estás en la primera página.");
        }

        UpdatePage();
        UpdateButtonStates();
    }

    // Métodos públicos alternativos para enlazar desde Inspector si hay conflicto con listeners
    public void NextButtonClicked() { Debug.Log("[BookController] NextButtonClicked() llamado (método público)."); OnNext(); }
    public void PrevButtonClicked() { Debug.Log("[BookController] PrevButtonClicked() llamado (método público)."); OnPrev(); }

    private void UpdatePage()
    {
        if (pages == null || pages.Length == 0)
        {
            if (pageImage != null) pageImage.sprite = null;
            Debug.LogWarning("[BookController] UpdatePage: no hay páginas.");
            return;
        }

        if (pageImage == null)
        {
            Debug.LogError("[BookController] UpdatePage: 'pageImage' NO está asignado en el inspector.");
            return;
        }

        if (currentIndex < 0 || currentIndex >= pages.Length)
        {
            Debug.LogError("[BookController] UpdatePage: currentIndex fuera de rango: " + currentIndex);
            currentIndex = Mathf.Clamp(currentIndex, 0, pages.Length - 1);
        }

        Sprite s = pages[currentIndex];
        if (s == null)
        {
            Debug.LogError("[BookController] UpdatePage: sprite en pages[" + currentIndex + "] es null.");
            pageImage.sprite = null;
            return;
        }

        // Asignación sin tocar tamaño ni Canvas
        pageImage.sprite = s;
        pageImage.enabled = true;
        pageImage.type = Image.Type.Simple;
        pageImage.preserveAspect = true;
        pageImage.color = Color.white;

        Debug.Log("[BookController] UpdatePage: mostrando página " + currentIndex + " -> " + s.name);
    }

    private void UpdateButtonStates()
    {
        if (pages == null || pages.Length == 0)
        {
            if (prevButton != null) prevButton.interactable = false;
            if (nextButton != null) nextButton.interactable = false;
            return;
        }

        if (loopPages)
        {
            if (prevButton != null) prevButton.interactable = true;
            if (nextButton != null) nextButton.interactable = true;
        }
        else
        {
            if (prevButton != null) prevButton.interactable = currentIndex > 0;
            if (nextButton != null) nextButton.interactable = currentIndex < pages.Length - 1;
        }

        Debug.Log($"[BookController] UpdateButtonStates: prev.interactable={(prevButton != null ? prevButton.interactable.ToString() : "null")} next.interactable={(nextButton != null ? nextButton.interactable.ToString() : "null")}");
    }

    // Cerrar: por defecto desactiva el panel del libro
    public void OnClose()
    {
        if (BookPanel != null) BookPanel.SetActive(false);
        Debug.Log("[BookController] OnClose called. BookPanel hidden.");
    }

    // Método usado por el botón openBook
    public void OnOpen()
    {
        Debug.Log("[BookController] OnOpen called.");
        OpenBook(0);
    }

    // Abrir el libro por código
    public void OpenBook(int startIndex = 0)
    {
        if (BookPanel != null) BookPanel.SetActive(true);

        currentIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, (pages != null ? pages.Length - 1 : 0)));
        UpdatePage();
        UpdateButtonStates();

        Debug.Log("[BookController] OpenBook: abierto con página " + currentIndex);
    }

    void Update()
    {
        // Permite probar con teclado (flechas) si los botones UI fallan
        if (Input.GetKeyDown(KeyCode.RightArrow)) { Debug.Log("[BookController] Key RightArrow pressed."); OnNext(); }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { Debug.Log("[BookController] Key LeftArrow pressed."); OnPrev(); }
    }

    public int GetCurrentPageIndex() => currentIndex;

    // ----------------------------
    // UTIL: Depuración de raycasts
    // ----------------------------
    private void DebugRaycastUnderPointer()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[BookController] DebugRaycastUnderPointer: EventSystem.current es null.");
            return;
        }

        PointerEventData ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();

        // Intentamos obtener GraphicRaycaster del canvas padre primero
        GraphicRaycaster gr = null;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) gr = parentCanvas.GetComponent<GraphicRaycaster>();

        if (gr == null)
        {
            // si no hay, buscamos cualquiera en la escena (útil en escenas pequeñas)
            gr = FindObjectOfType<GraphicRaycaster>();
        }

        if (gr == null)
        {
            Debug.LogWarning("[BookController] DebugRaycastUnderPointer: No GraphicRaycaster encontrado en la escena.");
            return;
        }

        gr.Raycast(ped, results);
        Debug.Log($"[BookController] Raycast at {Input.mousePosition} found {results.Count} results:");
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            Debug.Log($"  {i}: go={r.gameObject.name} module={r.module} depth={r.depth} worldPos={r.worldPosition} screenPos={r.screenPosition}");
        }
    }
}
