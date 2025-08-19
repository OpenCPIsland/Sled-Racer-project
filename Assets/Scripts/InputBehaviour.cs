using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBehaviour : AbstractInputBehaviour
{
    private InputManager manager;
    public bool UseMouse;
    public bool applicationIsOnPC { get; private set; }

    private void Awake()
    {
        manager = new InputManager();
        CalcApplicationIsOnPC();
    }

    private void CalcApplicationIsOnPC()
    {
        bool flag = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;
        bool flag2 = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
        bool flag3 = Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer;
        bool flag4 = Application.platform == RuntimePlatform.WebGLPlayer && !IsMobileWebGL();
        applicationIsOnPC = (flag || flag2 || flag3 || flag4);
    }

    private bool IsMobileWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return (Screen.width <= 800 || Screen.height <= 600);
#else
        return false;
#endif
    }

    public void setInputManager(InputManager inputManager)
    {
        manager = inputManager;
    }

    // MOUSE & KEYBOARD
    public bool MouseDown()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }
    public bool MouseUp()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
    }
    public Vector2 MousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }
    public bool LeftArrow()
    {
        return Keyboard.current != null && (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed);
    }
    public bool RightArrow()
    {
        return Keyboard.current != null && (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed);
    }
    public bool UpArrow()
    {
        return Keyboard.current != null && (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
    }

    // TOUCH
    private Vector2[] GetActiveTouches()
    {
        List<Vector2> activeTouches = new List<Vector2>();
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                    activeTouches.Add(touch.position.ReadValue());
            }
        }
        return activeTouches.ToArray();
    }

    // Example override for jump/right/left if you want to connect them to these input helpers
    internal override bool jump()
    {
        bool keyJump = UpArrow();
        Vector2[] touchPositions = GetActiveTouches();
        return manager.jump(touchPositions, keyJump);
    }
    internal override bool right()
    {
        return RightArrow() || manager.right(GetActiveTouches());
    }
    internal override bool left()
    {
        return LeftArrow() || manager.left(GetActiveTouches());
    }
}
