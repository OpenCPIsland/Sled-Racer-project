using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputBehaviour : AbstractInputBehaviour
{
    private InputManager manager;

    public bool UseMouse;

    public bool applicationIsOnPC
    {
        private get;
        set;
    }

    private InputAction horizontalAction;
    private InputAction verticalAction;

    private void Awake()
    {
        manager = new InputManager();
        CalcApplicationIsOnPC();
        EnhancedTouchSupport.Enable();

        verticalAction = new InputAction("Vertical", type: InputActionType.Value, expectedControlType: "Axis");
        verticalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/s")
            .With("Positive", "<Keyboard>/w");
        verticalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/downArrow")
            .With("Positive", "<Keyboard>/upArrow");
        verticalAction.Enable();

        horizontalAction = new InputAction("Horizontal", type: InputActionType.Value, expectedControlType: "Axis");
        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        horizontalAction.Enable();
    }

    private void OnDestroy()
    {
        verticalAction?.Disable();
        verticalAction?.Dispose();
        horizontalAction?.Disable();
        horizontalAction?.Dispose();
        EnhancedTouchSupport.Disable();
    }

    private void CalcApplicationIsOnPC()
    {
        bool flag = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WebGLPlayer;
        bool flag2 = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
        bool flag3 = Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer;
        applicationIsOnPC = (flag || flag2 || flag3) && SystemInfo.deviceType != DeviceType.Handheld;
    }

    public void setInputManager(InputManager inputManager)
    {
        manager = inputManager;
    }

    internal override bool jump()
    {
        if (applicationIsOnPC)
        {
            return manager.jump(new Vector2[0], verticalAction.ReadValue<float>() != 0f);
        }
        return manager.jump(getTouchPositions(), false);
    }

    internal override bool right()
    {
        if (applicationIsOnPC)
        {
            if (UseMouse)
            {
                if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                {
                    return manager.right((Vector3)Mouse.current.position.ReadValue());
                }
                return false;
            }
            return manager.right(horizontalAction.ReadValue<float>());
        }
        return manager.right(getTouchPositions());
    }

    internal override bool left()
    {
        if (applicationIsOnPC)
        {
            if (UseMouse)
            {
                if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                {
                    return manager.left((Vector3)Mouse.current.position.ReadValue());
                }
                return false;
            }
            return manager.left(horizontalAction.ReadValue<float>());
        }
        return manager.left(getTouchPositions());
    }

    private Vector2[] getTouchPositions()
    {
        List<Vector2> list = new List<Vector2>();
        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Ended &&
                touch.phase != UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                list.Add(touch.screenPosition);
            }
        }
        return list.ToArray();
    }
}