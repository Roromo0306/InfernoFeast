using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookController : MonoBehaviour
{
    [Header("UI References")]
    public Button openBook;
    public GameObject BookPanel;
    public Image pageImage;
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;

    [Header("Pages")]
    public Sprite[] pages;

    [Header("Options")]
    public bool loopPages = false;
    public bool enableRaycastDebug = false;

    private int currentIndex = 0;

    private void Start()
    {
        ValidateReferences();

        if (BookPanel != null)
        {
            BookPanel.SetActive(false);
        }

        AssignButton(openBook, OnOpen);
        AssignButton(prevButton, OnPrev);
        AssignButton(nextButton, OnNextButtonPressed);
        AssignButton(closeButton, OnClose);

        currentIndex = 0;
        UpdatePage();
        UpdateButtonStates();
    }

    private void Update()
    {
        if (BookPanel != null && !BookPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnNext();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnPrev();
        }
    }

    private void ValidateReferences()
    {
        if (EventSystem.current == null)
            Debug.LogError("[BookController] No hay EventSystem en la escena. Añade uno desde GameObject > UI > Event System.");

        if (BookPanel == null)
            Debug.LogWarning("[BookController] BookPanel no está asignado en el inspector.");

        if (pageImage == null)
            Debug.LogError("[BookController] pageImage no está asignado en el inspector.");

        if (prevButton == null)
            Debug.LogWarning("[BookController] prevButton no está asignado en el inspector.");

        if (nextButton == null)
            Debug.LogWarning("[BookController] nextButton no está asignado en el inspector.");

        if (closeButton == null)
            Debug.LogWarning("[BookController] closeButton no está asignado en el inspector.");

        if (openBook == null)
            Debug.LogWarning("[BookController] openBook no está asignado en el inspector.");

        if (pages == null || pages.Length == 0)
            Debug.LogWarning("[BookController] No hay páginas asignadas en el array pages.");
    }

    private void AssignButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void OnNextButtonPressed()
    {
        if (enableRaycastDebug)
        {
            DebugRaycastUnderPointer();
        }

        OnNext();
    }

    public void OnNext()
    {
        if (!HasPages())
            return;

        if (currentIndex < pages.Length - 1)
        {
            currentIndex++;
        }
        else if (loopPages)
        {
            currentIndex = 0;
        }

        UpdatePage();
        UpdateButtonStates();
    }

    public void OnPrev()
    {
        if (!HasPages())
            return;

        if (currentIndex > 0)
        {
            currentIndex--;
        }
        else if (loopPages)
        {
            currentIndex = pages.Length - 1;
        }

        UpdatePage();
        UpdateButtonStates();
    }

    public void NextButtonClicked()
    {
        OnNext();
    }

    public void PrevButtonClicked()
    {
        OnPrev();
    }

    public void OnOpen()
    {
        OpenBook(0);
    }

    public void OnClose()
    {
        if (BookPanel != null)
        {
            BookPanel.SetActive(false);
        }
    }

    public void OpenBook(int startIndex = 0)
    {
        if (BookPanel != null)
        {
            BookPanel.SetActive(true);
        }

        if (HasPages())
        {
            currentIndex = Mathf.Clamp(startIndex, 0, pages.Length - 1);
        }
        else
        {
            currentIndex = 0;
        }

        UpdatePage();
        UpdateButtonStates();
    }

    public int GetCurrentPageIndex()
    {
        return currentIndex;
    }

    private bool HasPages()
    {
        return pages != null && pages.Length > 0;
    }

    private void UpdatePage()
    {
        if (pageImage == null)
            return;

        if (!HasPages())
        {
            pageImage.sprite = null;
            pageImage.enabled = false;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, pages.Length - 1);
        Sprite currentPage = pages[currentIndex];

        pageImage.sprite = currentPage;
        pageImage.enabled = currentPage != null;
        pageImage.type = Image.Type.Simple;
        pageImage.preserveAspect = true;
        pageImage.color = Color.white;
    }

    private void UpdateButtonStates()
    {
        if (!HasPages())
        {
            if (prevButton != null) prevButton.interactable = false;
            if (nextButton != null) nextButton.interactable = false;
            return;
        }

        if (loopPages)
        {
            if (prevButton != null) prevButton.interactable = true;
            if (nextButton != null) nextButton.interactable = true;
            return;
        }

        if (prevButton != null) prevButton.interactable = currentIndex > 0;
        if (nextButton != null) nextButton.interactable = currentIndex < pages.Length - 1;
    }

    private void DebugRaycastUnderPointer()
    {
        if (EventSystem.current == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = null;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
        }

        if (raycaster == null)
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
        }

        if (raycaster == null)
            return;

        raycaster.Raycast(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            Debug.Log("[BookController] UI bajo el ratón: " + results[i].gameObject.name);
        }
    }
}