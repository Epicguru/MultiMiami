using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace MM.Core;

/// <summary>
/// Represents a 2D orthographic camera.
/// </summary>
public class Camera2D
{
    /// <summary>
    /// The camera position in world space.
    /// </summary>
    public Vector2 Position
    {
        get => position;
        set
        {
            position = value;
            isDirty = true;
        }
    }

    /// <summary>
    /// The scale (zoom) of the camera. Values higher than 1 zoom in, values less than 1 zoom out.
    /// </summary>
    public float Scale
    {
        get => scale;
        set
        {
            if (value == 0f)
                value = 0.001f;
            scale = value;
            isDirty = true;
        }
    }

    /// <summary>
    /// The angle of rotation of the camera, measured in radians.
    /// </summary>
    public float Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            isDirty = true;
        }
    }

    private Vector2 position;
    private float scale = 1f;
    private float rotation;

    private Matrix matrix;
    private Vector2 lastScreenSize;
    private bool isDirty = true;

    public Matrix GetMatrix(Vector2 screenSize)
    {
        Debug.Assert(screenSize.X != 0);
        Debug.Assert(screenSize.Y != 0);

        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            isDirty = true;
        }

        if (!isDirty)
            return matrix;

        isDirty = false;
        matrix = Matrix.CreateTranslation(-position.X, -position.Y, 0) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(screenSize.X * (0.5f / scale), screenSize.Y * (0.5f / scale), 0f) * Matrix.CreateScale(scale);
        return matrix;
    }
}
