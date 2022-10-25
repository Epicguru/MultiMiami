using Microsoft.Xna.Framework.Input;
using MM.Core;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MM.Input;

public static class Input
{
    public static int MouseScrollDelta => mouse.ScrollWheelValue - lastMouse.ScrollWheelValue;
    public static int HorizontalMouseScrollDelta => mouse.HorizontalScrollWheelValue - lastMouse.HorizontalScrollWheelValue;
    public static Vector2 WorldMousePosition { get; private set; }
    public static Vector2 ScreenMousePosition { get; private set; }

    private static KeyboardState keyboard, lastKeyboard;
    private static MouseState mouse, lastMouse;

    public static void Update(Camera2D camera)
    {
        lastKeyboard = keyboard;
        keyboard = Keyboard.GetState();

        lastMouse = mouse;
        mouse = Mouse.GetState();

        ScreenMousePosition = mouse.Position.ToVector2();
        if (camera != null)
        {
            WorldMousePosition = camera.GetWorldPosition(ScreenMousePosition);
        }
    }

    public static bool IsJustDown(Keys key) => keyboard.IsKeyDown(key) && lastKeyboard.IsKeyUp(key);
    
    public static bool IsJustUp(Keys key) => keyboard.IsKeyUp(key) && lastKeyboard.IsKeyDown(key);
    
    public static bool IsPressed(Keys key) => keyboard.IsKeyDown(key);

    public static bool IsJustDown(InputBinding binding)
    {
        Debug.Assert(binding != null);

        if (binding.Key2 == Keys.None)
            return IsJustDown(binding.Key);
        return IsPressed(binding.Key) && IsJustDown(binding.Key2);
    }

    public static bool IsJustUp(InputBinding binding)
    {
        Debug.Assert(binding != null);

        if (binding.Key2 == Keys.None)
            return IsJustUp(binding.Key);

        return (IsJustUp(binding.Key) && IsPressed(binding.Key2)) || (IsJustUp(binding.Key2) && IsPressed(binding.Key));
    }

    public static bool IsPressed(InputBinding binding)
    {
        Debug.Assert(binding != null);

        if (binding.Key2 == Keys.None)
            return IsPressed(binding.Key);

        return IsPressed(binding.Key) && IsPressed(binding.Key2);
    }

    public static bool IsJustDown(MouseButton button) => button switch
    {
        MouseButton.None => false,
        MouseButton.Left => mouse.LeftButton == ButtonState.Pressed && lastMouse.LeftButton == ButtonState.Released,
        MouseButton.Middle => mouse.MiddleButton == ButtonState.Pressed && lastMouse.MiddleButton == ButtonState.Released,
        MouseButton.Right => mouse.RightButton == ButtonState.Pressed && lastMouse.RightButton == ButtonState.Released,
        MouseButton.Back => mouse.XButton1 == ButtonState.Pressed && lastMouse.XButton1 == ButtonState.Released,
        MouseButton.Forwards => mouse.XButton2 == ButtonState.Pressed && lastMouse.XButton2 == ButtonState.Released,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
    };

    public static bool IsJustUp(MouseButton button) => button switch
    {
        MouseButton.None => false,
        MouseButton.Left => mouse.LeftButton == ButtonState.Released && lastMouse.LeftButton == ButtonState.Pressed,
        MouseButton.Middle => mouse.MiddleButton == ButtonState.Released && lastMouse.MiddleButton == ButtonState.Pressed,
        MouseButton.Right => mouse.RightButton == ButtonState.Released && lastMouse.RightButton == ButtonState.Pressed,
        MouseButton.Back => mouse.XButton1 == ButtonState.Released && lastMouse.XButton1 == ButtonState.Pressed,
        MouseButton.Forwards => mouse.XButton2 == ButtonState.Released && lastMouse.XButton2 == ButtonState.Pressed,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
    };

    public static bool IsPressed(MouseButton button) => button switch
    {
        MouseButton.None => false,
        MouseButton.Left => mouse.LeftButton == ButtonState.Pressed,
        MouseButton.Middle => mouse.MiddleButton == ButtonState.Pressed,
        MouseButton.Right => mouse.RightButton == ButtonState.Pressed,
        MouseButton.Back => mouse.XButton1 == ButtonState.Pressed,
        MouseButton.Forwards => mouse.XButton2 == ButtonState.Pressed,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
    };
}