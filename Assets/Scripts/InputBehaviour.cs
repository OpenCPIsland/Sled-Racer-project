using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class InputBehaviour : AbstractInputBehaviour
{
    private InputManager manager;
    public bool UseMouse;
    public bool applicationIsOnPC
    {
        private get;
        set;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int GetTouchCount();
    [DllImport("__Internal")]
    private static extern float GetTouchX(int index);
    [DllImport("__Internal")]
    private static extern float GetTouchY(int index);
#endif

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

    internal override bool jump()
    {
        if (applicationIsOnPC)
        {
            return manager.jump(UnityEngine.Input.GetAxis("Vertical"));
        }
        return manager.jump(getTouchPositions());
    }

    internal override bool right()
    {
        if (applicationIsOnPC)
        {
            if (UseMouse)
            {
                if (Input.GetMouseButton(0))
                {
                    return manager.right(UnityEngine.Input.mousePosition);
                }
                return false;
            }
            return manager.right(UnityEngine.Input.GetAxis("Horizontal"));
        }
        return manager.right(getTouchPositions());
    }

    internal override bool left()
    {
        if (applicationIsOnPC)
        {
            if (UseMouse)
            {
                if (Input.GetMouseButton(0))
                {
                    return manager.left(UnityEngine.Input.mousePosition);
                }
                return false;
            }
            return manager.left(UnityEngine.Input.GetAxis("Horizontal"));
        }
        return manager.left(getTouchPositions());
    }

    private Vector2[] getTouchPositions()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        int count = GetTouchCount();
        List<Vector2> list = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            float x = GetTouchX(i);
            float y = GetTouchY(i);
            list.Add(new Vector2(x, y));
        }
        return list.ToArray();
#else
        List<Vector2> list = new List<Vector2>();
        if ((float)Input.touches.Length > 0f)
        {
            Touch[] touches = Input.touches;
            for (int i = 0; i < touches.Length; i++)
            {
                Touch touch = touches[i];
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    list.Add(touch.position);
                }
            }
        }
        return list.ToArray();
#endif
    }
}