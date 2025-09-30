using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoCinematic : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cinematica;
    public DialogueBoss dialogueBoss;
    

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;
    }

    // Update is called once per frame
    void VideoEnd(VideoPlayer vp)
    {
        cinematica.SetActive(false);

        List<string> dialogo = new List<string>()
        {
            "Hola, bienvenido al juego.",
            "Espero que disfrutes la aventura.",
            "¡Vamos a comenzar!"
        };

        dialogueBoss.StartDialog(dialogo);
        
    }
}
