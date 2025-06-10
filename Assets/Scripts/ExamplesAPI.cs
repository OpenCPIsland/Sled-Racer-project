using Fabric;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ExamplesAPI : MonoBehaviour
{
	public int eventID;

	private void Start()
	{
	}

	private void Update()
	{
#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("Simple");
		}
		else if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("Simple", base.gameObject);
		}
		else if (Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("Simple", EventAction.PlaySound, null, base.gameObject);
		}
		else if (Keyboard.current != null && Keyboard.current.digit4Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("Simple", EventAction.StopSound, null, base.gameObject);
		}
		else if (Keyboard.current != null && Keyboard.current.digit5Key.wasPressedThisFrame)
		{
			EventManager.Instance.SetParameter("Timeline", "Parameter", 0.5f, base.gameObject);
		}
		else if (Keyboard.current != null && Keyboard.current.digit6Key.wasPressedThisFrame)
		{
			EventManager.Instance.SetDSPParameter("Event", DSPType.LowPass, "CutoffFrequency", 5000f, 5f, 0.5f, base.gameObject);
		}
		else if (Keyboard.current != null && Keyboard.current.digit7Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("DynamicMixer", EventAction.AddPreset, "MuteAll", null);
		}
		else if (Keyboard.current != null && Keyboard.current.digit8Key.wasPressedThisFrame)
		{
			EventManager.Instance.PostEvent("DynamicMixer", EventAction.RemovePreset, "MuteAll", null);
		}
		else if (Keyboard.current != null && Keyboard.current.digit9Key.wasPressedThisFrame)
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
		else if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
		{
			FabricManager.Instance.LoadAsset("NameOfPrefab", "Audio_SFX");
		}
		else if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
		{
			FabricManager.Instance.UnloadAsset("Audio_SFX");
		}
		else if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
		{
			InitialiseParameters initialiseParameters = new InitialiseParameters();
			initialiseParameters._pitch.Value = 2f;
			EventManager.Instance.PostEvent("Simple", EventAction.PlaySound, null, base.gameObject, initialiseParameters);
		}
		else if (Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
		{
			if (EventManager.Instance.IsEventActive("Simple", base.gameObject))
			{
				UnityEngine.Debug.Log("Event Simple is Active");
			}
			else
			{
				UnityEngine.Debug.Log("Event Simple is Inactive");
			}
		}
		else if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
		{
			Fabric.Component[] componentsByName = FabricManager.Instance.GetComponentsByName("Audio_Simple", base.gameObject);
			if (componentsByName != null && componentsByName.Length > 0)
			{
				componentsByName[0].Volume = 0.5f;
				if (componentsByName[0].IsPlaying())
				{
					UnityEngine.Debug.Log("Component is playing");
				}
			}
		}
		if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
		{
			eventID = EventManager.GetIDFromEventName("Simple");
			EventManager.Instance.PostEvent(eventID, base.gameObject);
		}
#endif
	}
}