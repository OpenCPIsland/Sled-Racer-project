using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class DayNightColorCycle : MonoBehaviour
{
    [Header("Time Settings (in seconds)")]
    public float dayStart = 0f;
    public float sunriseStart = 30f;
    public float sunsetStart = 60f;
    public float nightStart = 90f;
    public float fullCycle = 120f; // total cycle duration (loop)

    [Header("Colors")]
    public Color dayColor = Color.white;
    public Color sunriseColor = new Color(0.64f, 0.30f, 0.7f); // #A34CB2
    public Color sunsetColor = new Color(0.64f, 0.30f, 0.7f);  // #A34CB2
    public Color nightColor = new Color(0.345f, 0.345f, 0.85f); // #5858D9

    private Image imageComponent;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("No Image component found!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        float currentTime = GetCurrentTimeSeconds();
        imageComponent.color = GetColorForTime(currentTime);
    }

    private float GetCurrentTimeSeconds()
    {
        // Loop continuously over the full cycle
        return Time.time % fullCycle;
    }

    private Color GetColorForTime(float seconds)
    {
        if (seconds >= dayStart && seconds < sunriseStart)
        {
            float t = Mathf.InverseLerp(dayStart, sunriseStart, seconds);
            return Color.Lerp(dayColor, sunriseColor, t);
        }
        else if (seconds >= sunriseStart && seconds < sunsetStart)
        {
            float t = Mathf.InverseLerp(sunriseStart, sunsetStart, seconds);
            return Color.Lerp(sunriseColor, sunsetColor, t);
        }
        else if (seconds >= sunsetStart && seconds < nightStart)
        {
            float t = Mathf.InverseLerp(sunsetStart, nightStart, seconds);
            return Color.Lerp(sunsetColor, nightColor, t);
        }
        else // Night → Sunrise → Day
        {
            float t = Mathf.InverseLerp(nightStart, fullCycle, seconds);
            return Color.Lerp(nightColor, dayColor, t);
        }
    }
}
