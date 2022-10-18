namespace MM.Core;

public static class Mathf
{
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;
}