using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;

    [Header("Fade")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.8f;

    bool isFading = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Siempre empezamos con Fade IN
        canvasGroup.alpha = 1f;
        StartCoroutine(FadeIn());
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        // Fade OUT
        yield return Fade(0f, 1f);

        // Cargar escena
        SceneManager.LoadScene(sceneName);

        yield return null; // esperar 1 frame

        // Fade IN
        yield return Fade(1f, 0f);

        isFading = false;
    }

    IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        float time = 0f;
        canvasGroup.alpha = from;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}

