using Microsoft.Xna.Framework;
using MM.Core.Structures;
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

    /// <summary>
    /// Gets the world-space bounds of the camera's view.
    /// </summary>
    public RectF CameraBounds => camBounds;

    private Vector2 position;
    private float scale = 1f;
    private float rotation;

    private Matrix matrix, matrixNoOffset, matrixInverse, matrixNoOffsetInverse;
    private Vector2 lastScreenSize;
    private bool isDirty = true;
    private RectF camBounds;

    public Matrix GetMatrix() => GetMatrix(Screen.ScreenSize.ToVector2());

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
        matrixNoOffset = Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(scale);
        Matrix.Invert(ref matrix, out matrixInverse);
        Matrix.Invert(ref matrixNoOffset, out matrixNoOffsetInverse);

        camBounds.Encompass(GetScreenCorners(screenSize), true);

        return matrix;
    }

    private IEnumerable<Vector2> GetScreenCorners(Vector2 screenSize)
    {
        yield return GetWorldPosition(Vector2.Zero);
        yield return GetWorldPosition(new Vector2(screenSize.X, 0));
        yield return GetWorldPosition(new Vector2(0, screenSize.Y));
        yield return GetWorldPosition(new Vector2(screenSize.X, screenSize.Y));
    }

    public Vector2 GetScreenPosition(Vector2 worldPosition)
    {
        GetMatrix();
        Vector2.Transform(ref worldPosition, ref matrix, out var result);
        return result;
    }

    public Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        GetMatrix();
        Vector2.Transform(ref screenPosition, ref matrixInverse, out var result);
        return result;
    }

    public Vector2 GetWorldVector(Vector2 screenVector)
    {
        GetMatrix();
        Vector2.Transform(ref screenVector, ref matrixNoOffsetInverse, out var result);
        return result;
    }
}
