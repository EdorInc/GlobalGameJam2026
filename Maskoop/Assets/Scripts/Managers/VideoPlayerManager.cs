using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public RenderTexture renderTexture;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}
