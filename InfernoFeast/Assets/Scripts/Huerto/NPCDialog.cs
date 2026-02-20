using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI de diálogo")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Button continueButton;

    private int dialogueIndex = 0;
    private List<string> dialogueLines = new List<string>();

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(ContinueDialogue);
    }

    public void StartDialogue(List<string> lines)
    {
        dialogueLines = lines;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[dialogueIndex];
    }

    private void ContinueDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex >= dialogueLines.Count)
        {
            EndDialogue();
        }
        else
        {
            dialogueText.text = dialogueLines[dialogueIndex];
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        // Opcional: cada NPC puede dar recompensas o no, según otro script
    }
}
