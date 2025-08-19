using UnityEngine;

public class InputManager
{
    private class TouchLocations
    {
        public bool left;
        public bool right;
    }

    internal bool jump(Vector2[] touchPositions, bool keyboardJumpPressed)
    {
        TouchLocations touchLocations = getTouchLocations(touchPositions);
        bool touchJump = touchLocations.left && touchLocations.right;
        return keyboardJumpPressed || touchJump;
    }

    internal bool right(Vector3 mousePosition)
    {
        float num = (float)Screen.width / 2f;
        return mousePosition.x - num > 0f;
    }

    internal bool right(float axis)
    {
        return axis > 0f;
    }

    internal bool right(Vector2[] touchPositions)
    {
        TouchLocations touchLocations = getTouchLocations(touchPositions);
        return touchLocations.right;
    }

    internal bool left(Vector3 mousePosition)
    {
        float num = (float)Screen.width / 2f;
        return mousePosition.x - num < 0f;
    }

    internal bool left(float axis)
    {
        return axis < 0f;
    }

    internal bool left(Vector2[] touchPositions)
    {
        TouchLocations touchLocations = getTouchLocations(touchPositions);
        return touchLocations.left;
    }

    private TouchLocations getTouchLocations(Vector2[] touchPositions)
    {
        TouchLocations touchLocations = new TouchLocations();
        float num = (float)Screen.width / 2f;
        for (int i = 0; i < touchPositions.Length; i++)
        {
            Vector2 vector = touchPositions[i];
            float num2 = vector.x - num;
            if (num2 < 0f)
                touchLocations.left = true;
            else
                touchLocations.right = true;
        }
        return touchLocations;
    }
}
