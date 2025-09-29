using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoCinematic : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cinematica;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;
    }

    // Update is called once per frame
    void VideoEnd(VideoPlayer vp)
    {
        cinematica.SetActive(false);
    }
}
