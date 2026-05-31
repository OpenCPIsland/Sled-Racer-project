using System.Collections;
using Fabric;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using Disney.ClubPenguin.SledRacer;

public class IntroVideoPlayer : MonoBehaviour
{
    private const string VideoResourcePath = "IntroVideo";
    private const string MusicEventName = "MUS_Main";

    private VideoPlayer _videoPlayer;
    private AudioSource _audioSource;
    private RawImage _videoDisplay;
    private bool _skipRequested;
    private bool _videoFinished;
    private RenderTexture _rt;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
    }

    private void Start()
    {
        BuildUI();
        StartCoroutine(PlayIntroVideo());
    }

    private void Update()
    {
        if (_videoFinished) return;
        if (_skipRequested) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            RequestSkip();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RequestSkip();
            return;
        }

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    RequestSkip();
                    return;
                }
            }
        }
    }

    private void RequestSkip()
    {
        _skipRequested = true;
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("IntroVideoCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject displayGO = new GameObject("VideoDisplay");
        displayGO.transform.SetParent(canvasGO.transform, false);
        _videoDisplay = displayGO.AddComponent<RawImage>();
        _videoDisplay.color = Color.white;

        AspectRatioFitter fitter = displayGO.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 16f / 9f;

        RectTransform displayRT = displayGO.GetComponent<RectTransform>();
        displayRT.anchorMin = Vector2.zero;
        displayRT.anchorMax = Vector2.one;
        displayRT.offsetMin = Vector2.zero;
        displayRT.offsetMax = Vector2.zero;
    }

    private IEnumerator PlayIntroVideo()
    {
        ResourceRequest loadRequest = Resources.LoadAsync<VideoClip>(VideoResourcePath);
        yield return loadRequest;

        VideoClip clip = loadRequest.asset as VideoClip;

        if (clip == null)
        {
            Destroy(gameObject);
            yield break;
        }

        _rt = new RenderTexture((int)clip.width, (int)clip.height, 0);

        AspectRatioFitter fitter = _videoDisplay.GetComponent<AspectRatioFitter>();
        if (fitter != null && clip.height > 0)
            fitter.aspectRatio = (float)clip.width / clip.height;

        _videoDisplay.texture = _rt;

        _videoPlayer.clip = clip;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _rt;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _videoPlayer.SetTargetAudioSource(0, _audioSource);
        _videoPlayer.isLooping = false;
        _videoPlayer.skipOnDrop = true;
        _videoPlayer.loopPointReached += OnVideoFinished;
        _videoPlayer.prepareCompleted += OnPrepareCompleted;

        _videoPlayer.Prepare();

        while (!_videoPlayer.isPrepared)
        {
            if (_skipRequested) goto Cleanup;
            yield return null;
        }

        while (!_videoFinished && !_skipRequested)
        {
            yield return null;
        }

    Cleanup:
        _videoPlayer.Stop();

        StopMainMenuMusic();
        StartMainMenuMusic();

        _videoDisplay.texture = null;
        _rt.Release();
        Destroy(_rt);

        Destroy(gameObject);
    }

    private void OnPrepareCompleted(VideoPlayer vp)
    {
        StopMainMenuMusic();
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        _videoFinished = true;
    }

    private void StopMainMenuMusic()
    {
        try
        {
            EventManager.Instance.PostEvent(MusicEventName, EventAction.StopSound);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[IntroVideoPlayer] Could not stop music: " + e.Message);
        }
    }

    private void StartMainMenuMusic()
    {
        try
        {
            EventManager.Instance.PostEvent(MusicEventName, EventAction.StopSound);
            EventManager.Instance.PostEvent(MusicEventName, EventAction.SetSwitch, MusicTrack.MainMenu.ToString());
            EventManager.Instance.PostEvent(MusicEventName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[IntroVideoPlayer] Could not start music: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
            _videoPlayer.prepareCompleted -= OnPrepareCompleted;
        }

        if (_rt != null)
        {
            _rt.Release();
            Destroy(_rt);
        }
    }
}