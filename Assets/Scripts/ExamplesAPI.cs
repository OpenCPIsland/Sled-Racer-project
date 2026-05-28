using Fabric;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExamplesAPI : MonoBehaviour
{
    public int eventID;

    private void Start()
    {
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("Simple");
        }
        else if (kb.digit2Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("Simple", base.gameObject);
        }
        else if (kb.digit3Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("Simple", EventAction.PlaySound, null, base.gameObject);
        }
        else if (kb.digit4Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("Simple", EventAction.StopSound, null, base.gameObject);
        }
        else if (kb.digit5Key.wasPressedThisFrame)
        {
            EventManager.Instance.SetParameter("Timeline", "Parameter", 0.5f, base.gameObject);
        }
        else if (kb.digit6Key.wasPressedThisFrame)
        {
            EventManager.Instance.SetDSPParameter("Event", DSPType.LowPass, "CutoffFrequency", 5000f, 5f, 0.5f, base.gameObject);
        }
        else if (kb.digit7Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("DynamicMixer", EventAction.AddPreset, "MuteAll", null);
        }
        else if (kb.digit8Key.wasPressedThisFrame)
        {
            EventManager.Instance.PostEvent("DynamicMixer", EventAction.RemovePreset, "MuteAll", null);
        }
        else if (kb.digit9Key.wasPressedThisFrame)
        {
            Fabric.Component componentByName = FabricManager.Instance.GetComponentByName("Audio_Fabric_SFX_Test");
            if (componentByName != null)
            {
                componentByName.Volume = 0.5f;
                if (!componentByName.IsPlaying())
                {
                    componentByName.Play(base.gameObject);
                }
            }
        }
        else if (kb.aKey.wasPressedThisFrame)
        {
            FabricManager.Instance.LoadAsset("NameOfPrefab", "Audio_SFX");
        }
        else if (kb.bKey.wasPressedThisFrame)
        {
            FabricManager.Instance.UnloadAsset("Audio_SFX");
        }
        else if (kb.cKey.wasPressedThisFrame)
        {
            InitialiseParameters initialiseParameters = new InitialiseParameters();
            initialiseParameters._pitch.Value = 2f;
            EventManager.Instance.PostEvent("Simple", EventAction.PlaySound, null, base.gameObject, initialiseParameters);
        }
        else if (kb.dKey.wasPressedThisFrame)
        {
            if (EventManager.Instance.IsEventActive("Simple", base.gameObject))
            {
                Debug.Log("Event Simple is Active");
            }
            else
            {
                Debug.Log("Event Simple is Inactive");
            }
        }
        else if (kb.eKey.wasPressedThisFrame)
        {
            Fabric.Component[] componentsByName = FabricManager.Instance.GetComponentsByName("Audio_Simple", base.gameObject);
            if (componentsByName != null && componentsByName.Length > 0)
            {
                componentsByName[0].Volume = 0.5f;
                if (componentsByName[0].IsPlaying())
                {
                    Debug.Log("Component is playing");
                }
            }
        }

        if (kb.fKey.wasPressedThisFrame)
        {
            eventID = EventManager.GetIDFromEventName("Simple");
            EventManager.Instance.PostEvent(eventID, base.gameObject);
        }
    }
}