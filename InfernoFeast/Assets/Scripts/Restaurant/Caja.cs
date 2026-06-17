using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Caja : MonoBehaviour
{
    [Header("Listas de ingredientes y para la UI")]
    public List<TipoIngrediente> Ingredientes;
    public List<Sprite> ImagenesUI;
    public List<Image> Botones;
    public List<TextMeshProUGUI> textos;

    [Header("Padre del player para coger objetos")]
    public GameObject cogerObjeto;

    public CanvasCajas canvascajas;

    private bool playerInRange = false;
    private bool openedThisCanvas = false;

    private void Update()
    {
        if (!playerInRange)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        TryOpenIngredientCanvas();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerInRange = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerInRange = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerInRange = false;

        if (openedThisCanvas && canvascajas != null && canvascajas.gameObject.activeSelf)
        {
            canvascajas.CerrarUI();
        }

        openedThisCanvas = false;
    }

    private void TryOpenIngredientCanvas()
    {
        if (canvascajas == null)
        {
            Debug.LogWarning("[Caja] Falta la referencia a CanvasCajas en " + gameObject.name);
            return;
        }

        if (cogerObjeto == null)
        {
            Debug.LogWarning("[Caja] Falta la referencia cogerObjeto en " + gameObject.name);
            return;
        }

        if (cogerObjeto.transform.childCount > 0)
            return;

        if (Ingredientes == null || Ingredientes.Count == 0)
        {
            Debug.LogWarning("[Caja] La caja " + gameObject.name + " no tiene ingredientes asignados.");
            return;
        }

        ConfigureCanvasButtons();

        canvascajas.EspacioInstanciado = cogerObjeto.transform;
        canvascajas.SetTipos(new List<TipoIngrediente>(Ingredientes));
        canvascajas.gameObject.SetActive(true);
        canvascajas.RefreshButtonsState();

        openedThisCanvas = true;
    }

    private void ConfigureCanvasButtons()
    {
        int buttonCount = Botones != null ? Botones.Count : 0;

        for (int i = 0; i < buttonCount; i++)
        {
            Image buttonImage = Botones[i];
            if (buttonImage == null)
                continue;

            bool hasIngredient = Ingredientes != null && i < Ingredientes.Count && Ingredientes[i] != null;
            buttonImage.gameObject.SetActive(hasIngredient);

            if (!hasIngredient)
            {
                SetText(i, string.Empty);
                continue;
            }

            if (ImagenesUI != null && i < ImagenesUI.Count && ImagenesUI[i] != null)
            {
                buttonImage.sprite = ImagenesUI[i];
            }

            SetText(i, Ingredientes[i].name);
        }
    }

    private void SetText(int index, string value)
    {
        if (textos == null)
            return;

        if (index < 0 || index >= textos.Count)
            return;

        if (textos[index] != null)
            textos[index].text = value;
    }
}