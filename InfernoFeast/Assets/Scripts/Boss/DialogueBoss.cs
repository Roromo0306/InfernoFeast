using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBoss : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public List<string> lines;
    public float typingSpeed = 0.05f;

    [Header("Movimiento del panel")]
    public Vector2 visiblePosition = new Vector2(0, 0);   // posición cuando aparece
    public Vector2 hiddenPosition = new Vector2(0, -300); // posición cuando desaparece

    private int index = 0;
    private bool isShowing = false;
    private bool isTyping = false;

    void Start()
    {
        dialogPanel.SetActive(false);

        // aseguramos que empiece en la posición oculta
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = hiddenPosition;
    }

    public void StartDialog(List<string> newLines)
    {
        lines = newLines;
        index = 0;
        dialogPanel.SetActive(true);
        StartCoroutine(AnimatePanel(true)); // subir panel
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

    void NextLine()
    {
        index++;
        if (index < lines.Count)
        {
            ShowLine();
        }
        else
        {
            StartCoroutine(AnimatePanel(false)); // bajar panel
        }
    }

    IEnumerator AnimatePanel(bool show)
    {
        isShowing = show;

        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Vector2 target = show ? visiblePosition : hiddenPosition;

        float t = 0;
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
