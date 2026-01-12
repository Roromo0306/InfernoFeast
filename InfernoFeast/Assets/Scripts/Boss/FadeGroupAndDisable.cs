using System.Collections;
using UnityEngine;

public class FadeGroupAndDisable : MonoBehaviour
{
    public float fadeDuration = 0.6f;

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        float time = 0f;

        // Instanciamos materiales para no afectar a otros objetos
        Material[][] materials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].materials;
        }

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);

            foreach (var matArray in materials)
            {
                foreach (var mat in matArray)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
