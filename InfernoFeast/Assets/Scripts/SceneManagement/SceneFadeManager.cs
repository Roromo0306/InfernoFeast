using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;

    [Header("Fade")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.8f;
    public bool useUnscaledTime = false;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            StartCoroutine(Fade(1f, 0f));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (canvasGroup == null)
            return;

        StartCoroutine(Fade(1f, 0f));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (isFading)
            return;

        if (canvasGroup == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        yield return Fade(0f, 1f);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (canvasGroup == null)
            yield break;

        float duration = Mathf.Max(0.01f, fadeDuration);
        float time = 0f;

        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = to > 0f || from > 0f;

        while (time < duration)
        {
            time += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = to > 0f;

        if (to <= 0f)
            isFading = false;
    }
}