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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Empezamos siempre en negro
        canvasGroup.alpha = 1f;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que se carga una escena → Fade IN
        StartCoroutine(Fade(1f, 0f));
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

        isFading = false;
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


