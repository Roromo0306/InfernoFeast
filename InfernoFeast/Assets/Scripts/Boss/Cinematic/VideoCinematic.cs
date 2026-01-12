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

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;
    }

    // Update is called once per frame
    void VideoEnd(VideoPlayer vp)
    {
        // Ocultamos la cinemática
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
            musicAudioSource.Play();
        }

    }
}
