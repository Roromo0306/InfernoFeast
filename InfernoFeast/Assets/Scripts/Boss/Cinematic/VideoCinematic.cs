using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoCinematic : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cinematica;
    public DialogueBoss dialogueBoss;

    public AudioSource musicAudioSource;   // AudioSource que reproducirá la música
    public AudioClip backgroundMusic;

    public float fadeDuration = 1f; // Duración del fade en segundos

    private CanvasGroup canvasGroup;

    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;

        // Asegurarse de tener un CanvasGroup para el fade
        canvasGroup = cinematica.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = cinematica.AddComponent<CanvasGroup>();
        }
    }

    void VideoEnd(VideoPlayer vp)
    {
        // Iniciar coroutine para fade antes de ocultar la cinemática
        StartCoroutine(FadeOutCinematic());
    }

    IEnumerator FadeOutCinematic()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        // Asegurarnos de que termine completamente invisible
        canvasGroup.alpha = 0f;
        cinematica.SetActive(false);

        // Empezamos el diálogo
        List<string> dialogo = new List<string>()
        {
            "Bienvenido seas Cedrik!",
            "Espero que disfrutes la experiencia.",
            "¡Demuestra de lo que eres capaz!"
        };

        dialogueBoss.StartDialog(dialogo);

        // 🔹 Reproducir música en loop
        if (musicAudioSource != null && backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.loop = true;
            musicAudioSource.volume = 0.55f;
            musicAudioSource.Play();
        }
    }
}
