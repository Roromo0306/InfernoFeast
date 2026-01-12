using System.Collections;
using UnityEngine;

public class FadeGroupAndDisable : MonoBehaviour
{
    public float fadeDuration = 0.6f;
    private Renderer[] renderers;
    private bool isFading = false;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void FadeOut()
    {
        if (isFading) return;
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        isFading = true;
        float time = 0f;

        Material[][] materials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].materials;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            foreach (var matArray in materials)
                foreach (var mat in matArray)
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }

            time += Time.deltaTime;
            yield return null;
        }

        SetActiveRecursive(gameObject, false);
        isFading = false;
    }

    // 🔹 Reinicia la mesa para la siguiente ronda
    public void ResetFade()
    {
        // Cancelamos cualquier corrutina de fade activa
        StopAllCoroutines();
        isFading = false;

        // Reactivar todos los objetos hijos
        SetActiveRecursive(gameObject, true);

        // Restaurar alpha
        foreach (var rend in renderers)
            foreach (var mat in rend.materials)
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = 1f;
                    mat.color = c;
                }
    }

    // 🔹 Helper para reactivar/desactivar todo el hierarchy
    private void SetActiveRecursive(GameObject obj, bool value)
    {
        obj.SetActive(value);
        foreach (Transform child in obj.transform)
            SetActiveRecursive(child.gameObject, value);
    }
}
