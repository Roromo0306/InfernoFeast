using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueBoss : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public List<string> lines;
    public float typingSpeed = 0.05f;
    public Timer timer;

    [Header("Movimiento del panel")]
    public Vector2 visiblePosition = new Vector2(0, 0);
    public Vector2 hiddenPosition = new Vector2(0, -300);

    private int index = 0;
    private bool isShowing = false;
    private bool isTyping = false;

    // EVENTO: se invoca cuando el panel termina y se cierra (después de iniciar el timer si timer != null)
    public Action OnDialogClosed;

    void Start()
    {
        dialogPanel.SetActive(false);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = hiddenPosition;
    }

    public void StartDialog(List<string> newLines)
    {
        lines = newLines;
        index = 0;
        dialogPanel.SetActive(true);
        StartCoroutine(AnimatePanel(true));
        ShowLine();
    }

    void Update()
    {
        if (isShowing && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogText.text = lines[index];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    void ShowLine()
    {
        dialogText.text = "";
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        foreach (char c in lines[index].ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void NextLine()
    {
        index++;
        if (index < lines.Count)
        {
            ShowLine();
        }
        else
        {
            StartCoroutine(ClosePanelAndStartTimer());
        }
    }

    private IEnumerator ClosePanelAndStartTimer()
    {
        yield return StartCoroutine(AnimatePanel(false));

        // Inicia el timer después de cerrar el panel (si hay timer asignado)
        if (timer != null)
        {
            timer.StartTimer();
        }

        // Notificamos al que quiera saber que el diálogo ha terminado
        OnDialogClosed?.Invoke();
    }

    IEnumerator AnimatePanel(bool show)
    {
        isShowing = show;

        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Vector2 target = show ? visiblePosition : hiddenPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            rt.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        if (!show)
        {
            dialogPanel.SetActive(false);
        }
    }
}
