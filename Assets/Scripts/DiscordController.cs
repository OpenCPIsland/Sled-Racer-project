using Discord.Sdk;
using Disney.ClubPenguin.SledRacer;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    private const ulong applicationId = 1333055222687203388UL;

    [SerializeField] private float presenceUpdateInterval = 5f;

    private Discord.Sdk.Client client;
    private bool initialized;

    private static ulong gameStartTimeMs;
    private static MethodInfo runCallbacksStatic;

    private float nextUpdateUnscaled;

    public static DiscordController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!initialized || client == null)
            return;

        TryRunCallbacksIfExposed();

        if (Time.unscaledTime >= nextUpdateUnscaled)
        {
            nextUpdateUnscaled = Time.unscaledTime + Mathf.Max(1f, presenceUpdateInterval);
            RefreshPresence();
        }
    }

    private void OnDestroy()
    {
        Shutdown();

        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void Initialize()
    {
        if (initialized || client != null)
            return;

        if (!IsDiscordRunning())
        {
            UnityEngine.Debug.LogWarning("[DiscordController] Discord is not running. Skipping integration.");
            return;
        }

        try
        {
            client = new Discord.Sdk.Client();
            client.AddLogCallback(OnDiscordLog, LoggingSeverity.Error);
            client.SetStatusChangedCallback(OnStatusChanged);
            client.SetApplicationId(applicationId);

            try
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                client.SetGameWindowPid(pid);
            }
            catch { }

            if (gameStartTimeMs == 0UL)
                gameStartTimeMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            CacheOptionalRunCallbacks();

            nextUpdateUnscaled = Time.unscaledTime + Mathf.Max(1f, presenceUpdateInterval);
            initialized = true;

            RefreshPresence();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[DiscordController] Failed to initialize Discord SDK: " + e.Message);
            client = null;
            initialized = false;
        }
    }

    private void Shutdown()
    {
        if (client != null)
        {
            try { client.ClearRichPresence(); } catch { }
            TryRunCallbacksIfExposed();
            try { client.Disconnect(); } catch { }
            TryRunCallbacksIfExposed();
            try { client.Dispose(); } catch { }
            client = null;
        }

        initialized = false;
    }

    private void RefreshPresence()
    {
        if (client == null || !initialized)
            return;

        UpdatePresence(BuildStateText(), "default_icon", BuildDetailsText(), gameStartTimeMs);
    }

    private string BuildDetailsText()
    {
        int current = GetCurrentScore();
        int best = GetBestScore();

        if (best > 0)
            return string.Format("Score: {0}  |  Best: {1}", current, best);

        return string.Format("Score: {0}", current);
    }

    private string BuildStateText()
    {
        return "Unity " + Application.unityVersion + (IsUnityLTS() ? " LTS" : "");
    }

    private bool IsUnityLTS()
    {
        string[] parts = Application.unityVersion.Split('.');
        if (parts.Length < 2)
            return false;

        if (!int.TryParse(parts[0], out int major) || !int.TryParse(parts[1], out int minor))
            return false;

        if (major >= 6000)
            return minor == 0 || minor == 3 || minor == 7;

        return minor == 3;
    }

    private int GetCurrentScore()
    {
        try
        {
            SledRacerGameManager gm = GameManager.GetInstanceAs<SledRacerGameManager>();
            if (gm != null)
                return gm.getCurrentScore();
        }
        catch { }

        return 0;
    }

    private int GetBestScore()
    {
        try
        {
            PlayerDataService pds = Service.Get<PlayerDataService>();
            if (pds != null)
            {
                int? score = pds.PlayerData.HighScore.Score;
                if (score.HasValue)
                    return score.Value;
            }
        }
        catch { }

        return 0;
    }

    private void UpdatePresence(string state, string imageKey, string details, ulong startTimestampMs)
    {
        if (client == null)
            return;

        if (startTimestampMs == 0UL)
        {
            startTimestampMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            gameStartTimeMs = startTimestampMs;
        }

        Activity activity = new Activity();
        activity.SetType(ActivityTypes.Playing);
        activity.SetState(state);
        activity.SetDetails(details);

        ActivityAssets assets = new ActivityAssets();
        assets.SetLargeImage(imageKey);
        assets.SetLargeText("Sled Racing");
        activity.SetAssets(assets);

        ActivityTimestamps timestamps = new ActivityTimestamps();
        timestamps.SetStart(startTimestampMs);
        activity.SetTimestamps(timestamps);

        client.UpdateRichPresence(activity, OnUpdateRichPresence);
    }

    private void OnUpdateRichPresence(ClientResult result)
    {
        if (!result.Successful())
            UnityEngine.Debug.LogError("[DiscordController] Failed to update rich presence: " + result.Error());
    }

    private void OnDiscordLog(string message, LoggingSeverity severity) { }

    private void OnStatusChanged(Discord.Sdk.Client.Status status, Discord.Sdk.Client.Error error, int errorCode)
    {
        if (error != Discord.Sdk.Client.Error.None)
            UnityEngine.Debug.LogError(string.Format("[DiscordController] Error: {0} (code {1})", error, errorCode));
    }

    private bool IsDiscordRunning()
    {
        try
        {
            return System.Diagnostics.Process.GetProcessesByName("Discord").Any();
        }
        catch
        {
            return false;
        }
    }

    private static void CacheOptionalRunCallbacks()
    {
        if (runCallbacksStatic != null)
            return;

        string[] candidateTypeNames =
        {
            "discordpp",
            "discordpp.discordpp",
            "Discord.Sdk.discordpp",
            "Discord.Sdk.NativeMethods"
        };

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var tn in candidateTypeNames)
            {
                try
                {
                    var t = asm.GetType(tn, false);
                    if (t == null) continue;

                    var mi = t.GetMethod("RunCallbacks",
                        BindingFlags.Public | BindingFlags.Static,
                        null, Type.EmptyTypes, null);

                    if (mi != null)
                    {
                        runCallbacksStatic = mi;
                        return;
                    }
                }
                catch { }
            }
        }
    }

    private static void TryRunCallbacksIfExposed()
    {
        if (runCallbacksStatic == null)
            return;

        try
        {
            runCallbacksStatic.Invoke(null, null);
        }
        catch { }
    }
}
