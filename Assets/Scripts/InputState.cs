using System.Collections.Generic;
using UnityEngine;

public static class InputState {

    public static int lastScrollWheelConsumptionFrame = -1;
    public static bool IsScrollWheelConsumed => lastScrollWheelConsumptionFrame == Time.frameCount;
    public static bool TryConsumeScrollWheel(out int value) {
        value = ScrollWheel;
        if (value != 0)
            lastScrollWheelConsumptionFrame = Time.frameCount;
        return value != 0;
    }
    public static int ScrollWheel {
        get {
            if (IsScrollWheelConsumed)
                return 0;
            var value = Input.GetAxisRaw("Mouse ScrollWheel");
            return Mathf.Approximately(0, value) ? 0 : Mathf.RoundToInt(Mathf.Sign(value));
        }
    }

    public static Dictionary<KeyCode, int> lastKeyDownConsumptionFrame = new();
    public static bool IsKeyDownConsumed(KeyCode keyCode) {
        return lastKeyDownConsumptionFrame.TryGetValue(keyCode, out var frame) && frame == Time.frameCount;
    }
    public static bool TryConsumeKeyDown(KeyCode keyCode) {
        var isKeyDown = IsKeyDown(keyCode);
        if (isKeyDown)
            lastKeyDownConsumptionFrame[keyCode] = Time.frameCount;
        return isKeyDown;
    }
    public static bool IsKeyDown(KeyCode keyCode) {
        if (IsKeyDownConsumed(keyCode) || !Input.GetKeyDown(keyCode))
            return false;
        // if (consume)
        // ConsumeKeyDown(keyCode);
        return true;
    }

    public static int[] lastMouseDownConsumptionFrame = new int[10];
    public static bool IsMouseButtonDownConsumed(int button) {
        return lastMouseDownConsumptionFrame[button] == Time.frameCount;
    }
    public static bool TryConsumeMouseButtonDown(int button) {
        var isMouseButtonDown = IsMouseButtonDown(button);
        if (isMouseButtonDown)
            lastMouseDownConsumptionFrame[button] = Time.frameCount;
        return isMouseButtonDown;
    }
    public static bool IsMouseButtonDown(int button) {
        if (IsMouseButtonDownConsumed(button) || !Input.GetMouseButtonDown(button))
            return false;
        return true;
    }
}