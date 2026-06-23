using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Manages playing an intro video clip once on scene load,
/// then transitioning to a loopable background video clip.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class MenuVideoManager : MonoBehaviour
{
    [Header("Video Clips")]
    [Tooltip("The intro video to play once when the menu loads.")]
    public VideoClip introClip;
    [Tooltip("The main video clip to loop continuously after the intro finishes.")]
    public VideoClip loopClip;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (videoPlayer == null) return;

        // Register callback for when the video reaches the end
        videoPlayer.loopPointReached += OnVideoFinished;

        // Start playing the intro clip if assigned, otherwise fallback to loop clip
        if (introClip != null)
        {
            videoPlayer.clip = introClip;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
        }
        else if (loopClip != null)
        {
            PlayLoopClip();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // If we just finished the intro clip, transition to the loop clip
        if (vp.clip == introClip && loopClip != null)
        {
            // Unsubscribe to prevent infinite calls since the next clip will loop
            videoPlayer.loopPointReached -= OnVideoFinished;
            PlayLoopClip();
        }
    }

    private void PlayLoopClip()
    {
        videoPlayer.clip = loopClip;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
